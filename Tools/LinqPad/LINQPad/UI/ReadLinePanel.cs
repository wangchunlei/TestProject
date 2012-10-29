namespace LINQPad.UI
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    internal class ReadLinePanel : UserControl
    {
        private List<string> _history;
        private int _historyIndex;
        private bool _init;
        private string[] _options;
        public Action<string> EntryMade;
        private Label lblPrompt;
        private Panel panTextBorder;
        private TextBox txtInput;

        public ReadLinePanel()
        {
            Label label = new Label {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0, 2, 0, 3)
            };
            this.lblPrompt = label;
            TextBox box = new TextBox {
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend
            };
            this.txtInput = box;
            Panel panel = new Panel {
                BackColor = Color.Black,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Top
            };
            this.panTextBorder = panel;
            this._history = new List<string>();
            this._historyIndex = 0;
            try
            {
                this.txtInput.Font = new Font("Consolas", 10f);
            }
            catch
            {
                try
                {
                    this.txtInput.Font = new Font("Courier New", 10f);
                }
                catch
                {
                }
            }
            try
            {
                this.lblPrompt.Font = new Font(FontManager.GetDefaultFont(), FontStyle.Bold);
            }
            catch
            {
            }
            this.panTextBorder.Controls.Add(this.txtInput);
            base.Controls.Add(this.panTextBorder);
            base.Controls.Add(this.lblPrompt);
        }

        public void FocusTextBox()
        {
            this.txtInput.Focus();
        }

        public void Go(string prompt, string defaultValue, string[] options)
        {
            this._init = false;
            this._options = options;
            if ((this._options != null) && (this._options.Length == 0))
            {
                this._options = null;
            }
            this.lblPrompt.Text = prompt ?? "";
            this.lblPrompt.Visible = this.lblPrompt.Text.Length > 0;
            this.txtInput.AutoCompleteSource = (this._options == null) ? AutoCompleteSource.None : AutoCompleteSource.CustomSource;
            if (this._options != null)
            {
                this.txtInput.AutoCompleteCustomSource.Clear();
                this.txtInput.AutoCompleteCustomSource.AddRange(this._options);
            }
            this.txtInput.Text = defaultValue ?? "";
            this.txtInput.SelectionStart = 0;
            this.txtInput.SelectionLength = this.txtInput.Text.Length;
            this.panTextBorder.Padding = new Padding(4, 3, 3, 3);
            this.txtInput.Focus();
            this._init = true;
            base.PerformLayout();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (this._init)
            {
                this.panTextBorder.Height = (this.EntryControl.Height + this.panTextBorder.Padding.Top) + this.panTextBorder.Padding.Bottom;
                int num = (this.panTextBorder.Height + ((this.lblPrompt.Text.Length > 0) ? this.lblPrompt.Height : 0)) + 2;
                if (base.Height != num)
                {
                    base.Height = num;
                }
            }
            base.OnLayout(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (((keyData == Keys.Up) && (this._historyIndex > 0)) && (this._options == null))
            {
                this._historyIndex--;
                this.EntryControl.Text = this._history[this._historyIndex];
                return true;
            }
            if (((keyData == Keys.Down) && (this._historyIndex < this._history.Count)) && (this._options == null))
            {
                this._historyIndex++;
                this.EntryControl.Text = (this._historyIndex == this._history.Count) ? "" : this._history[this._historyIndex];
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (this.UserText.Length > 0)
                {
                    if (this._history.Contains(this.UserText))
                    {
                        this._history.Remove(this.UserText);
                    }
                    else if (this._history.Count > 100)
                    {
                        this._history.RemoveAt(0);
                    }
                    this._history.Add(this.UserText);
                    this._historyIndex = this._history.Count;
                }
                if (this.EntryMade != null)
                {
                    this.EntryMade(this.UserText);
                }
                return true;
            }
            if (keyData == Keys.Escape)
            {
                this.EntryControl.Text = "";
                this._historyIndex = this._history.Count;
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private Control EntryControl
        {
            get
            {
                return this.txtInput;
            }
        }

        public string UserText
        {
            get
            {
                return this.EntryControl.Text;
            }
        }
    }
}

