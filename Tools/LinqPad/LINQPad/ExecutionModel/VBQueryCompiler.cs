namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using LINQPad.Extensibility.DataContext;
    using LINQPad.UI;
    using Microsoft.VisualBasic;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class VBQueryCompiler : QueryCompiler
    {
        public VBQueryCompiler(QueryCore query, bool addReferences) : base(query, addReferences)
        {
        }

        public override bool Compile(string queryText, QueryCore query, TempFileRef dataContextAssembly)
        {
            bool flag;
            StringBuilder builder = new StringBuilder(this.GetHeader(query));
            base.LineOffset = builder.ToString().Count<char>(c => c == '\n');
            builder.AppendLine(Regex.Replace(queryText.Trim(), "(?<!\r)\n", "\r\n"));
            builder.Append(this.GetFooter(query));
            string str = "v4.0";
            Dictionary<string, string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", str);
            VBCodeProvider codeProvider = new VBCodeProvider(providerOptions);
            if ((!(flag = base.Compile(codeProvider, builder.ToString(), dataContextAssembly, query.QueryKind, this.GetCompilerOptions(query))) && (query.QueryKind == QueryLanguage.VBExpression)) && (base.ErrorMessage == ") expected"))
            {
                base.ErrorMessage = ") or end of expression expected";
                return flag;
            }
            if (!(flag || !base.ErrorMessage.ToLowerInvariant().Contains("predicatebuilder")))
            {
                base.ErrorMessage = base.ErrorMessage + QueryCompiler.PredicateBuilderMessage;
                return flag;
            }
            if (!(((flag || (query.QueryKind != QueryLanguage.VBStatements)) || !(base.ErrorMessage == "Expression is not a method.")) || query.Source.TrimStart(new char[0]).StartsWith("dim", StringComparison.OrdinalIgnoreCase)))
            {
                base.ErrorMessage = base.ErrorMessage + "\r\n(Try setting the query language to 'VB Expression' rather than 'VB Statements')";
                return flag;
            }
            if ((!flag && (query.QueryKind == QueryLanguage.VBExpression)) && (base.ErrorMessage == "Expression expected."))
            {
                base.ErrorMessage = base.ErrorMessage + "\r\n(Set the query language to 'VB Statement(s)' for a statement-based code)";
            }
            return flag;
        }

        public override string GetCompilerOptions(QueryCore queryCore)
        {
            return ("/optioninfer+" + (MainForm.Instance.OptimizeQueries ? " /optimize" : "") + (UserOptions.Instance.CompileVBInStrictMode ? " /optionstrict" : ""));
        }

        public override string GetFooter(QueryCore query)
        {
            StringBuilder builder = new StringBuilder();
            if (query.QueryKind == QueryLanguage.VBExpression)
            {
                builder.AppendLine("LINQPad.Extensions.Dump(linqPadQuery)");
            }
            if (query.QueryKind != QueryLanguage.VBProgram)
            {
                builder.AppendLine("End Sub");
            }
            builder.AppendLine("End Class");
            return builder.ToString();
        }

        public override string GetHeader(QueryCore query)
        {
            StringBuilder builder = new StringBuilder("#Const LINQPAD = True\r\n");
            builder.AppendLine(string.Join("\r\n", (from n in base.ImportedNamespaces select "Imports " + n).ToArray<string>()));
            builder.AppendLine();
            if (query.IncludePredicateBuilder)
            {
                builder.AppendLine("Public Module PredicateBuilder\r\n    <System.Runtime.CompilerServices.Extension> _\r\n    Public Function [And](Of T)(ByVal expr1 As Expression(Of Func(Of T, Boolean)), ByVal expr2 As Expression(Of Func(Of T, Boolean))) As Expression(Of Func(Of T, Boolean))\r\n        Dim invokedExpr As Expression = Expression.Invoke(expr2, expr1.Parameters.Cast(Of Expression)())\r\n        Return Expression.Lambda(Of Func(Of T, Boolean))(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters)\r\n    End Function\r\n\r\n    Public Function [False](Of T)() As Expression(Of Func(Of T, Boolean))\r\n      Return Function(f) False\r\n    End Function\r\n\r\n    <System.Runtime.CompilerServices.Extension> _\r\n    Public Function [Or](Of T)(ByVal expr1 As Expression(Of Func(Of T, Boolean)), ByVal expr2 As Expression(Of Func(Of T, Boolean))) As Expression(Of Func(Of T, Boolean))\r\n        Dim invokedExpr As Expression = Expression.Invoke(expr2, expr1.Parameters.Cast(Of Expression)())\r\n        Return Expression.Lambda(Of Func(Of T, Boolean))(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters)\r\n    End Function\r\n\r\n    Public Function [True](Of T)() As Expression(Of Func(Of T, Boolean))\r\n      Return Function(f) True\r\n    End Function\r\n\r\nEnd Module\r\n");
            }
            builder.AppendLine("Public Class UserQuery");
            if (query.QueryBaseClassName != null)
            {
                builder.AppendLine("  Inherits " + query.QueryBaseClassName);
            }
            builder.AppendLine();
            builder.AppendLine();
            DataContextDriver driver = query.GetDriver(true);
            if (driver != null)
            {
                ParameterDescriptor[] contextConstructorParams = base.GetContextConstructorParams(driver, query.Repository);
                builder.Append("  Public Sub New (");
                builder.Append(string.Join(", ", (from p in contextConstructorParams select "ByVal " + p.ParameterName + " As " + p.FullTypeName).ToArray<string>()));
                builder.AppendLine(")");
                builder.Append("    MyBase.New(");
                builder.Append(string.Join(", ", (from p in contextConstructorParams select p.ParameterName).ToArray<string>()));
                builder.AppendLine(")");
                builder.AppendLine("  End Sub");
                builder.AppendLine();
            }
            builder.AppendLine("  Private Sub RunUserAuthoredQuery()");
            if (query.QueryKind == QueryLanguage.VBExpression)
            {
                builder.Append("Dim linqPadQuery = ");
            }
            else if (query.QueryKind == QueryLanguage.VBProgram)
            {
                builder.AppendLine("  Main\r\nEnd Sub");
            }
            return builder.ToString();
        }
    }
}

