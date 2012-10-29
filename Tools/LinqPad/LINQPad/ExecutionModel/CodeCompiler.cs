namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class CodeCompiler
    {
        public CompilerErrorCollection Errors = new CompilerErrorCollection();
        public int LineOffset;

        public bool Compile(CodeDomProvider codeProv, IEnumerable<string> references, string source, string outputFile, string compilerOptions)
        {
            CompilerResults results;
            this.Errors.Clear();
            CompilerParameters options = new CompilerParameters((from r in references
                where (r != null) && (r.Trim().Length > 0)
                select r).ToArray<string>(), outputFile, true) {
                TreatWarningsAsErrors = false,
                GenerateExecutable = false,
                IncludeDebugInformation = true,
                WarningLevel = 4
            };
            options.TempFiles.KeepFiles = source.Contains("Debugger.Break") || source.Contains("Debugger.Launch");
            options.CompilerOptions = compilerOptions;
            try
            {
                string path = Path.ChangeExtension(outputFile, ".cs");
                File.WriteAllText(path, source);
                results = codeProv.CompileAssemblyFromFile(options, new string[] { path });
                if (!options.TempFiles.KeepFiles && File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch
                    {
                    }
                }
            }
            catch (ExternalException)
            {
                Thread.Sleep(0x3e8);
                try
                {
                    results = codeProv.CompileAssemblyFromSource(options, new string[] { source });
                }
                catch (ExternalException)
                {
                    CompilerError error = new CompilerError {
                        ErrorText = "Unable to invoke the code provider: ExternalException was thrown. Please report this problem if it persists."
                    };
                    this.Errors.Add(error);
                    return false;
                }
            }
            foreach (CompilerError error2 in results.Errors)
            {
                if ((error2.ErrorNumber != "CS4014") || (Math.Max(0, error2.Line - this.LineOffset) > 1))
                {
                    CompilerError error3 = new CompilerError("", Math.Max(0, error2.Line - this.LineOffset), error2.Column, error2.ErrorNumber, error2.ErrorText.Replace(" (are you missing an assembly reference?)", "")) {
                        IsWarning = error2.IsWarning
                    };
                    this.Errors.Add(error3);
                }
            }
            return !this.Errors.HasErrors;
        }

        public virtual string GetCompilerOptions(QueryCore query)
        {
            return null;
        }

        protected void ReportError(string message)
        {
            this.ReportError(message, "", 0, 0);
        }

        protected void ReportError(string message, string errorNumber, int row, int column)
        {
            this.Errors.Add(new CompilerError(null, row, column, errorNumber, message));
        }

        private CompilerError Error
        {
            get
            {
                return this.Errors.Cast<CompilerError>().FirstOrDefault<CompilerError>(ce => !ce.IsWarning);
            }
        }

        protected string ErrorMessage
        {
            get
            {
                CompilerError error = this.Error;
                if (error == null)
                {
                    return null;
                }
                return error.ErrorText;
            }
            set
            {
                CompilerError error = this.Error;
                if (error == null)
                {
                    this.ReportError(value);
                }
                else
                {
                    error.ErrorText = value;
                }
            }
        }

        protected string ErrorNumber
        {
            get
            {
                CompilerError error = this.Error;
                if (error == null)
                {
                    return null;
                }
                return error.ErrorNumber;
            }
        }
    }
}

