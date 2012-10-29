namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class FSharpQueryCompiler : QueryCompiler
    {
        public FSharpQueryCompiler(QueryCore query, bool addReferences) : base(query, addReferences)
        {
        }

        public override bool Compile(string queryText, QueryCore query, TempFileRef dataContextAssembly)
        {
            Process process;
            string str9;
            bool flag2;
            StringBuilder builder = new StringBuilder(this.GetHeader(query));
            base.LineOffset = builder.ToString().Count<char>(c => c == '\n');
            if (queryText.Trim().Length == 0)
            {
                queryText = "\"\"";
            }
            string str = queryText.Replace("\t", "".PadRight(UserOptions.Instance.TabSizeActual));
            if (query.QueryKind == QueryLanguage.FSharpExpression)
            {
                str = "  " + str.Replace("\r\n", "\r\n  ");
            }
            builder.AppendLine(str);
            builder.Append(this.GetFooter(query));
            base.OutputFile = TempFileRef.GetRandom("query", (query.QueryKind == QueryLanguage.FSharpExpression) ? ".dll" : ".exe");
            base.PDBFile = new TempFileRef(Path.ChangeExtension(base.OutputFile.FullPath, ".pdb"));
            List<string> list = new List<string>(base.References);
            if (dataContextAssembly != null)
            {
                list.Add(dataContextAssembly.FullPath);
            }
            string str2 = @"Microsoft F#\v4.0\fsc.exe";
            string path = Path.Combine(PathHelper.ProgramFiles, str2);
            string str4 = Path.Combine(PathHelper.ProgramFilesX86, str2);
            if (!(File.Exists(path) || File.Exists(str4)))
            {
                base.ReportError("Cannot locate " + str4 + " - is F# installed?");
                return false;
            }
            string str5 = File.Exists(str4) ? str4 : path;
            string str6 = Path.ChangeExtension(base.OutputFile.FullPath, ".fs");
            string str7 = (query.QueryKind == QueryLanguage.FSharpExpression) ? "library" : "exe";
            string str8 = (("-o:\"" + base.OutputFile.FullPath + "\" -g --debug:full --target:" + str7 + " --utf8output " + this.GetCompilerOptions(query) + " ") + string.Join(" ", (from r in list select "-r:\"" + r + "\"").ToArray<string>())) + " \"" + str6 + "\"";
            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = str5,
                Arguments = str8,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            File.WriteAllText(str6, builder.ToString());
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                process = Process.Start(startInfo);
                str9 = process.StandardError.ReadToEnd();
            }
            finally
            {
                try
                {
                    File.Delete(str6);
                }
                catch
                {
                }
            }
            stopwatch.Stop();
            if (!(flag2 = process.ExitCode == 0))
            {
                this.ParseError(str9);
            }
            return flag2;
        }

        public override string GetCompilerOptions(QueryCore queryCore)
        {
            return (MainForm.Instance.OptimizeQueries ? "--optimize+" : "");
        }

        public override string GetFooter(QueryCore query)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            if (query.QueryKind == QueryLanguage.FSharpExpression)
            {
                builder.AppendLine("  ).Dump()");
                builder.AppendLine("end");
            }
            return builder.ToString();
        }

        public override string GetHeader(QueryCore q)
        {
            StringBuilder builder = new StringBuilder();
            if (q.QueryKind == QueryLanguage.FSharpExpression)
            {
                builder.AppendLine("namespace global\r\n");
            }
            builder.AppendLine(string.Join("\r\n", (from n in base.ImportedNamespaces select "open " + n).ToArray<string>()));
            builder.AppendLine("open LINQPad.FSharpExtensions");
            if (q.QueryKind == QueryLanguage.FSharpProgram)
            {
                builder.AppendLine("let Dump = Extensions.Dump");
            }
            DataContextDriver driver = q.GetDriver(true);
            StringBuilder builder2 = new StringBuilder();
            if (q.QueryKind == QueryLanguage.FSharpExpression)
            {
                builder.Append("\r\ntype UserQuery(");
                if ((driver != null) && (q.QueryKind == QueryLanguage.FSharpExpression))
                {
                    builder2.Append(" inherit " + q.QueryBaseClassName + "(");
                    ParameterDescriptor[] contextConstructorParams = base.GetContextConstructorParams(driver, q.Repository);
                    builder.Append(string.Join(", ", (from p in contextConstructorParams select p.ParameterName + " : " + p.FullTypeName).ToArray<string>()));
                    builder.Append(") as this ");
                    builder2.Append(string.Join(", ", (from p in contextConstructorParams select p.ParameterName).ToArray<string>()));
                    builder2.Append(")");
                }
                else
                {
                    builder.Append(")");
                }
                builder.AppendLine(" = class");
                if (builder2.Length > 0)
                {
                    builder.AppendLine(builder2.ToString());
                }
                builder.AppendLine(" member private dc.RunUserAuthoredQuery() =");
                builder.AppendLine("  (");
            }
            return builder.ToString();
        }

        private void ParseError(string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                base.ReportError("Error compiling code snippet.");
            }
            else
            {
                error = error.Trim();
                if (error.Contains("\r\n\r\n"))
                {
                    error = error.Substring(0, error.IndexOf("\r\n\r\n"));
                }
                error = error.Replace("\r\n", " ");
                Match match = Regex.Match(error, "\r\n\t\t\t\t\\(\r\n\t\t\t\t\t(\\d+) \\s* , \\s* (\\d+)\r\n\t\t\t\t\\)\r\n\t\t\t\t\\s* : \\s* error \\s+ (.+?): \\s+ (.*)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
                if (!(match.Success && (match.Groups.Count >= 5)))
                {
                    base.ReportError(error.Trim());
                }
                else
                {
                    int row = 0;
                    int column = 0;
                    try
                    {
                        row = Math.Max(0, int.Parse(match.Groups[1].Value) - base.LineOffset);
                        column = int.Parse(match.Groups[2].Value);
                    }
                    catch
                    {
                    }
                    string errorNumber = match.Groups[3].Value;
                    string message = match.Groups[4].Value;
                    base.ReportError(message, errorNumber, row, column);
                }
            }
        }
    }
}

