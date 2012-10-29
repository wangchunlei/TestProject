namespace LINQPad.UI
{
    using ActiproBridge;
    using ActiproSoftware.Drawing;
    using ActiproSoftware.SyntaxEditor;
    using ActiproSoftware.SyntaxEditor.Addons.CSharp;
    using ActiproSoftware.SyntaxEditor.Addons.DotNet.Ast;
    using ActiproSoftware.SyntaxEditor.Commands;
    using LINQPad;
    using LINQPad.Properties;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;

    internal class QueryEditor : ActiproSoftware.SyntaxEditor.SyntaxEditor
    {
        private bool _firstSmartTag;
        private int _initialSelectioMarginWidth;
        private DateTimeOffset _lastChange;
        private int _lastCursorPos;
        private int _slowSmartTagCount;
        private Timer _smartTagTimer;
        private bool _textForSmartTagDirty;
        private static float _zoomFactor = 1f;
        private static Font DefaultQueryFont;
        internal SpanIndicatorLayer MainErrorLayer;
        public static FindReplaceOptions Options = new FindReplaceOptions();
        private static string QueryZoomPath = Path.Combine(Program.UserDataFolder, "queryzoom.txt");
        internal SpanIndicatorLayer StackTraceLayer;
        internal SpanIndicatorLayer UriLayer;
        private static Regex UriMatcher = new Regex(@"\bhttp://[^\s]+", RegexOptions.Multiline);
        internal SpanIndicatorLayer WarningsLayer;

        public event EventHandler EscapeRequest;

        public event EventHandler NextQueryRequest;

        public event EventHandler PreviousQueryRequest;

        public event EventHandler<RepositoryEventArgs> RepositoryDropped;

        static QueryEditor()
        {
            Options.set_FindText("");
            try
            {
                if (File.Exists(QueryZoomPath))
                {
                    _zoomFactor = float.Parse(File.ReadAllText(QueryZoomPath).Replace(",", "."), CultureInfo.InvariantCulture);
                }
            }
            catch
            {
            }
            try
            {
                DefaultQueryFont = Control.DefaultFont;
                DefaultQueryFont = new Font(FontFamily.GenericMonospace, 10f);
            }
            catch
            {
            }
            try
            {
                DefaultQueryFont = new Font("Consolas", 10f);
                if (DefaultQueryFont.Name != "Consolas")
                {
                    DefaultQueryFont = new Font("Courier New", 10f);
                }
            }
            catch
            {
            }
        }

        public QueryEditor()
        {
            EventHandler onClick = null;
            EventHandler handler2 = null;
            EventHandler handler3 = null;
            EventHandler handler4 = null;
            EventHandler handler5 = null;
            EventHandler handler6 = null;
            Action a = null;
            this._lastChange = DateTimeOffset.Now;
            this._firstSmartTag = true;
            this._smartTagTimer = new Timer();
            this.NextQueryRequest = delegate (object sender, EventArgs e) {
            };
            this.PreviousQueryRequest = delegate (object sender, EventArgs e) {
            };
            this.EscapeRequest = delegate (object sender, EventArgs e) {
            };
            this.RepositoryDropped = delegate (object sender, RepositoryEventArgs e) {
            };
            base.set_BracketHighlightingVisible(true);
            this.Dock = DockStyle.Fill;
            base.set_ScrollBarType(0);
            base.set_IndicatorMarginVisible(false);
            this._initialSelectioMarginWidth = base.get_SelectionMarginWidth();
            base.set_SelectionMarginWidth(Control.DefaultFont.Height / 3);
            base.set_Renderer(SyntaxEditorRenderer.get_DefaultRenderer());
            this.UpdateColors(false);
            base.get_Renderer().set_CurrentLineHighlightExtendsToMargins(true);
            base.get_Renderer().set_CurrentLineHighlightBackColor(Color.FromArgb(20, Color.Red));
            base.get_Renderer().set_CurrentLineHighlightBorderColor(Color.FromArgb(50, Color.Maroon));
            this.AllowDrop = true;
            base.set_IndentType(1);
            base.set_ScrollBarType(3);
            base.get_IntelliPrompt().get_SmartTag().set_AutoHideTimeout(0x2710);
            base.get_IntelliPrompt().get_SmartTag().set_ClearOnDocumentModification(true);
            base.get_IntelliPrompt().get_SmartTag().set_MultipleSmartTagsEnabled(false);
            ContextMenuStrip strip = new ContextMenuStrip();
            ToolStripMenuItem miComment = (ToolStripMenuItem) strip.Items.Add("&Comment selected lines", Resources.Comment, new EventHandler(this.CommentSelection));
            ToolStripMenuItem miUncomment = (ToolStripMenuItem) strip.Items.Add("&Uncomment selected lines", Resources.Uncomment, new EventHandler(this.UncommentSelection));
            strip.Items.Add("-");
            if (onClick == null)
            {
                onClick = (sender, e) => this.InsertSnippet(false);
            }
            ToolStripItem miInsertSnippet = strip.Items.Add("Insert Snippet...", null, onClick);
            if (handler2 == null)
            {
                handler2 = (sender, e) => this.InsertSnippet(true);
            }
            ToolStripItem miSurroundWith = strip.Items.Add("Surround With...", null, handler2);
            strip.Items.Add("-");
            if (handler3 == null)
            {
                handler3 = (sender, e) => base.get_SelectedView().CutToClipboard();
            }
            ToolStripItem miCut = strip.Items.Add("Cut", Resources.Cut, handler3);
            if (handler4 == null)
            {
                handler4 = (sender, e) => base.get_SelectedView().CopyToClipboard();
            }
            ToolStripItem miCopy = strip.Items.Add("Copy", Resources.Copy, handler4);
            if (handler5 == null)
            {
                handler5 = (sender, e) => this.CopyPlain();
            }
            ToolStripItem miCopyPlain = strip.Items.Add("Copy without formatting", Resources.Copy, handler5);
            if (handler6 == null)
            {
                handler6 = (sender, e) => base.get_SelectedView().PasteFromClipboard();
            }
            ToolStripItem miPaste = strip.Items.Add("Paste", Resources.Paste, handler6);
            strip.Items.Add("-");
            strip.Items.Add("&Query Properties...", Resources.AdvancedProperties, (sender, e) => MainForm.Instance.AdvancedQueryProps());
            strip.Items.Add("-");
            strip.Items.Add("&Preferences...", Resources.Preferences, new EventHandler(this.SetOptions));
            strip.Opening += delegate (object sender, CancelEventArgs e) {
                miCopy.Enabled = miCopyPlain.Enabled = miSurroundWith.Enabled = this.get_SelectedView().get_SelectedText().Length > 0;
                miCut.Enabled = this.get_SelectedView().get_CanDelete();
                miPaste.Enabled = this.get_SelectedView().get_CanPaste();
                miInsertSnippet.Enabled = this.get_SelectedView().get_SelectedText().Length == 0;
                miComment.ShortcutKeyDisplayString = HotKeyManager.UseStudioKeys ? "Ctrl+E, C or Ctrl+K, C" : "Ctrl+K";
                miUncomment.ShortcutKeyDisplayString = HotKeyManager.UseStudioKeys ? "Ctrl+E, U or Ctrl+K, U" : "Ctrl+U";
            };
            this.ContextMenuStrip = strip;
            if (a == null)
            {
                a = () => base.set_ScrollBarType(0);
            }
            Program.RunOnWinFormsTimer(a);
            this._smartTagTimer.Interval = 200;
            this._smartTagTimer.Tick += new EventHandler(this._smartTagTimer_Tick);
            this._smartTagTimer.Start();
            base.set_LineNumberMarginWidth((DefaultQueryFont.Height * 0x12) / 10);
        }

        private void _smartTagTimer_Tick(object sender, EventArgs e)
        {
            if (((((base.get_Document().get_Length() > 0) && this._textForSmartTagDirty) && ((DateTimeOffset.Now > this._lastChange.AddMilliseconds(500.0)) && (this._lastCursorPos == base.get_SelectedView().get_Selection().get_EndOffset()))) && !base.get_IntelliPrompt().get_SmartTag().get_Visible()) && (this._slowSmartTagCount < 2))
            {
                CustomCSharpSyntaxLanguage language = base.get_Document().get_Language() as CustomCSharpSyntaxLanguage;
                if (language != null)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    try
                    {
                        language.ShowNamespaceSmartTag(this, this._lastCursorPos);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                    }
                    stopwatch.Stop();
                    if ((stopwatch.ElapsedMilliseconds > 0x7d0L) || ((stopwatch.ElapsedMilliseconds > 500L) && !this._firstSmartTag))
                    {
                        this._slowSmartTagCount += 2;
                    }
                    else if (!((stopwatch.ElapsedMilliseconds <= 100L) || this._firstSmartTag))
                    {
                        this._slowSmartTagCount++;
                    }
                    this._firstSmartTag = false;
                }
                this._textForSmartTagDirty = false;
            }
        }

        private void AddedNamespace()
        {
            CustomCSharpSyntaxLanguage language = base.get_Document().get_Language() as CustomCSharpSyntaxLanguage;
            if (language != null)
            {
                language.ShowIntelliPromptParameterInfo(this);
            }
        }

        private string BulletizeAndEscape(string s)
        {
            if (!s.Contains<char>('\n'))
            {
                return IntelliPrompt.EscapeMarkupText(s);
            }
            string[] strArray = (from line in s.Split(new char[] { '\n' }) select "• " + IntelliPrompt.EscapeMarkupText(line)).ToArray<string>();
            return string.Join("<span style=\"font-size:3pt\"><br />&nbsp;<br /></span>", strArray);
        }

        internal bool CheckSmartTagAtLocation(int location)
        {
            CustomCSharpSyntaxLanguage language = base.get_Document().get_Language() as CustomCSharpSyntaxLanguage;
            if (language == null)
            {
                return false;
            }
            try
            {
                return language.ShowNamespaceSmartTag(this, location);
            }
            catch
            {
                return false;
            }
        }

        internal void CommentSelection()
        {
            base.get_SelectedView().RaiseEditCommand(new CommentLinesCommand());
        }

        private void CommentSelection(object sender, EventArgs e)
        {
            this.CommentSelection();
        }

        private static string ConvertSpacesToTabs(string text)
        {
            return StringUtil.ConvertSpacesToTabs(text);
        }

        private static string ConvertTabsToSpaces(string text)
        {
            return StringUtil.ConvertTabsToSpaces(text);
        }

        public void CopyPlain()
        {
            string str = base.get_SelectedView().get_SelectedText();
            if (!string.IsNullOrEmpty(str))
            {
                Clipboard.SetText(ConvertTabsToSpaces(str));
            }
        }

        private void DisplaySmartTag(Point? onlyIfAtThisLocation, bool activateDropDown)
        {
            SpanIndicatorLayer layer = base.get_Document().get_SpanIndicatorLayers().get_Item("Smart tags");
            if ((((layer != null) && (layer.get_Count() > 0)) && (layer.get_Visible() && !base.get_IntelliPrompt().get_SmartTag().get_Visible())) && (AutocompletionManager.CurrentSmartTag != null))
            {
                int num = layer.get_Item(0).get_TextRange().get_EndOffset() - 1;
                if (onlyIfAtThisLocation.HasValue)
                {
                    Rectangle characterBounds = base.get_SelectedView().GetCharacterBounds(base.get_SelectedView().OffsetToPosition(num));
                    characterBounds.Height += characterBounds.Height / 2;
                    characterBounds.Inflate(3, 0);
                    if (!characterBounds.Contains(onlyIfAtThisLocation.Value))
                    {
                        return;
                    }
                }
                base.get_IntelliPrompt().get_SmartTag().Show(num, AutocompletionManager.CurrentSmartTag);
                if (activateDropDown)
                {
                    this.OnIntelliPromptSmartTagClicked(EventArgs.Empty);
                }
            }
        }

        internal void DisplaySmartTagAtCursor()
        {
            if (base.get_IntelliPrompt().get_SmartTag().get_Visible())
            {
                this.OnIntelliPromptSmartTagClicked(EventArgs.Empty);
            }
            else
            {
                this.DisplaySmartTag(null, true);
            }
        }

        protected override void Dispose(bool disposing)
        {
            this._smartTagTimer.Dispose();
            base.Dispose(disposing);
        }

        internal void InsertSnippet(bool surroundWith)
        {
            if (!MainForm.Instance.ShowLicensee)
            {
                MainForm.Instance.ActivateAutocompletion();
            }
            else
            {
                SnippetManager.ShowSnippets(this, surroundWith);
            }
        }

        protected override void OnCodeSnippetActivating(CodeSnippetEventArgs e)
        {
            string str;
            RunnableQuery query;
            if (((SnippetManager.ActiveSnippetForm != null) && !SnippetManager.ActiveSnippetForm.IsDisposed) && SnippetManager.ActiveSnippetForm.Visible)
            {
                SnippetManager.ActiveSnippetForm.TopMost = false;
            }
            foreach (CodeSnippetDeclaration declaration in e.get_CodeSnippet().get_Declarations())
            {
                if (!string.IsNullOrEmpty(declaration.get_Function()))
                {
                    str = this.ProcessSnippetFunction(declaration);
                    if (declaration.get_Editable())
                    {
                        declaration.set_Default(str);
                    }
                    else if ((str != null) && (e.get_CodeText() != null))
                    {
                        e.set_CodeText(e.get_CodeText().Replace("$" + declaration.get_ID() + "$", str));
                    }
                }
            }
            if (UserOptions.Instance.ConvertTabsToSpaces && (e.get_CodeText() != null))
            {
                e.set_CodeText(ConvertTabsToSpaces(e.get_CodeText()));
            }
            if ((string.IsNullOrEmpty(base.get_SelectedView().get_SelectedText()) || (e.get_CodeText() == null)) || !e.get_CodeText().Contains("$selected$"))
            {
                base.get_IntelliPrompt().get_CodeSnippets().set_AutoIndentSnippetCode(true);
            }
            else
            {
                int index = e.get_CodeText().IndexOf("$selected$");
                string text = "";
                if (!(((index <= 0) || (e.get_CodeText()[index - 1] != '\t')) ? (e.get_CodeText()[index - 1] != ' ') : false))
                {
                    text = "\t";
                    if (UserOptions.Instance.ConvertTabsToSpaces)
                    {
                        text = ConvertTabsToSpaces(text);
                    }
                }
                for (int i = base.get_SelectedView().get_Selection().get_FirstOffset(); i >= base.get_SelectedView().get_Selection().get_LastOffset(); i++)
                {
                Label_01B1:
                    if (0 == 0)
                    {
                        string substring = base.get_Document().GetSubstring(base.get_SelectedView().get_Selection().get_FirstOffset(), i - base.get_SelectedView().get_Selection().get_FirstOffset());
                        int num3 = base.get_SelectedView().get_Selection().get_FirstOffset() - 1;
                    Label_0260:
                        if ((num3 >= 0) && !char.IsWhiteSpace(base.get_Document().get_Characters(num3)))
                        {
                        }
                        if (0 == 0)
                        {
                            num3++;
                            string str4 = base.get_Document().GetSubstring(num3, i - num3);
                            str = base.get_SelectedView().get_SelectedText().Trim().Replace("\n", "\n" + text);
                            e.set_CodeText(substring + e.get_CodeText().Replace("\n", "\n" + str4).Replace("$selected$", str) + ((base.get_SelectedView().get_SelectedText().Last<char>() == '\n') ? "\n" : ""));
                            base.get_IntelliPrompt().get_CodeSnippets().set_AutoIndentSnippetCode(false);
                        }
                        else
                        {
                            num3--;
                            goto Label_0260;
                        }
                        goto Label_0324;
                    }
                }
                goto Label_01B1;
            }
        Label_0324:
            query = MainForm.Instance.CurrentQueryControl.Query;
            List<string> list = query.AdditionalReferences.ToList<string>();
            List<string> list2 = query.AdditionalGACReferences.ToList<string>();
            using (query.TransactChanges())
            {
                if (e.get_CodeSnippet().get_References().Count > 0)
                {
                    using (IEnumerator enumerator = e.get_CodeSnippet().get_References().GetEnumerator())
                    {
                        Predicate<string> match = null;
                        Predicate<string> predicate2 = null;
                        CodeSnippetReference reference;
                        while (enumerator.MoveNext())
                        {
                            reference = (CodeSnippetReference) enumerator.Current;
                            if (reference.get_Assembly().Contains<char>(','))
                            {
                                if (match == null)
                                {
                                    match = r => string.Equals(r.Split(new char[] { ',' }).First<string>(), reference.get_Assembly().Split(new char[] { ',' }).First<string>(), StringComparison.InvariantCultureIgnoreCase);
                                }
                                list2.RemoveAll(match);
                                list2.Add(reference.get_Assembly());
                            }
                            else
                            {
                                if (predicate2 == null)
                                {
                                    predicate2 = r => string.Equals(Path.GetFileName(r), Path.GetFileName(reference.get_Assembly()), StringComparison.InvariantCultureIgnoreCase);
                                }
                                list.RemoveAll(predicate2);
                                list.Add(PathHelper.ResolveReference(reference.get_Assembly()));
                            }
                        }
                    }
                    query.AdditionalReferences = list.ToArray();
                    query.AdditionalGACReferences = list2.ToArray();
                    query.SortReferences();
                }
                if (e.get_CodeSnippet().get_Imports().Count > 0)
                {
                    query.AdditionalNamespaces = query.AdditionalNamespaces.Concat<string>((from ci in e.get_CodeSnippet().get_Imports().OfType<CodeSnippetImport>() select ci.get_Namespace())).Distinct<string>().ToArray<string>();
                }
                if (e.get_CodeText() != null)
                {
                    if (e.get_CodeText().TrimStart(new char[0]).StartsWith("void ") || e.get_CodeText().TrimStart(new char[0]).StartsWith("static void"))
                    {
                        query.QueryKind = QueryLanguage.Program;
                    }
                    else if (e.get_CodeText().Contains(";") && (query.QueryKind == QueryLanguage.Expression))
                    {
                        query.QueryKind = QueryLanguage.Statements;
                    }
                }
            }
            base.OnCodeSnippetActivating(e);
        }

        protected override void OnCodeSnippetFieldActivated(CodeSnippetFieldEventArgs e)
        {
            if ((e.get_DeclarationID() == "sequence") && (base.get_Document().GetSubstring(e.get_Indicator().get_TextRange()) == "querySource"))
            {
                base.get_Document().ReplaceText(0, e.get_Indicator().get_TextRange(), "");
                if ((MainForm.Instance.CurrentQueryControl != null) && MainForm.Instance.ShowLicensee)
                {
                    MainForm.Instance.CurrentQueryControl.ListTables();
                }
            }
            base.OnCodeSnippetFieldActivated(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            bool flag = false;
            bool flag2 = e.Effect == DragDropEffects.Copy;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] source = e.Data.GetData(DataFormats.FileDrop) as string[];
                if ((source != null) && (MainForm.Instance != null))
                {
                    Func<string, bool> predicate = s => ".dll .exe .xml .pdb .winmd".Split(new char[0]).Contains<string>(Path.GetExtension(s).ToLowerInvariant());
                    if (source.All<string>(predicate))
                    {
                        RunnableQuery currentQuery = MainForm.Instance.CurrentQuery;
                        if (currentQuery != null)
                        {
                            foreach (string str in source)
                            {
                                if (".dll .exe .winmd".Split(new char[0]).Contains<string>(Path.GetExtension(str).ToLowerInvariant()))
                                {
                                    currentQuery.AddRefIfNotPresent(true, new string[] { str });
                                }
                            }
                            currentQuery.SortReferences();
                            flag = true;
                        }
                    }
                    else
                    {
                        foreach (string str in source)
                        {
                            if (".linq .sql .txt".Split(new char[0]).Contains<string>(Path.GetExtension(str).ToLowerInvariant()) || (new FileInfo(str).Length < 0xc350L))
                            {
                                MainForm.Instance.OpenQuery(str, true);
                            }
                        }
                    }
                }
                MainForm.Instance.Activate();
            }
            else if (e.Data.GetDataPresent(typeof(RepositoryDragData)))
            {
                RepositoryDragData data = (RepositoryDragData) e.Data.GetData(typeof(RepositoryDragData));
                RepositoryEventArgs args = new RepositoryEventArgs {
                    Repository = data.Repository,
                    Copy = flag2
                };
                this.RepositoryDropped(this, args);
            }
            base.OnDragDrop(e);
            base.Focus();
            if (flag)
            {
                MainForm.Instance.BeginInvoke(new Action(MainForm.Instance.AdvancedQueryProps));
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(RepositoryDragData)))
            {
                RepositoryDragData data = (RepositoryDragData) e.Data.GetData(typeof(RepositoryDragData));
                e.Effect = ((((e.KeyState & 8) != 8) || !MainForm.Instance.IsPremium) || data.Repository.LinkedDatabases.Any<LinkedDatabase>()) ? DragDropEffects.Link : DragDropEffects.Copy;
            }
            else
            {
                base.OnDragOver(e);
            }
        }

        protected override void OnIntelliPromptMemberListItemDescriptionRequested(EventArgs e)
        {
            CustomIntelliPromptMemberListItem item = base.get_IntelliPrompt().get_MemberList().get_SelectedItem() as CustomIntelliPromptMemberListItem;
            if ((item != null) && (item.DescriptionFunc != null))
            {
                item.set_Description(item.DescriptionFunc());
            }
        }

        protected override void OnIntelliPromptSmartTagClicked(EventArgs e)
        {
            ToolStripDropDownClosedEventHandler handler = null;
            SmartTag tag = base.get_IntelliPrompt().get_SmartTag().get_ActiveSmartTag();
            if ((tag != null) && (tag.get_Tag() is NamespaceCompletionData))
            {
                string str;
                if (base.get_IntelliPrompt().get_MemberList().get_Visible())
                {
                    base.get_IntelliPrompt().get_MemberList().Abort();
                }
                base.get_IntelliPrompt().get_SmartTag().set_AutoHideTimeout(0xea60);
                NamespaceCompletionData data = (NamespaceCompletionData) tag.get_Tag();
                ContextMenuStrip strip = new ContextMenuStrip();
                foreach (NamespaceSuggestion suggestion in data.Suggestions)
                {
                    NamespaceSuggestion localItem = suggestion;
                    str = (localItem.ExtraReference == null) ? "" : (" (in " + Path.GetFileName(localItem.ExtraReference) + ")");
                    strip.Items.Add("using " + localItem.Namespace + str, null, delegate (object sender, EventArgs e) {
                        if (localItem.CaseFixedIdentifier != null)
                        {
                            this.get_Document().ReplaceText(0, new TextRange(data.Offset, data.Offset + data.Length), localItem.CaseFixedIdentifier);
                        }
                        MainForm.Instance.AddNamespaceToQuery(localItem.Namespace);
                        if (localItem.ExtraReference != null)
                        {
                            MainForm.Instance.AddReferenceToQuery(localItem.ExtraReference, true);
                        }
                        this.get_IntelliPrompt().get_SmartTag().Hide();
                        this.get_IntelliPrompt().get_SmartTag().Clear();
                        this.AddedNamespace();
                    });
                }
                strip.Items.Add("-");
                foreach (NamespaceSuggestion suggestion in data.Suggestions)
                {
                    NamespaceSuggestion localItem = suggestion;
                    str = (localItem.ExtraReference == null) ? "" : (" (in " + Path.GetFileName(localItem.ExtraReference) + ")");
                    strip.Items.Add(localItem.Namespace + "." + (localItem.CaseFixedIdentifier ?? data.Identifier) + str, null, delegate (object sender, EventArgs e) {
                        this.get_Document().ReplaceText(0, new TextRange(data.Offset, data.Offset + data.Length), localItem.Namespace + "." + (localItem.CaseFixedIdentifier ?? data.Identifier));
                        if (localItem.ExtraReference != null)
                        {
                            MainForm.Instance.AddReferenceToQuery(localItem.ExtraReference, true);
                        }
                        this.AddedNamespace();
                    });
                }
                if (handler == null)
                {
                    handler = (sender, e) => base.get_IntelliPrompt().get_SmartTag().set_AutoHideTimeout(0x2710);
                }
                strip.Closed += handler;
                strip.Items[0].Select();
                Rectangle rectangle = base.get_IntelliPrompt().get_SmartTag().get_DesktopBounds();
                strip.Show(rectangle.X, rectangle.Y + rectangle.Height);
            }
            base.OnIntelliPromptSmartTagClicked(e);
        }

        protected override void OnIntelliPromptTipLinkClick(IntelliPromptTipLinkClickEventArgs e)
        {
            if (e.get_HRef() == "DisableLambdaSnippets")
            {
                if (base.get_IntelliPrompt().get_MemberList().get_Visible())
                {
                    base.get_IntelliPrompt().get_MemberList().Abort();
                }
                UserOptions.Instance.DisableLambdaSnippets = true;
                UserOptions.Instance.Save();
                AutocompletionManager.DisableLambdaSnippets = true;
                MessageBox.Show("Feature disabled.\r\n\r\nGo to Edit | Preferences -> Advanced to re-enable.", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            base.OnIntelliPromptTipLinkClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (UserOptions.Instance.ConvertTabsToSpaces && (base.get_SelectedView().get_Selection().get_Length() > 0))
            {
                if (e.KeyData == (Keys.Shift | Keys.Tab))
                {
                    base.get_Document().set_TabSize(UserOptions.Instance.TabSizeActual - 1);
                }
                else if (base.get_Document().get_TabSize() != UserOptions.Instance.TabSizeActual)
                {
                    base.get_Document().set_TabSize(UserOptions.Instance.TabSizeActual);
                }
            }
            if (((e.KeyData == Keys.Tab) && base.get_IntelliPrompt().get_MemberList().get_Visible()) && base.get_IntelliPrompt().get_MemberList().get_IsSelectionVirtual())
            {
                base.get_IntelliPrompt().get_MemberList().Abort();
            }
            else if (!(((e.KeyData != Keys.Escape) || !base.get_IntelliPrompt().get_ParameterInfo().get_Visible()) || base.get_IntelliPrompt().get_MemberList().get_Visible()))
            {
                base.get_IntelliPrompt().get_ParameterInfo().Hide();
                e.Handled = true;
            }
            else if (e.KeyData == (Keys.Control | Keys.T))
            {
                e.Handled = true;
                base.OnKeyDown(new KeyEventArgs(Keys.Control | Keys.Multiply));
            }
            else
            {
                if ((e.KeyData == (Keys.Alt | Keys.Shift | Keys.F10)) || (e.KeyData == (Keys.Control | Keys.OemPeriod)))
                {
                    this.DisplaySmartTagAtCursor();
                    e.Handled = true;
                    return;
                }
                if (((e.KeyData == (Keys.Control | Keys.H)) && base.get_IntelliPrompt().get_MemberList().get_Visible()) && (base.get_Document().get_Language() is ICustomSyntaxLanguage))
                {
                    ((ICustomSyntaxLanguage) base.get_Document().get_Language()).ShowIntelliPromptMemberList(this, false, false, null, true);
                }
                else if (Program.PresentationMode && !base.get_IntelliPrompt().get_MemberList().get_Visible())
                {
                    if (((e.KeyData == Keys.PageDown) && base.get_SelectedView().get_Selection().get_IsZeroLength()) && (base.get_SelectedView().get_Selection().get_StartDocumentPosition().get_Line() >= (base.get_Document().get_Lines().get_Count() - 1)))
                    {
                        e.Handled = true;
                        this.NextQueryRequest(this, EventArgs.Empty);
                    }
                    else if (((e.KeyData == Keys.PageUp) && base.get_SelectedView().get_Selection().get_IsZeroLength()) && (base.get_SelectedView().get_Selection().get_StartDocumentPosition().get_Line() == 0))
                    {
                        e.Handled = true;
                        this.PreviousQueryRequest(this, EventArgs.Empty);
                    }
                    else if (e.KeyData == Keys.Escape)
                    {
                        e.Handled = true;
                        this.EscapeRequest(this, EventArgs.Empty);
                    }
                }
            }
            if ((base.get_Document().get_Language() is CSharpSyntaxLanguage) && MainForm.Instance.ShowLicensee)
            {
                ActiproKeyFilter.KeyDown(e, this);
            }
            if (((base.get_IntelliPrompt().get_MemberList().get_Visible() || base.get_IntelliPrompt().get_ParameterInfo().get_Visible()) && e.Control) && (e.KeyData != (Keys.Control | Keys.T)))
            {
                this.SetIntelliPopupOpacity(0.15);
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (MainForm.Instance.ShowLicensee && (base.get_Document().get_Language() is CSharpSyntaxLanguage))
            {
                ActiproKeyFilter.KeyPress(e, this);
            }
            base.OnKeyPress(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (!((base.get_IntelliPrompt().get_MemberList().get_Visible() || base.get_IntelliPrompt().get_ParameterInfo().get_Visible()) ? e.Control : true))
            {
                this.SetIntelliPopupOpacity(1.0);
            }
            if (UserOptions.Instance.ConvertTabsToSpaces && (base.get_Document().get_TabSize() != UserOptions.Instance.TabSizeActual))
            {
                base.get_Document().set_TabSize(UserOptions.Instance.TabSizeActual);
            }
            base.OnKeyUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.DisplaySmartTag(new Point?(e.Location), false);
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((Control.ModifierKeys == Keys.Control) && (e.Delta != 0))
            {
                Zoom(e.Delta);
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        protected override void OnPasteDragDrop(PasteDragDropEventArgs e)
        {
            if (!(string.IsNullOrEmpty(e.get_Text()) || UserOptions.Instance.ConvertTabsToSpaces))
            {
                e.set_Text(ConvertSpacesToTabs(e.get_Text()));
            }
            if ((e.get_Source() == 4) && (SchemaTree.DragRepository != null))
            {
                RepositoryEventArgs args = new RepositoryEventArgs {
                    Repository = SchemaTree.DragRepository
                };
                this.RepositoryDropped(this, args);
            }
            base.OnPasteDragDrop(e);
        }

        protected override void OnSelectionChanged(SelectionEventArgs e)
        {
            MainForm.Instance.IsSplitting = false;
            this._textForSmartTagDirty = true;
            this._lastChange = DateTimeOffset.Now;
            this._lastCursorPos = base.get_SelectedView().get_Selection().get_EndOffset();
            base.OnSelectionChanged(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            MainForm.Instance.IsSplitting = false;
            if ((this.UriLayer != null) && (this.UriLayer.get_Count() > 0))
            {
                this.UriLayer.Clear();
            }
            ActiproKeyFilter.TextChanged(this);
            base.OnTextChanged(e);
        }

        protected override void OnTokenMouseEnter(EditorViewMouseEventArgs e)
        {
            base.OnTokenMouseEnter(e);
            if ((e.get_HitTestResult().get_Token() != null) && ((e.get_HitTestResult().get_Token().get_Key() == "SingleLineComment") || (e.get_HitTestResult().get_Token().get_Key() == "MultiLineComment")))
            {
                string input = null;
                try
                {
                    input = base.get_Document().GetTokenText(e.get_HitTestResult().get_Token());
                }
                catch (ArgumentException)
                {
                    return;
                }
                Match match = UriMatcher.Match(input);
                if (match.Success && (this.UriLayer != null))
                {
                    this.UriLayer.Add(new UriSpanIndicator(), new TextRange(e.get_HitTestResult().get_Token().get_StartOffset() + match.Index, (e.get_HitTestResult().get_Token().get_StartOffset() + match.Index) + match.Length));
                }
            }
        }

        protected override void OnTokenMouseLeave(EditorViewMouseEventArgs e)
        {
            base.OnTokenMouseLeave(e);
            if (this.UriLayer != null)
            {
                this.UriLayer.Clear();
            }
        }

        protected override void OnViewMouseDown(EditorViewMouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (Control.ModifierKeys == Keys.Control))
            {
                if ((e.get_HitTestResult().get_Token() != null) && (e.get_HitTestResult().get_Token().get_Key() == "CommentURLToken"))
                {
                    e.set_Cancel(true);
                    WebHelper.LaunchBrowser(base.get_Document().GetTokenText(e.get_HitTestResult().get_Token()));
                }
                else if ((e.get_HitTestResult().get_Token() != null) && ((e.get_HitTestResult().get_Token().get_Key() == "SingleLineComment") || (e.get_HitTestResult().get_Token().get_Key() == "MultiLineComment")))
                {
                    string tokenText = base.get_Document().GetTokenText(e.get_HitTestResult().get_Token());
                    Match match = UriMatcher.Match(tokenText);
                    if (match.Success)
                    {
                        e.set_Cancel(true);
                        WebHelper.LaunchBrowser(match.Value);
                    }
                }
            }
            base.OnViewMouseDown(e);
        }

        protected override void OnViewMouseHover(EditorViewMouseEventArgs e)
        {
            if (e.get_HitTestResult().get_Target() == 10)
            {
                if ((((e.get_HitTestResult().get_DisplayLine() != null) && (e.get_HitTestResult().get_DisplayLine().get_DocumentLine() != null)) && (e.get_HitTestResult().get_DisplayLine().get_DocumentLine().get_LineIndicators() != null)) && (e.get_HitTestResult().get_DisplayLine().get_DocumentLine().get_LineIndicators().get_Count() > 0))
                {
                    string s = e.get_HitTestResult().get_DisplayLine().get_DocumentLine().get_LineIndicators().get_Item(0).get_Tag() as string;
                    if (s != null)
                    {
                        e.set_ToolTipText(this.BulletizeAndEscape(s));
                    }
                }
            }
            else if ((e.get_HitTestResult().get_Token() != null) && (e.get_HitTestResult().get_Token().get_Key() == "CommentURLToken"))
            {
                e.set_ToolTipText(base.get_Document().GetTokenText(e.get_HitTestResult().get_Token()) + "<br/><b>CTRL + click to follow link</b>");
            }
            else if ((e.get_HitTestResult().get_Token() != null) && ((e.get_HitTestResult().get_Token().get_Key() == "SingleLineComment") || (e.get_HitTestResult().get_Token().get_Key() == "MultiLineComment")))
            {
                string tokenText = base.get_Document().GetTokenText(e.get_HitTestResult().get_Token());
                Match match = UriMatcher.Match(tokenText);
                if (match.Success)
                {
                    e.set_ToolTipText(match.Value + "<br/><b>CTRL + click to follow link</b>");
                }
            }
            else
            {
                SpanIndicator[] indicatorsForTextRange = this.MainErrorLayer.GetIndicatorsForTextRange(new TextRange(e.get_HitTestResult().get_Offset(), e.get_HitTestResult().get_Offset() + 1));
                if (indicatorsForTextRange.Length == 0)
                {
                    indicatorsForTextRange = this.WarningsLayer.GetIndicatorsForTextRange(new TextRange(e.get_HitTestResult().get_Offset(), e.get_HitTestResult().get_Offset() + 1));
                }
                if (indicatorsForTextRange.Length > 0)
                {
                }
                if ((CS$<>9__CachedAnonymousMethodDelegate22 == null) && indicatorsForTextRange.OfType<CompilerErrorSpanIndicator>().Any<CompilerErrorSpanIndicator>(CS$<>9__CachedAnonymousMethodDelegate22))
                {
                    e.set_ToolTipText(this.BulletizeAndEscape(indicatorsForTextRange.OfType<CompilerErrorSpanIndicator>().First<CompilerErrorSpanIndicator>(i => (i.get_Tag() is string)).get_Tag().ToString()));
                }
            }
            base.OnViewMouseHover(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
        }

        private string ProcessSnippetFunction(CodeSnippetDeclaration d)
        {
            // This item is obfuscated and can not be translated.
            if (!Regex.IsMatch(d.get_Function().Trim(), @"ClassName\s*\(\s*\)", RegexOptions.IgnoreCase))
            {
                Match match = Regex.Match(d.get_Function().Trim(), @"SimpleTypeName\s*\((.*)\)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string str2 = match.Groups[1].Value.Trim();
                    if (str2.StartsWith("global::"))
                    {
                        str2 = str2.Substring("global::".Length);
                    }
                    return str2;
                }
                if (d.get_Function().Trim() == "DateTime.Now.Year")
                {
                    return DateTime.Now.Year.ToString();
                }
                if (d.get_Function().Trim() == "DateTime.Now.Month")
                {
                    return DateTime.Now.Month.ToString();
                }
                if (d.get_Function().Trim() == "DateTime.Now.Day")
                {
                    return DateTime.Now.Day.ToString();
                }
                return null;
            }
            SemanticParserService.WaitForParse(SemanticParserServiceRequest.GetParseHashKey(base.get_Document(), base.get_Document()), 0x7d0);
            IAstNode node = base.get_Document().get_SemanticParseData() as IAstNode;
            if (node != null)
            {
                for (IAstNode node2 = node.FindNodeRecursive(base.get_Caret().get_Offset()); node2 == null; node2 = node2.get_ParentNode())
                {
                Label_0065:
                    if (0 == 0)
                    {
                        if (node2 != null)
                        {
                            return ((ClassDeclaration) node2).get_FullName().Split(new char[] { '.', '+' }).Last<string>();
                        }
                        goto Label_00B6;
                    }
                }
                goto Label_0065;
            }
        Label_00B6:
            return null;
        }

        internal void PropagateOptions()
        {
            string path = Path.Combine(Program.UserDataFolder, "queryfont.txt");
            if (!File.Exists(path))
            {
                this.Font = DefaultQueryFont;
            }
            else
            {
                try
                {
                    this.Font = new Font(File.ReadAllText(path), DefaultQueryFont.SizeInPoints);
                }
                catch
                {
                }
            }
            base.set_LineNumberMarginVisible(UserOptions.Instance.ShowLineNumbersInEditor);
        }

        private void SetIntelliPopupOpacity(double value)
        {
            foreach (Form form in Application.OpenForms)
            {
                if ((form.Name == "IntelliPromptMemberListPopup") || (form.Name == "PopupControl"))
                {
                    form.Opacity = value;
                }
            }
        }

        private void SetOptions(object sender, EventArgs e)
        {
            using (OptionsForm form = new OptionsForm())
            {
                if (form.ShowDialog(MainForm.Instance) == DialogResult.OK)
                {
                    MainForm.Instance.PropagateOptions();
                }
            }
        }

        internal void UncommentSelection()
        {
            base.get_SelectedView().RaiseEditCommand(new UncommentLinesCommand());
        }

        private void UncommentSelection(object sender, EventArgs e)
        {
            this.UncommentSelection();
        }

        internal void UpdateColors(bool forceChange)
        {
            if (forceChange || !string.IsNullOrEmpty(UserOptions.Instance.EditorBackColor))
            {
                try
                {
                    BackgroundFill fill;
                    VisualStudio2005SyntaxEditorRenderer renderer = base.get_Renderer();
                    renderer.set_SelectionMarginBackgroundFill(fill = new SolidColorBackgroundFill(UserOptions.Instance.ActualEditorBackColor));
                    renderer.set_TextAreaBackgroundFill(fill);
                }
                catch
                {
                }
            }
        }

        private void UpdateOutlining()
        {
            if (base.get_Document() != null)
            {
                int num = this.IsOutliningEnabled ? this._initialSelectioMarginWidth : (Control.DefaultFont.Height / 3);
                if (base.get_SelectionMarginWidth() != num)
                {
                    base.set_SelectionMarginWidth(num);
                }
            }
        }

        internal void UpdateZoom()
        {
            float emSize = ZoomFactor * 10f;
            if (Math.Abs((float) (this.Font.SizeInPoints - emSize)) > 0.1f)
            {
                Font font;
                base.set_LineNumberMarginFont(font = new Font(this.Font.FontFamily, emSize));
                this.Font = font;
            }
        }

        internal static bool Zoom(int delta)
        {
            if (!(((delta >= 0) || (ZoomFactor >= 0.65f)) ? ((delta <= 0) || (ZoomFactor <= 1.7f)) : false))
            {
                return false;
            }
            float num = (ZoomFactor > 1.3f) ? 0.2f : 0.1f;
            ZoomFactor += num * Math.Sign(delta);
            return true;
        }

        public bool IsOutliningEnabled
        {
            get
            {
                if (base.get_Document() == null)
                {
                    return false;
                }
                return (base.get_Document().get_Outlining().get_Mode() == 2);
            }
            set
            {
                if ((base.get_Document() != null) && (!value || MainForm.Instance.ShowLicensee))
                {
                    OutliningMode mode = value ? ((OutliningMode) 2) : ((OutliningMode) 0);
                    if (base.get_Document().get_Outlining().get_Mode() != mode)
                    {
                        base.get_Document().get_Outlining().set_Mode(mode);
                    }
                    this.UpdateOutlining();
                }
            }
        }

        internal static float ZoomFactor
        {
            get
            {
                return _zoomFactor;
            }
            set
            {
                float num = (float) Math.Round((double) value, 1);
                if (num != _zoomFactor)
                {
                    _zoomFactor = num;
                    MainForm.Instance.UpdateQueryZoom();
                    try
                    {
                        if (!Directory.Exists(Program.UserDataFolder))
                        {
                            Directory.CreateDirectory(Program.UserDataFolder);
                        }
                        File.WriteAllText(QueryZoomPath, ZoomFactor.ToString(CultureInfo.InvariantCulture));
                    }
                    catch
                    {
                    }
                }
            }
        }

        public class RepositoryEventArgs : EventArgs
        {
            public bool Copy;
            public LINQPad.Repository Repository;
        }
    }
}

