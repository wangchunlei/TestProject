namespace LINQPad.UI.SchemaTreeInternal
{
    using LINQPad;
    using LINQPad.ExecutionModel;
    using LINQPad.UI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    internal static class QueryableMenuHelper
    {
        public static void AddQueryableItems(SchemaTree tree, BaseNode node, ContextMenuStrip menu, string memberStem, IEnumerable<string> columnNames)
        {
            EventHandler onClick = null;
            EventHandler handler2 = null;
            EventHandler handler3 = null;
            EventHandler handler4 = null;
            string compQueryBase;
            bool isVBDefault = UserOptions.Instance.IsVBDefault;
            QueryLanguage exprLanguage = isVBDefault ? QueryLanguage.VBExpression : QueryLanguage.Expression;
            string take100Query = memberStem + ".Take (100)";
            menu.Items.Add(take100Query, null, (sender, e) => tree.OnNewQuery(new NewQueryArgs(take100Query, new QueryLanguage?(exprLanguage), !memberStem.Contains("..."))));
            string takeQuery = memberStem + ".Take (" + (UserOptions.Instance.ResultsInGrids ? "1000" : "...") + ")";
            menu.Items.Add(takeQuery, null, (sender, e) => tree.OnNewQuery(new NewQueryArgs(takeQuery, new QueryLanguage?(exprLanguage), UserOptions.Instance.ResultsInGrids && !memberStem.Contains("..."))));
            if (UserOptions.Instance.ResultsInGrids)
            {
                string takeBigQuery = memberStem + ".Take (50000)";
                menu.Items.Add(takeBigQuery, null, (sender, e) => tree.OnNewQuery(new NewQueryArgs(takeBigQuery, new QueryLanguage?(exprLanguage), !memberStem.Contains("..."))));
            }
            string countQuery = memberStem + ".Count()";
            menu.Items.Add(countQuery, null, (sender, e) => tree.OnNewQuery(new NewQueryArgs(countQuery, new QueryLanguage?(exprLanguage), !memberStem.Contains("..."))));
            string iterationVar = memberStem.Substring(0, 1).ToLowerInvariant();
            if (!isVBDefault)
            {
                string whereQuery = memberStem + ".Where (" + iterationVar + " => ";
                menu.Items.Add(whereQuery + "...)", null, delegate (object sender, EventArgs e) {
                    NewQueryArgs args = new NewQueryArgs(whereQuery + iterationVar + (memberStem.Contains("...") ? "" : ".…)"), new QueryLanguage?(exprLanguage)) {
                        ListMembers = true
                    };
                    tree.OnNewQuery(args);
                });
            }
            if (isVBDefault)
            {
                compQueryBase = "From " + iterationVar + " In " + memberStem + " _\nWhere ... _\nSelect ";
            }
            else
            {
                compQueryBase = "from " + iterationVar + " in " + memberStem + "\nwhere ... \nselect ";
            }
            string simpleCompQuery = compQueryBase + iterationVar;
            menu.Items.Add(simpleCompQuery.Replace("\n", " "), null, delegate (object sender, EventArgs e) {
                NewQueryArgs args = new NewQueryArgs(simpleCompQuery, new QueryLanguage?(exprLanguage)) {
                    ListMembers = true
                };
                tree.OnNewQuery(args);
            });
            simpleCompQuery = simpleCompQuery.Replace("here ...", "here " + (memberStem.Contains("...") ? "" : (iterationVar + ".…")));
            string str = compQueryBase + "new { <all columns> }";
            if (!(!columnNames.Any<string>() || isVBDefault))
            {
                menu.Items.Add(str.Replace("\n", " "), null, delegate (object sender, EventArgs e) {
                    StringBuilder builder = new StringBuilder(compQueryBase.Replace("here ...", "here " + (memberStem.Contains("...") ? "" : (iterationVar + ".…"))) + "new\n{\n");
                    foreach (string str in columnNames)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            bool flag;
                            string s = (flag = str.Contains("(")) ? Regex.Match(str, @"^\w+", RegexOptions.CultureInvariant).Value : str;
                            string str3 = (!char.IsLower(s, 0) || !CSharpQueryCompiler.IsKeyword(s)) ? "" : "@";
                            builder.Append("\t");
                            if (flag)
                            {
                                builder.Append(str3 + s + " = " + iterationVar + "." + str + ",\n");
                            }
                            else
                            {
                                builder.Append(iterationVar + "." + str3 + str + ",\n");
                            }
                        }
                    }
                    builder.Remove(builder.Length - 2, 2);
                    builder.Append("\n}");
                    NewQueryArgs args = new NewQueryArgs(builder.ToString(), new QueryLanguage?(exprLanguage)) {
                        ListMembers = true
                    };
                    tree.OnNewQuery(args);
                });
            }
            if (!UserOptions.Instance.ResultsInGrids)
            {
                menu.Items.Add("-");
                if (onClick == null)
                {
                    onClick = delegate (object sender, EventArgs e) {
                        NewQueryArgs args = new NewQueryArgs(memberStem + ".Take(100)", new QueryLanguage?(exprLanguage), !memberStem.Contains("...")) {
                            IntoGrids = true
                        };
                        tree.OnNewQuery(args);
                    };
                }
                menu.Items.Add("Explore top 100 rows in grid", null, onClick);
                if (handler2 == null)
                {
                    handler2 = delegate (object sender, EventArgs e) {
                        NewQueryArgs args = new NewQueryArgs(memberStem + ".Take(1000)", new QueryLanguage?(exprLanguage), !memberStem.Contains("...")) {
                            IntoGrids = true
                        };
                        tree.OnNewQuery(args);
                    };
                }
                menu.Items.Add("Explore top 1000 rows in grid", null, handler2);
                if (handler3 == null)
                {
                    handler3 = delegate (object sender, EventArgs e) {
                        NewQueryArgs args = new NewQueryArgs(memberStem + ".Take(50000)", new QueryLanguage?(exprLanguage), !memberStem.Contains("...")) {
                            IntoGrids = true
                        };
                        tree.OnNewQuery(args);
                    };
                }
                menu.Items.Add("Explore top 50,000 rows in grid", null, handler3);
                if (handler4 == null)
                {
                    handler4 = delegate (object sender, EventArgs e) {
                        NewQueryArgs args = new NewQueryArgs(simpleCompQuery, new QueryLanguage?(exprLanguage)) {
                            IntoGrids = true,
                            ListMembers = true
                        };
                        tree.OnNewQuery(args);
                    };
                }
                menu.Items.Add("Execute LINQ query into data grid", null, handler4);
            }
        }
    }
}

