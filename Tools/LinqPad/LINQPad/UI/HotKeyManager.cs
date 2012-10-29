namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.Windows.Forms;

    internal class HotKeyManager
    {
        private static Keys? _firstLetter;
        private MainForm _mainForm;
        private QueryControl _queryControl;

        private HotKeyManager(MainForm f, QueryControl qc)
        {
            this._mainForm = f;
            this._queryControl = qc;
        }

        private void CloseQuery()
        {
            this.CurrentQueryControl.TryClose();
        }

        private void CommentSelection()
        {
            this.CurrentQueryControl.Editor.CommentSelection();
        }

        private void ExecuteQuery()
        {
            this.CurrentQueryControl.Run();
        }

        private bool HandleCommonKeys(Keys keyData)
        {
            Keys keys = keyData;
            if (keys <= (Keys.Control | Keys.OemMinus))
            {
                switch (keys)
                {
                    case Keys.F5:
                        goto Label_05AF;

                    case Keys.F7:
                        this.CurrentQueryControl.FocusQueryExplicit();
                        return true;

                    case Keys.F1:
                        this.CurrentQueryControl.ActivateHelp();
                        return true;

                    case (Keys.Shift | Keys.F1):
                        this.CurrentQueryControl.ActivateReflector();
                        return true;

                    case (Keys.Shift | Keys.F4):
                        this.CurrentQueryControl.CloseCurrentVisualizer();
                        return true;

                    case (Keys.Shift | Keys.F5):
                        this.CurrentQueryControl.Cancel(false);
                        return true;

                    case (Keys.Control | Keys.Tab):
                    case (Keys.Control | Keys.PageDown):
                        MainForm.Instance.NextQuery();
                        return true;

                    case (Keys.Control | Keys.PageUp):
                        goto Label_030F;

                    case (Keys.Control | Keys.D0):
                        this.CurrentQueryControl.SetLanguage(9);
                        return true;

                    case (Keys.Control | Keys.D1):
                        this.CurrentQueryControl.SetLanguage(0);
                        return true;

                    case (Keys.Control | Keys.D2):
                        this.CurrentQueryControl.SetLanguage(1);
                        return true;

                    case (Keys.Control | Keys.D3):
                        this.CurrentQueryControl.SetLanguage(2);
                        return true;

                    case (Keys.Control | Keys.D4):
                        this.CurrentQueryControl.SetLanguage(3);
                        return true;

                    case (Keys.Control | Keys.D5):
                        this.CurrentQueryControl.SetLanguage(4);
                        return true;

                    case (Keys.Control | Keys.D6):
                        this.CurrentQueryControl.SetLanguage(5);
                        return true;

                    case (Keys.Control | Keys.D7):
                        this.CurrentQueryControl.SetLanguage(6);
                        return true;

                    case (Keys.Control | Keys.D8):
                        this.CurrentQueryControl.SetLanguage(7);
                        return true;

                    case (Keys.Control | Keys.D9):
                        this.CurrentQueryControl.SetLanguage(8);
                        return true;

                    case (Keys.Control | Keys.D):
                        this.CurrentQueryControl.UseCurrentDb(true);
                        return true;

                    case (Keys.Control | Keys.R):
                        this.CurrentQueryControl.ToggleResultsCollapse();
                        return true;

                    case (Keys.Control | Keys.Add):
                    case (Keys.Control | Keys.Oemplus):
                        QueryEditor.Zoom(1);
                        return true;

                    case (Keys.Control | Keys.Subtract):
                    case (Keys.Control | Keys.OemMinus):
                        QueryEditor.Zoom(-1);
                        return true;

                    case (Keys.Control | Keys.F3):
                        this.CurrentQueryControl.FindNextSelected();
                        return true;

                    case (Keys.Control | Keys.F4):
                        this.CurrentQueryControl.TryClose();
                        return true;
                }
            }
            else if (keys > (Keys.Control | Keys.Shift | Keys.F5))
            {
                switch (keys)
                {
                    case (Keys.Alt | Keys.PageUp):
                        this.CurrentQueryControl.ScrollResults(VerticalScrollAmount.Page, false);
                        return true;

                    case (Keys.Alt | Keys.PageDown):
                        this.CurrentQueryControl.ScrollResults(VerticalScrollAmount.Page, true);
                        return true;

                    case (Keys.Alt | Keys.End):
                        this.CurrentQueryControl.ScrollResults(VerticalScrollAmount.Document, true);
                        return true;

                    case (Keys.Alt | Keys.Home):
                        this.CurrentQueryControl.ScrollResults(VerticalScrollAmount.Document, false);
                        return true;

                    case (Keys.Alt | Keys.Left):
                        this.CurrentQueryControl.NavigateResultPanel(false);
                        return true;

                    case (Keys.Alt | Keys.Up):
                        this.CurrentQueryControl.ScrollResults(VerticalScrollAmount.Line, false);
                        return true;

                    case (Keys.Alt | Keys.Right):
                        this.CurrentQueryControl.NavigateResultPanel(true);
                        return true;

                    case (Keys.Alt | Keys.Down):
                        this.CurrentQueryControl.ScrollResults(VerticalScrollAmount.Line, true);
                        return true;

                    case (Keys.Alt | Keys.D0):
                        this.CurrentQueryControl.CollapseResultsTo(null);
                        return true;

                    case (Keys.Alt | Keys.D1):
                        this.CurrentQueryControl.CollapseResultsTo(1);
                        return true;

                    case (Keys.Alt | Keys.D2):
                        this.CurrentQueryControl.CollapseResultsTo(2);
                        return true;

                    case (Keys.Alt | Keys.D3):
                        this.CurrentQueryControl.CollapseResultsTo(3);
                        return true;

                    case (Keys.Alt | Keys.D4):
                        this.CurrentQueryControl.CollapseResultsTo(4);
                        return true;

                    case (Keys.Alt | Keys.D5):
                        this.CurrentQueryControl.CollapseResultsTo(5);
                        return true;

                    case (Keys.Alt | Keys.A):
                        this.CurrentQueryControl.SelectLambdaPanel(true);
                        return true;

                    case (Keys.Alt | Keys.O):
                        if (this.CurrentQueryControl.IsInDataGridMode())
                        {
                            this.CurrentQueryControl.SelectResultsPanel(true);
                            return true;
                        }
                        return false;

                    case (Keys.Alt | Keys.R):
                        this.CurrentQueryControl.SelectResultsPanel(true);
                        return true;

                    case (Keys.Alt | Keys.S):
                        this.CurrentQueryControl.SelectSqlPanel(true);
                        return true;

                    case (Keys.Alt | Keys.X):
                        goto Label_05AF;

                    case (Keys.Alt | Keys.I):
                        this.CurrentQueryControl.SelectILPanel(true);
                        return true;
                }
            }
            else if (keys > (Keys.Control | Keys.Shift | Keys.G))
            {
                switch (keys)
                {
                    case (Keys.Control | Keys.Shift | Keys.R):
                        this.CurrentQueryControl.ReflectIL();
                        return true;

                    case (Keys.Control | Keys.Shift | Keys.T):
                        this.CurrentQueryControl.Query.ToDataGrids = false;
                        return true;

                    case (Keys.Control | Keys.Shift | Keys.J):
                        this.CurrentQueryControl.JumpToExecutingLine();
                        return true;

                    case (Keys.Control | Keys.Shift | Keys.F3):
                        this.CurrentQueryControl.FindPreviousSelected();
                        return true;

                    case (Keys.Control | Keys.Shift | Keys.F5):
                        this.CurrentQueryControl.Cancel(true);
                        return true;
                }
            }
            else
            {
                switch (keys)
                {
                    case (Keys.Control | Keys.Shift | Keys.Tab):
                        goto Label_030F;

                    case (Keys.Control | Keys.Shift | Keys.G):
                        this.CurrentQueryControl.Query.ToDataGrids = true;
                        return true;
                }
            }
            return false;
        Label_030F:
            MainForm.Instance.PreviousQuery();
            return true;
        Label_05AF:
            this.CurrentQueryControl.Run();
            return true;
        }

        private bool HandleCommonQuerylessKeys(Keys keyData)
        {
            Keys keys = keyData;
            if (keys <= (Keys.Control | Keys.Shift | Keys.P))
            {
                if (keys > (Keys.Shift | Keys.F8))
                {
                    switch (keys)
                    {
                        case (Keys.Control | Keys.Shift | Keys.A):
                            UserOptionsLive.Instance.ExecutionTrackingDisabled = !UserOptionsLive.Instance.ExecutionTrackingDisabled;
                            return true;

                        case (Keys.Control | Keys.Shift | Keys.E):
                            MainForm.Instance.ToggleAutoScrollResults(true);
                            return true;

                        case (Keys.Control | Keys.Shift | Keys.P):
                            ManagePasswordsForm.ShowInstance();
                            return true;
                    }
                }
                else
                {
                    switch (keys)
                    {
                        case Keys.F6:
                            MainForm.Instance.FocusQueries();
                            return true;

                        case (Keys.Shift | Keys.F8):
                            MainForm.Instance.ToggleExplorerVisibility();
                            return true;
                    }
                }
            }
            else
            {
                switch (keys)
                {
                    case (Keys.Control | Keys.Shift | Keys.F8):
                        MainForm.Instance.SuspendLayout();
                        MainForm.Instance.ToggleVerticalResults();
                        MainForm.Instance.ToggleExplorerVisibility();
                        MainForm.Instance.ResumeLayout();
                        return true;

                    case (Keys.Alt | Keys.W):
                        MainForm.Instance.ChooseOpenQuery();
                        return true;

                    case (Keys.Alt | Keys.Shift | Keys.E):
                        MainForm.Instance.ToggleAutoScrollResults(false);
                        return true;

                    case (Keys.Alt | Keys.Shift | Keys.G):
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        return true;

                    case (Keys.Alt | Keys.Shift | Keys.O):
                        UserOptionsLive.Instance.OptimizeQueries = !UserOptionsLive.Instance.OptimizeQueries;
                        return true;

                    case (Keys.Alt | Keys.Shift | Keys.S):
                        MainForm.Instance.FocusSchemaExplorer();
                        return true;
                }
            }
            return false;
        }

        public static bool HandleKey(MainForm form, QueryControl qc, Keys keyData)
        {
            HotKeyManager manager = new HotKeyManager(form, qc);
            if (manager.HandleCommonQuerylessKeys(keyData))
            {
                return true;
            }
            if (manager.CurrentQueryControl == null)
            {
                return false;
            }
            if (manager.HandleCommonKeys(keyData))
            {
                return true;
            }
            if (UseStudioKeys)
            {
                return manager.HandleStudioKey(keyData);
            }
            return manager.HandleSSMSKey(keyData);
        }

        private bool HandleSSMSKey(Keys keyData)
        {
            switch (keyData)
            {
                case (Keys.Control | Keys.J):
                    this.ListMembers();
                    return true;

                case (Keys.Control | Keys.K):
                    this.CommentSelection();
                    return true;

                case (Keys.Control | Keys.M):
                    this.ToggleOutlining(true);
                    return true;

                case (Keys.Control | Keys.P):
                    this.InsertSnippet(false);
                    return true;

                case (Keys.Control | Keys.E):
                    this.ExecuteQuery();
                    return true;

                case (Keys.Control | Keys.U):
                    this.UncommentSelection();
                    return true;

                case (Keys.Control | Keys.W):
                    this.CloseQuery();
                    return true;

                case (Keys.Control | Keys.Shift | Keys.M):
                    this.ToggleAllOutlining();
                    return true;

                case (Keys.Control | Keys.Shift | Keys.P):
                    this.InsertSnippet(true);
                    return true;
            }
            return false;
        }

        private bool HandleStudioKey(Keys keyData)
        {
            Keys? nullable = _firstLetter;
            _firstLetter = null;
            Keys valueOrDefault = nullable.GetValueOrDefault();
            if (!nullable.HasValue)
            {
                switch (keyData)
                {
                    case (Keys.Control | Keys.E):
                    case (Keys.Control | Keys.K):
                    case (Keys.Control | Keys.M):
                    case (Keys.Control | Keys.W):
                        _firstLetter = new Keys?(keyData);
                        return true;

                    case (Keys.Control | Keys.G):
                        this.ExecuteQuery();
                        return true;

                    case (Keys.Control | Keys.J):
                        this.ListMembers();
                        return true;
                }
                return false;
            }
            switch (valueOrDefault)
            {
                case (Keys.Control | Keys.K):
                    valueOrDefault = keyData;
                    if (valueOrDefault > Keys.X)
                    {
                        if (valueOrDefault > (Keys.Control | Keys.L))
                        {
                            switch (valueOrDefault)
                            {
                                case (Keys.Control | Keys.S):
                                    goto Label_013B;

                                case (Keys.Control | Keys.T):
                                    goto Label_0149;

                                case (Keys.Control | Keys.U):
                                    goto Label_0150;
                            }
                            if (valueOrDefault != (Keys.Control | Keys.X))
                            {
                                goto Label_0149;
                            }
                            goto Label_012D;
                        }
                        if (valueOrDefault == (Keys.Control | Keys.C))
                        {
                            goto Label_00FE;
                        }
                        if (valueOrDefault == (Keys.Control | Keys.L))
                        {
                            break;
                        }
                    }
                    else if (valueOrDefault > Keys.L)
                    {
                        switch (valueOrDefault)
                        {
                            case Keys.S:
                                goto Label_013B;

                            case Keys.U:
                                goto Label_0150;

                            case Keys.X:
                                goto Label_012D;
                        }
                    }
                    else
                    {
                        switch (valueOrDefault)
                        {
                            case Keys.C:
                                goto Label_00FE;
                        }
                    }
                    goto Label_0149;

                case (Keys.Control | Keys.M):
                    switch (keyData)
                    {
                        case Keys.L:
                        case Keys.O:
                        case (Keys.Control | Keys.L):
                        case (Keys.Control | Keys.O):
                            this.ToggleAllOutlining();
                            return true;

                        case Keys.M:
                        case (Keys.Control | Keys.M):
                            this.ToggleOutlining(false);
                            return true;
                    }
                    return false;

                case (Keys.Control | Keys.W):
                    switch (keyData)
                    {
                        case Keys.L:
                        case (Keys.Control | Keys.L):
                            MainForm.Instance.FocusSchemaExplorer();
                            return true;

                        case Keys.S:
                        case (Keys.Control | Keys.S):
                            MainForm.Instance.FocusQueries();
                            return true;
                    }
                    return false;

                case (Keys.Control | Keys.E):
                    switch (keyData)
                    {
                        case Keys.C:
                        case (Keys.Control | Keys.C):
                            this.CommentSelection();
                            return true;

                        case Keys.U:
                        case (Keys.Control | Keys.U):
                            this.UncommentSelection();
                            return true;
                    }
                    return false;

                default:
                    return false;
            }
            this.ListMembers();
            return true;
        Label_00FE:
            this.CommentSelection();
            return true;
        Label_012D:
            this.InsertSnippet(false);
            return true;
        Label_013B:
            this.InsertSnippet(true);
            return true;
        Label_0149:
            return false;
        Label_0150:
            this.UncommentSelection();
            return true;
        }

        private void InsertSnippet(bool surroundWith)
        {
            if (this._mainForm.ShowLicensee)
            {
                this.CurrentQueryControl.InsertSnippet(surroundWith);
            }
        }

        private void ListMembers()
        {
            if (this._mainForm.ShowLicensee)
            {
                this.CurrentQueryControl.ListMembers();
            }
        }

        private void SaveQuery()
        {
            if (((this.CurrentQueryControl.Query.Source.Trim().Length == 0) || !this.CurrentQueryControl.Query.IsModified) || this.CurrentQueryControl.Save())
            {
                this.CurrentQueryControl.TryClose();
            }
        }

        private void ToggleAllOutlining()
        {
            if (this._mainForm.ShowLicensee)
            {
                this.CurrentQueryControl.ToggleAllOutlining();
            }
        }

        private void ToggleOutlining(bool fromControlM)
        {
            if (this._mainForm.ShowLicensee)
            {
                bool flag = this.CurrentQueryControl.ToggleOutliningExpansion();
                if (!(!fromControlM || flag))
                {
                    this.ListMembers();
                }
            }
        }

        private void UncommentSelection()
        {
            this.CurrentQueryControl.Editor.UncommentSelection();
        }

        private QueryControl CurrentQueryControl
        {
            get
            {
                return this._queryControl;
            }
        }

        public static bool UseStudioKeys
        {
            get
            {
                return !UserOptions.Instance.NativeHotKeys;
            }
        }
    }
}

