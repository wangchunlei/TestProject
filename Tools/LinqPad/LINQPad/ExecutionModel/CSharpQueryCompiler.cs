namespace LINQPad.ExecutionModel
{
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.CSharp;
    using ActiproSoftware.SyntaxEditor.Addons.DotNet.Ast;
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI;
    using Microsoft.CSharp;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    internal class CSharpQueryCompiler : QueryCompiler
    {
        public static readonly string[] Keywords;
        private const string NoNestString = "#define NONEST";
        private const string NoOptimizeString = "#LINQPad /optimize-";

        static CSharpQueryCompiler()
        {
            Keywords = (from w in "abstract\r\nbyte\r\nclass\r\ndelegate\r\nevent\r\nfixed\r\nif\r\ninternal\r\nnew\r\noverride\r\nreadonly\r\nshort\r\nstruct\r\ntry\r\nunsafe\r\nwhile\r\nas\r\ncase\r\nconst\r\ndo\r\nexplicit\r\nfloat\r\nimplicit\r\nis\r\nnull\r\nparams\r\nref\r\nsizeof\r\nswitch\r\ntypeof\r\nushort\r\nbase\r\ncatch\r\ncontinue\r\ndouble\r\nextern\r\nfor\r\nin\r\nlock\r\nobject\r\nprivate\r\nreturn\r\nstackalloc\r\nthis\r\nuint\r\nusing\r\nbool\r\nchar\r\ndecimal\r\nelse\r\nfalse\r\nforeach\r\nint\r\nlong\r\noperator\r\nprotected\r\nsbyte\r\nstatic\r\nthrow\r\nulong\r\nvirtual\r\nbreak\r\nchecked\r\ndefault\r\nenum\r\nfinally\r\ngoto\r\ninterface\r\nnamespace\r\nout\r\npublic\r\nsealed\r\nstring\r\ntrue\r\nunchecked\r\nvolatile\r\nvoid".Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                orderby w
                select w).ToArray<string>();
        }

        public CSharpQueryCompiler(QueryCore query, bool addReferences) : base(query, addReferences)
        {
        }

        public override bool Compile(string queryText, QueryCore query, TempFileRef dataContextAssembly)
        {
            string str = "";
            if (queryText.Contains("#LINQPad /optimize-"))
            {
                queryText = queryText.Replace("#LINQPad /optimize-", "".PadRight("#LINQPad /optimize-".Length));
            }
            bool flag = queryText.Contains("#define NONEST");
            if (queryText.Contains("#define"))
            {
                string str2 = queryText.Replace("\r\n", "\n");
                int preprocessorDirectiveEndOffset = this.GetPreprocessorDirectiveEndOffset(str2);
                if (preprocessorDirectiveEndOffset > 0)
                {
                    str = str2.Substring(0, preprocessorDirectiveEndOffset) + "\n";
                    queryText = str2.Substring(preprocessorDirectiveEndOffset);
                }
            }
            if (queryText.Contains("Util.DisplayControl"))
            {
            }
            if (!((CS$<>9__CachedAnonymousMethodDelegate13 != null) || base.References.Any<string>(CS$<>9__CachedAnonymousMethodDelegate13)))
            {
                base.References.Add("System.Windows.Forms.dll");
            }
            string header = this.GetHeader(query);
            base.LineOffset = header.Count<char>(c => c == '\n');
            if (str.Length > 0)
            {
                base.LineOffset++;
            }
            StringBuilder builder = new StringBuilder(str + header);
            if (queryText.Trim().Length == 0)
            {
                queryText = "\"\"";
            }
            builder.AppendLine(queryText + "\r\n");
            builder.Append(this.GetFooter(query));
            if (base.IsMyExtensions || flag)
            {
                builder = new StringBuilder(this.ExtractNestedTypes(builder.ToString(), false));
            }
            string str4 = "v4.0";
            Dictionary<string, string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", str4);
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);
            string compilerOptions = this.GetCompilerOptions(query);
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool flag2 = base.Compile(codeProvider, builder.ToString(), dataContextAssembly, query.QueryKind, compilerOptions);
            stopwatch.Stop();
            if (!flag2)
            {
                IEnumerable<CompilerError> source = base.Errors.Cast<CompilerError>().Where<CompilerError>(delegate (CompilerError e) {
                    if (e.ErrorNumber == "CS0006")
                    {
                    }
                    return (CS$<>9__CachedAnonymousMethodDelegate1a == null) && (e.ErrorText.Count<char>(CS$<>9__CachedAnonymousMethodDelegate1a) == 2);
                });
                if (source.Any<CompilerError>())
                {
                    List<CompilerError> list = new List<CompilerError>();
                    foreach (CompilerError error in source)
                    {
                        string filename = error.ErrorText.Substring(error.ErrorText.IndexOf('\'') + 1);
                        try
                        {
                            filename = Path.GetFileName(filename.Substring(0, filename.IndexOf('\'')));
                        }
                        catch
                        {
                            continue;
                        }
                        string key = base.References.FirstOrDefault<string>(r => Path.GetFileName(r).Equals(filename, StringComparison.InvariantCultureIgnoreCase));
                        if (key != null)
                        {
                            base.References.Remove(key);
                            list.Add(error);
                        }
                    }
                    if (list.Any<CompilerError>())
                    {
                        if (flag2 = base.Compile(codeProvider, builder.ToString(), dataContextAssembly, query.QueryKind, compilerOptions))
                        {
                            list.Reverse();
                            foreach (CompilerError error in list)
                            {
                                error.IsWarning = true;
                                base.Errors.Insert(0, error);
                            }
                        }
                        if (!flag2)
                        {
                            string errorMessage = base.ErrorMessage;
                            string[] strArray = new string[] { errorMessage, "\r\n", (list.Count == 1) ? "There was also an assembly reference error" : "There were also assembly reference errors", " - press F4 to fix:\r\n", string.Join("\r\n", (from s in list select "   " + s.ErrorText).ToArray<string>()) };
                            base.ErrorMessage = string.Concat(strArray);
                        }
                    }
                }
            }
            if (!flag2 && (query.QueryKind == QueryLanguage.Statements))
            {
            }
            if ((CS$<>9__CachedAnonymousMethodDelegate17 == null) && base.Errors.Cast<CompilerError>().Any<CompilerError>(CS$<>9__CachedAnonymousMethodDelegate17))
            {
                builder.Replace("void RunUserAuthoredQuery()", "async System.Threading.Tasks.Task RunUserAuthoredQuery()", 0, (str + header).Length);
                flag2 = base.Compile(codeProvider, builder.ToString(), dataContextAssembly, query.QueryKind, compilerOptions);
            }
            if ((!flag2 && (base.ErrorNumber == "CS0012")) && base.ErrorMessage.Contains("'System.Windows.Forms,"))
            {
                base.References.Add("System.Windows.Forms.dll");
                flag2 = base.Compile(codeProvider, builder.ToString(), dataContextAssembly, query.QueryKind, compilerOptions);
            }
            if (((!flag2 && (base.ErrorNumber == "CS1502")) && (query.QueryKind == QueryLanguage.Expression)) && base.ErrorMessage.Contains("cannot convert from 'method group' to 'object'"))
            {
                base.ErrorMessage = "Cannot convert from a method group to an expression. Try adding '()' after the method name.";
                return false;
            }
            if ((!flag2 && !flag) && !base.IsMyExtensions)
            {
            }
            if ((CS$<>9__CachedAnonymousMethodDelegate18 == null) && base.Errors.Cast<CompilerError>().Any<CompilerError>(CS$<>9__CachedAnonymousMethodDelegate18))
            {
                string str8 = this.ExtractNestedTypes(builder.ToString(), true);
                if (str8 != null)
                {
                    if (!(flag2 = base.Compile(codeProvider, str8, dataContextAssembly, query.QueryKind, compilerOptions)))
                    {
                    }
                    if ((CS$<>9__CachedAnonymousMethodDelegate19 == null) && base.Errors.Cast<CompilerError>().Any<CompilerError>(CS$<>9__CachedAnonymousMethodDelegate19))
                    {
                        str8 = this.ExtractNestedTypes(builder.ToString(), false);
                        if (str8 != null)
                        {
                            flag2 = base.Compile(codeProvider, str8, dataContextAssembly, query.QueryKind, compilerOptions);
                        }
                    }
                }
            }
            if (!flag2)
            {
                if ((query.QueryKind == QueryLanguage.Expression) && (base.ErrorMessage == ") expected"))
                {
                    base.ErrorMessage = ") or end of expression expected";
                    if (queryText.Contains<char>(';'))
                    {
                        base.ErrorMessage = base.ErrorMessage + " (change the Query Language to 'C# Statements' for multi-statement queries)";
                    }
                }
                else if (base.ErrorNumber == "CS0012")
                {
                    base.ErrorMessage = base.ErrorMessage + " (Press F4)";
                }
                else
                {
                    base.ErrorMessage = base.ErrorMessage.Replace("are you missing a using directive or an assembly reference?", "press F4 to add a using directive or assembly reference");
                }
                if (base.ErrorMessage.Contains("PredicateBuilder"))
                {
                    base.ErrorMessage = base.ErrorMessage + QueryCompiler.PredicateBuilderMessage;
                }
            }
            return flag2;
        }

        private string ExtractNestedTypes(string source, bool staticOnly)
        {
            Func<TypeDeclaration, bool> predicate = null;
            Func<TypeDeclaration, TextRange> selector = null;
            Document document = new Document();
            document.set_Language(new CSharpSyntaxLanguage());
            document.set_Text(source);
            IAstNode root = null;
            TextRange[] rangeArray = null;
            ClassDeclaration declaration = null;
            for (int i = 0; i < 50; i++)
            {
                if (i > 0)
                {
                    Thread.Sleep(50);
                }
                root = document.get_SemanticParseData() as IAstNode;
                if (root != null)
                {
                    declaration = root.get_ChildNodes().OfType<ClassDeclaration>().FirstOrDefault<ClassDeclaration>(c => c.get_FullName() == "UserQuery");
                    if (declaration != null)
                    {
                        if (i < 3)
                        {
                            i = 0x2f;
                        }
                        else
                        {
                            i = 0x2d;
                        }
                        if (predicate == null)
                        {
                            predicate = m => !staticOnly || ((m is ClassDeclaration) && ((m.get_Modifiers() & 0x100000) == 0x100000));
                        }
                        if (selector == null)
                        {
                            selector = m => (((m.get_AttributeSections() == null) || (m.get_AttributeSections().Count == 0)) || ((m.get_AttributeSections().get_Item(0).get_StartOffset() < 0) || (m.get_AttributeSections().get_Item(0).get_StartOffset() <= root.get_StartOffset()))) ? m.get_TextRange() : new TextRange(m.get_AttributeSections().get_Item(0).get_StartOffset(), m.get_TextRange().get_EndOffset());
                        }
                        rangeArray = declaration.get_Members().OfType<TypeDeclaration>().Where<TypeDeclaration>(predicate).Select<TypeDeclaration, TextRange>(selector).ToArray<TextRange>();
                        if (rangeArray.Length > 0)
                        {
                            break;
                        }
                    }
                }
            }
            string text = document.GetText(0);
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            int startIndex = 0;
            foreach (TextRange range in rangeArray)
            {
                builder.Append(text.Substring(startIndex, range.get_StartOffset() - startIndex));
                int introduced18 = range.get_StartOffset();
                int num4 = text.Take<char>((introduced18 + range.get_Length())).Count<char>(c => (c == '\n')) + 1;
                builder.Append("\n#line " + num4 + "\n");
                num4 = text.Take<char>(range.get_StartOffset()).Count<char>(c => (c == '\n')) + 1;
                object[] objArray = new object[5];
                objArray[0] = "#line ";
                objArray[1] = num4;
                objArray[2] = "\n";
                int introduced19 = range.get_StartOffset();
                objArray[3] = text.Substring(introduced19, range.get_Length());
                objArray[4] = "\n";
                builder2.Append(string.Concat(objArray));
                startIndex = range.get_EndOffset();
            }
            builder.Append(text.Substring(startIndex));
            return (builder.ToString() + "\n" + builder2.ToString());
        }

        public override string GetCompilerOptions(QueryCore queryCore)
        {
            bool flag = queryCore.Source.Contains("#LINQPad /optimize-");
            return ("/unsafe" + ((!MainForm.Instance.OptimizeQueries || flag) ? "" : " /optimize"));
        }

        public override string GetFooter(QueryCore query)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            if (query.QueryKind == QueryLanguage.Expression)
            {
                builder.AppendLine("  );");
            }
            if (query.QueryKind != QueryLanguage.Program)
            {
                builder.AppendLine("  }");
            }
            builder.AppendLine("}");
            return builder.ToString();
        }

        public override string GetHeader(QueryCore q)
        {
            StringBuilder builder = new StringBuilder("#define LINQPAD\r\n");
            builder.AppendLine(string.Join("\r\n", (from n in base.ImportedNamespaces select "using " + n + ";").ToArray<string>()));
            if (q.IncludePredicateBuilder)
            {
                builder.AppendLine("public static class PredicateBuilder\r\n{\r\n\tpublic static System.Linq.Expressions.Expression<Func<T, bool>> True<T> () { return f => true; }\r\n\tpublic static System.Linq.Expressions.Expression<Func<T, bool>> False<T> () { return f => false; }\r\n\r\n\tpublic static System.Linq.Expressions.Expression<Func<T, bool>> Or<T> (this System.Linq.Expressions.Expression<Func<T, bool>> expr1, System.Linq.Expressions.Expression<Func<T, bool>> expr2)\r\n\t{\r\n\t\tSystem.Linq.Expressions.Expression invokedExpr = System.Linq.Expressions.Expression.Invoke (\r\n\t\t  expr2, expr1.Parameters.Cast<System.Linq.Expressions.Expression> ());\r\n\r\n\t\treturn System.Linq.Expressions.Expression.Lambda<Func<T, bool>> (\r\n\t\t  System.Linq.Expressions.Expression.OrElse (expr1.Body, invokedExpr), expr1.Parameters);\r\n\t}\r\n\r\n\tpublic static System.Linq.Expressions.Expression<Func<T, bool>> And<T> (this System.Linq.Expressions.Expression<Func<T, bool>> expr1, System.Linq.Expressions.Expression<Func<T, bool>> expr2)\r\n\t{\r\n\t\tSystem.Linq.Expressions.Expression invokedExpr = System.Linq.Expressions.Expression.Invoke (\r\n\t\t  expr2, expr1.Parameters.Cast<System.Linq.Expressions.Expression> ());\r\n\r\n\t\treturn System.Linq.Expressions.Expression.Lambda<Func<T, bool>> (\r\n\t\t  System.Linq.Expressions.Expression.AndAlso (expr1.Body, invokedExpr), expr1.Parameters);\r\n\t}\r\n}");
            }
            builder.Append("\r\npublic partial class UserQuery");
            if (q.QueryBaseClassName != null)
            {
                builder.Append(" : " + q.QueryBaseClassName);
            }
            builder.AppendLine("\r\n{");
            DataContextDriver driver = q.GetDriver(true);
            if (driver != null)
            {
                builder.Append("  public UserQuery (");
                ParameterDescriptor[] contextConstructorParams = base.GetContextConstructorParams(driver, q.Repository);
                builder.Append(string.Join(", ", (from p in contextConstructorParams select p.FullTypeName + " " + p.ParameterName).ToArray<string>()));
                builder.Append(") : base (");
                builder.Append(string.Join(", ", (from p in contextConstructorParams select p.ParameterName).ToArray<string>()));
                builder.AppendLine(") { }");
            }
            builder.AppendLine("\r\n  [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
            builder.AppendLine("  void RunUserAuthoredQuery()");
            builder.AppendLine("  {");
            if (q.QueryKind == QueryLanguage.Expression)
            {
                builder.AppendLine("  LINQPad.Extensions.Dump<object> (");
            }
            else if (q.QueryKind == QueryLanguage.Program)
            {
                builder.AppendLine("  Main();");
                builder.AppendLine("}");
            }
            return builder.ToString();
        }

        private int GetPreprocessorDirectiveEndOffset(string queryText)
        {
            Document document = new Document();
            document.set_Language(new CSharpSyntaxLanguage());
            document.set_Text(queryText);
            bool flag = false;
            if (document.get_Tokens().Count < 2)
            {
                return 0;
            }
            IToken token = null;
            using (IEnumerator enumerator = document.get_Tokens().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IToken current = (IToken) enumerator.Current;
                    if (!(current.get_IsWhitespace() || current.get_IsComment()))
                    {
                        if ((current.get_Key() != "DefinePreProcessorDirective") && !(current.get_Key() == "PreProcessorDirectiveText"))
                        {
                            goto Label_00A5;
                        }
                        flag = true;
                    }
                    token = current;
                }
                goto Label_00C6;
            Label_00A5:
                if (!flag)
                {
                    return 0;
                }
            }
        Label_00C6:
            if (!flag)
            {
                return 0;
            }
            return token.get_TextRange().get_EndOffset();
        }

        public static bool IsKeyword(string word)
        {
            return (Array.BinarySearch<string>(Keywords, word) > -1);
        }
    }
}

