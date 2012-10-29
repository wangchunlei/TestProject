namespace LINQPad.Extensibility.DataContext
{
    using LINQPad;
    using Microsoft.CSharp;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Services.Client;
    using System.Data.Services.Design;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;

    internal static class AstoriaHelper
    {
        private const string DesignDllErrorMessage = "Cannot load System.Data.Services.Design.dll. (A possible cause is installing the .NET Framework Client Profile instead of the full .NET Framework.)";

        private static void BuildAssembly(string code, AssemblyName name)
        {
            CompilerResults results;
            string str = "v4.0";
            string str2 = "";
            Dictionary<string, string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", str);
            using (CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions))
            {
                CompilerParameters options = new CompilerParameters(("System.dll System.Core.dll System.Xml.dll " + str2 + "System.Data.Services.Client.dll").Split(new char[0]), name.CodeBase, true);
                results = provider.CompileAssemblyFromSource(options, new string[] { code });
            }
            if (results.Errors.Count > 0)
            {
                string msg = string.Concat(new object[] { "Cannot compile typed context: ", results.Errors[0].ErrorText, " (line ", results.Errors[0].Line, ")" });
                if (results.Errors[0].ErrorNumber == "CS0513")
                {
                    msg = msg + "\r\nHave you forgotten to call SetEntitySetAccessRule(...) and SetServiceOperationAccessRule(...) in your Data Service?";
                }
                throw new DisplayToUserException(msg);
            }
        }

        private static string GenerateCode(XmlReader reader, string nameSpace)
        {
            EntityClassGenerator generator = new EntityClassGenerator(LanguageOption.GenerateCSharpCode) {
                Version = DataServiceCodeVersion.V2
            };
            StringWriter targetWriter = new StringWriter();
            try
            {
                IList<EdmSchemaError> list = generator.GenerateCode(reader, targetWriter, nameSpace);
                if (list.Count > 0)
                {
                    throw new DisplayToUserException(string.Concat(new object[] { "Bad schema: ", list[0].Message, " (line ", list[0].Line, ")" }));
                }
            }
            catch (MetadataException exception)
            {
                throw new DisplayToUserException("MetadataException: " + exception.Message);
            }
            catch (XmlSchemaValidationException exception2)
            {
                throw new DisplayToUserException("This schema is unsupported.\r\n\r\nEntityClassGenerator returned the following error: " + exception2.Message);
            }
            catch (FileNotFoundException exception3)
            {
                if (exception3.Message.Contains("System.Data.Services.Design"))
                {
                    throw new DisplayToUserException("Cannot load System.Data.Services.Design.dll. (A possible cause is installing only the .NET Framework Client Profile instead of the full .NET Framework.)");
                }
                throw;
            }
            return targetWriter.ToString();
        }

        private static List<ExplorerItem> GetSchema(XDocument data, out string typeName, out string nameSpace)
        {
            return new AstoriaSchemaReader(data).GetSchema(out typeName, out nameSpace);
        }

        internal static List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo r, AssemblyName name, ref string nameSpace, ref string typeName)
        {
            XmlReader reader;
            XDocument document;
            string str;
            string str2;
            try
            {
                using (new PushDefaultWebProxy())
                {
                    using (reader = GetSchemaReader(r))
                    {
                        document = XDocument.Load(reader);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception, "Astoria GetSchema");
                throw new DisplayToUserException(exception.Message);
            }
            try
            {
                using (reader = document.CreateReader())
                {
                    str = GenerateCode(reader, nameSpace);
                }
            }
            catch (FileNotFoundException exception2)
            {
                if (exception2.Message.Contains("System.Data.Services.Design"))
                {
                    throw new DisplayToUserException("Cannot load System.Data.Services.Design.dll. (A possible cause is installing the .NET Framework Client Profile instead of the full .NET Framework.)");
                }
                throw;
            }
            BuildAssembly(str, name);
            List<ExplorerItem> list = GetSchema(document, out typeName, out str2);
            if (!(!Regex.IsMatch(str, @"^\s*namespace\s+" + nameSpace + "." + str2 + @"\b", RegexOptions.Multiline) || Regex.IsMatch(str, @"^\s*namespace\s+" + nameSpace + @"\b", RegexOptions.Multiline)))
            {
                nameSpace = nameSpace + "." + str2;
            }
            return list;
        }

        private static XmlReader GetSchemaReader(IConnectionInfo r)
        {
            Uri metadataUri = new DataServiceContext(new Uri(r.DatabaseInfo.Server)).GetMetadataUri();
            XmlReaderSettings settings = new XmlReaderSettings();
            XmlResolver resolver = new XmlUrlResolver();
            if (r.DatabaseInfo.UserName.Length > 0)
            {
                resolver.Credentials = new NetworkCredential(r.DatabaseInfo.UserName, r.DatabaseInfo.Password);
            }
            else
            {
                resolver.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            settings.XmlResolver = resolver;
            return XmlReader.Create(metadataUri.ToString(), settings);
        }

        internal static void InitializeContext(IConnectionInfo r, object context, bool dallas)
        {
            DataServiceContext context2 = (DataServiceContext) context;
            context2.ResolveType = name => Type.GetType("LINQPad.User." + name.Split(new char[] { '.' }).Last<string>());
            if (dallas)
            {
                string str = (string) r.DriverData.Element("AccountKey");
                if (!string.IsNullOrEmpty(str))
                {
                    str = r.Decrypt(str);
                }
                if (!string.IsNullOrEmpty(str))
                {
                    context2.Credentials = new NetworkCredential("accountkey", str);
                }
            }
            else if (r.DatabaseInfo.UserName.Length > 0)
            {
                context2.Credentials = new NetworkCredential(r.DatabaseInfo.UserName, r.DatabaseInfo.Password);
            }
            else
            {
                context2.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            context2.SendingRequest += delegate (object sender, SendingRequestEventArgs e) {
                WebProxy webProxy = Util.GetWebProxy();
                if (webProxy != null)
                {
                    e.Request.Proxy = webProxy;
                }
                DataContextBase.SqlLog.WriteLine(e.Request.RequestUri);
            };
        }

        internal static void PreprocessObjectToWrite(ref object objectToWrite, ObjectGraphInfo info)
        {
            if (objectToWrite is DataServiceQuery)
            {
                objectToWrite = ((DataServiceQuery) objectToWrite).Execute();
            }
            if (objectToWrite is QueryOperationResponse)
            {
                QueryOperationResponse qor = (QueryOperationResponse) objectToWrite;
                if (qor.GetType().IsGenericType && (qor.GetType().GetGenericTypeDefinition() == typeof(QueryOperationResponse<>)))
                {
                    objectToWrite = Util.VerticalRun(new object[] { new QueryOperationResponseWrapper(true, qor), new QueryOperationResponseWrapper(false, qor) });
                }
            }
            else if (objectToWrite is QueryOperationResponseWrapper)
            {
                QueryOperationResponseWrapper wrapper = (QueryOperationResponseWrapper) objectToWrite;
                if (wrapper.Enumerate)
                {
                    objectToWrite = wrapper.Qor;
                }
                else
                {
                    DataServiceQueryContinuation continuation = wrapper.Qor.GetContinuation();
                    if (!((continuation == null) || wrapper.ElementType.Name.Contains<char>('<')))
                    {
                        Uri nextLinkUri = continuation.NextLinkUri;
                        objectToWrite = new Hyperlinq(QueryLanguage.Expression, "Execute<" + wrapper.ElementType.Name + "> (new Uri (\"" + nextLinkUri.ToString() + "\"))", "Next Page");
                    }
                    else
                    {
                        objectToWrite = info.DisplayNothingToken;
                    }
                }
            }
            else
            {
                DataServiceQueryException exception = objectToWrite as DataServiceQueryException;
                if ((exception != null) && (exception.InnerException is DataServiceClientException))
                {
                    DataServiceClientException innerException = (DataServiceClientException) exception.InnerException;
                    try
                    {
                        XElement element = XElement.Parse(innerException.Message);
                        if (element.Name.LocalName == "error")
                        {
                            XNamespace namespace2 = element.Name.Namespace;
                            string str = (string) element.Element((XName) (namespace2 + "message"));
                            if (!string.IsNullOrEmpty(str))
                            {
                                str = str.Trim();
                                if (str.EndsWith("cannot be used in a query"))
                                {
                                    str = str + " predicate";
                                }
                                if (!str.EndsWith("."))
                                {
                                    str = str + ".";
                                }
                                Util.Highlight(str + " See exception below for more details.").Dump<object>();
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal static string TestConnection(IConnectionInfo r)
        {
            try
            {
                return TestConnectionInternal(r);
            }
            catch (Exception exception)
            {
                string str2 = exception.GetType().Name + ": " + exception.Message;
                if (exception is XmlSchemaValidationException)
                {
                    str2 = "This schema is unsupported - EntityClassGenerator returned the following error: " + str2;
                }
                if ((exception is FileNotFoundException) && exception.Message.Contains("System.Data.Services.Design"))
                {
                    str2 = "Cannot load System.Data.Services.Design.dll. (A possible cause is installing the .NET Framework Client Profile instead of the full .NET Framework.)";
                }
                return str2;
            }
        }

        internal static string TestConnectionInternal(IConnectionInfo r)
        {
            IList<EdmSchemaError> list;
            EntityClassGenerator generator = new EntityClassGenerator(LanguageOption.GenerateCSharpCode) {
                Version = DataServiceCodeVersion.V2
            };
            StringWriter targetWriter = new StringWriter();
            using (new PushDefaultWebProxy())
            {
                using (XmlReader reader = GetSchemaReader(r))
                {
                    list = generator.GenerateCode(reader, targetWriter, null);
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            return list.First<EdmSchemaError>().Message;
        }

        private class QueryOperationResponseWrapper
        {
            public readonly bool Enumerate;
            public readonly QueryOperationResponse Qor;

            public QueryOperationResponseWrapper(bool enumerate, QueryOperationResponse qor)
            {
                this.Enumerate = enumerate;
                this.Qor = qor;
            }

            public Type ElementType
            {
                get
                {
                    return this.Qor.GetType().GetGenericArguments()[0];
                }
            }
        }
    }
}

