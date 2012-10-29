namespace LINQPad.UI
{
    using ActiproSoftware.SyntaxEditor;
    using LINQPad;
    using LINQPad.Properties;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    internal class ResultStylesForm : BaseForm
    {
        private Rectangle _screenBounds;
        private ActiproSoftware.SyntaxEditor.SyntaxEditor baseEditor;
        private Button btnApply;
        private CheckBox chkTopMost;
        private IContainer components;
        private ActiproSoftware.SyntaxEditor.SyntaxEditor customEditor;
        private static ResultStylesForm Instance;
        private Label label1;
        private Label label2;
        private Panel panBottom;
        public readonly string Path;
        private SplitContainer splitContainer;

        public ResultStylesForm(string path, Rectangle screenBounds)
        {
            SyntaxLanguage language;
            int num;
            this.components = null;
            Instance = this;
            this.Path = path;
            this._screenBounds = screenBounds;
            this.InitializeComponent();
            base.Icon = Resources.LINQPad;
            this.customEditor.get_Document().set_Language(language = DocumentManager.GetDynamicLanguage("CSS", SystemColors.Window.GetBrightness()));
            this.baseEditor.get_Document().set_Language(language);
            this.baseEditor.get_Document().set_Text("body\r\n{\r\n\tmargin: 0.3em 0.3em 0.4em 0.5em;\r\n\tfont-family: Verdana;\r\n\tfont-size: 80%;\r\n\tbackground: white;\r\n}\r\n\r\np, pre\r\n{\r\n\tmargin:0;\r\n\tpadding:0;\r\n\tfont-family: Verdana;\r\n}\r\n\r\ntable\r\n{\r\n\tborder-collapse: collapse;\r\n\tborder: 2px solid #17b;\r\n\tborder-top: 1px;\r\n\tmargin: 0.3em 0.2em;\r\n}\r\n\r\ntable.limit\r\n{\r\n\tborder-collapse: collapse;\r\n\tborder-bottom: 2px solid #c31;\r\n}\r\n\r\ntd, th\r\n{\r\n\tvertical-align: top;\r\n\tborder: 1px solid #aaa;\r\n\tpadding: 0.1em 0.2em;\r\n\tmargin: 0;\r\n}\r\n\r\nth\r\n{\r\n\ttext-align: left;\r\n\tbackground-color: #ddd;\r\n\tborder: 1px solid #777;\r\n\tfont-family: tahoma;\r\n\tfont-size:90%;\r\n\tfont-weight: bold;\r\n}\r\n\r\nth.member\r\n{\r\n\tpadding: 0.1em 0.2em 0.1em 0.2em;\r\n}\r\n\r\ntd.typeheader\r\n{\r\n\tfont-family: tahoma;\r\n\tfont-size: 90%;\r\n\tfont-weight: bold;\r\n\tbackground-color: #17b;\r\n\tcolor: white;\r\n\tpadding: 0 0.2em 0.1em 0.1em;\r\n}\r\n\r\ntd.n { text-align: right }\r\n\r\na:link.typeheader, a:visited.typeheader\r\n{\r\n\tfont-family: tahoma;\r\n\tfont-size: 90%;\r\n\tfont-weight: bold;\r\n\ttext-decoration: none;\r\n\tbackground-color: #17b;\r\n\tcolor: white;\r\n\tfloat:left;\r\n}\r\n\r\nspan.typeglyph\r\n{\r\n\tfont-family: webdings;\r\n\tpadding: 0 0.2em 0 0;\r\n\tmargin: 0;\r\n}\r\n\r\ntable.group\r\n{\r\n\tborder: none;\r\n\tmargin: 0;\r\n}\r\n\r\ntd.group\r\n{\r\n\tborder: none;\r\n\tpadding: 0 0.1em;\r\n}\r\n\r\ndiv.spacer\r\n{\r\n\tmargin: 0.6em 0;\r\n}\r\n\r\ntable.headingpresenter\r\n{\r\n\tborder: none;\r\n\tborder-left: 3px dotted #1a5;\r\n\tmargin: 1em 0em 1.2em 0.15em;\r\n}\r\n\r\nth.headingpresenter\r\n{\r\n\tfont-family: Arial;\r\n\tborder: none;\r\n\tpadding: 0 0 0.2em 0.5em;\r\n\tbackground-color: white;\r\n\tcolor: green;\r\n\tfont-size: 110%;        \r\n}\r\n\r\ntd.headingpresenter\r\n{\r\n\tborder: none;\r\n\tpadding: 0 0 0 0.6em;\r\n}\r\n\r\ntd.summary\r\n{ \r\n\tbackground-color: #def;\r\n\tcolor: #024;\r\n\tfont-family: Tahoma;\r\n\tpadding: 0 0.1em 0.1em 0.1em;\r\n}\r\n\r\ntd.columntotal\r\n{\r\n\tfont-family: Tahoma;\r\n\tbackground-color: #eee;\r\n\tfont-weight: bold;\r\n\tcolor: #17b;\r\n\tfont-size:90%;\r\n\ttext-align:right;\r\n}\r\n\r\nspan.graphbar\r\n{\r\n\tbackground: #17b;\r\n\tcolor: #17b;\r\n\tmargin-left: -2px;\r\n\tmargin-right: -2px;\r\n}\r\n\r\na:link.graphcolumn, a:visited.graphcolumn\r\n{\r\n\tcolor: #17b;\r\n\ttext-decoration: none;\r\n\tfont-weight: bold;\r\n\tfont-family: Arial;\r\n\tfont-size: 110%;\r\n\tletter-spacing: -0.4em;\t\r\n\tmargin-left: 0.3em;\r\n}\r\n\r\ni { color: green; }\r\n\r\nem { color: red; }\r\n\r\nspan.highlight { background: #ff8; }");
            if (File.Exists(path))
            {
                this.customEditor.get_Document().LoadFile(path);
            }
            this.customEditor.get_Document().set_Modified(false);
            this.baseEditor.set_SelectionMarginWidth(num = this.Font.Height / 2);
            this.customEditor.set_SelectionMarginWidth(num);
            try
            {
                this.customEditor.Font = this.baseEditor.Font = new Font("Consolas", 9.5f);
            }
            catch
            {
            }
            this.EnableControls();
            this.chkTopMost.Checked = false;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            this.SaveChanges();
        }

        private void chkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            base.TopMost = this.chkTopMost.Checked;
        }

        private void customEditor_TextChanged(object sender, EventArgs e)
        {
            this.EnableControls();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void EnableControls()
        {
            this.btnApply.Enabled = this.customEditor.get_Document().get_Modified();
        }

        private void InitializeComponent()
        {
            Document document = new Document();
            Document document2 = new Document();
            this.customEditor = new ActiproSoftware.SyntaxEditor.SyntaxEditor();
            this.panBottom = new Panel();
            this.chkTopMost = new CheckBox();
            this.btnApply = new Button();
            this.baseEditor = new ActiproSoftware.SyntaxEditor.SyntaxEditor();
            this.splitContainer = new SplitContainer();
            this.label2 = new Label();
            this.label1 = new Label();
            this.panBottom.SuspendLayout();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            base.SuspendLayout();
            this.customEditor.set_BracketHighlightingVisible(true);
            this.customEditor.Dock = DockStyle.Fill;
            this.customEditor.set_Document(document);
            this.customEditor.set_IndicatorMarginVisible(false);
            this.customEditor.Location = new Point(0, 0x15);
            this.customEditor.Name = "customEditor";
            this.customEditor.set_ScrollBarType(3);
            this.customEditor.Size = new Size(0x1b1, 240);
            this.customEditor.set_SplitType(0);
            this.customEditor.TabIndex = 0;
            this.customEditor.TextChanged += new EventHandler(this.customEditor_TextChanged);
            this.panBottom.Controls.Add(this.chkTopMost);
            this.panBottom.Controls.Add(this.btnApply);
            this.panBottom.Dock = DockStyle.Bottom;
            this.panBottom.Location = new Point(5, 0x27a);
            this.panBottom.Name = "panBottom";
            this.panBottom.Padding = new Padding(0, 6, 0, 0);
            this.panBottom.Size = new Size(0x1b1, 0x23);
            this.panBottom.TabIndex = 1;
            this.chkTopMost.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.chkTopMost.AutoSize = true;
            this.chkTopMost.Checked = true;
            this.chkTopMost.CheckState = CheckState.Checked;
            this.chkTopMost.Location = new Point(1, 11);
            this.chkTopMost.Name = "chkTopMost";
            this.chkTopMost.Size = new Size(0x9f, 0x17);
            this.chkTopMost.TabIndex = 3;
            this.chkTopMost.Text = "Keep Window on Top";
            this.chkTopMost.UseVisualStyleBackColor = true;
            this.chkTopMost.CheckedChanged += new EventHandler(this.chkTopMost_CheckedChanged);
            this.btnApply.Dock = DockStyle.Right;
            this.btnApply.Location = new Point(0x13d, 6);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new Size(0x74, 0x1d);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "&Apply Changes";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new EventHandler(this.btnApply_Click);
            this.baseEditor.set_BracketHighlightingVisible(true);
            this.baseEditor.Dock = DockStyle.Fill;
            document2.set_ReadOnly(true);
            this.baseEditor.set_Document(document2);
            this.baseEditor.set_IndicatorMarginVisible(false);
            this.baseEditor.Location = new Point(0, 0x15);
            this.baseEditor.Name = "baseEditor";
            this.baseEditor.set_ScrollBarType(3);
            this.baseEditor.Size = new Size(0x1b1, 340);
            this.baseEditor.set_SplitType(0);
            this.baseEditor.TabIndex = 2;
            this.baseEditor.set_UseDisabledRenderingForReadOnlyMode(true);
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.Location = new Point(5, 6);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = Orientation.Horizontal;
            this.splitContainer.Panel1.Controls.Add(this.baseEditor);
            this.splitContainer.Panel1.Controls.Add(this.label2);
            this.splitContainer.Panel2.Controls.Add(this.customEditor);
            this.splitContainer.Panel2.Controls.Add(this.label1);
            this.splitContainer.Size = new Size(0x1b1, 0x274);
            this.splitContainer.SplitterDistance = 0x169;
            this.splitContainer.SplitterWidth = 6;
            this.splitContainer.TabIndex = 0;
            this.label2.AutoSize = true;
            this.label2.Dock = DockStyle.Top;
            this.label2.Location = new Point(0, 0);
            this.label2.Margin = new Padding(0, 0, 3, 0);
            this.label2.Name = "label2";
            this.label2.Padding = new Padding(0, 0, 0, 2);
            this.label2.Size = new Size(0x4c, 0x15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Base Styles";
            this.label1.AutoSize = true;
            this.label1.Dock = DockStyle.Top;
            this.label1.Location = new Point(0, 0);
            this.label1.Margin = new Padding(0, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new Padding(0, 0, 0, 2);
            this.label1.Size = new Size(0x67, 0x15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Customizations";
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x1bb, 0x2a2);
            base.Controls.Add(this.splitContainer);
            base.Controls.Add(this.panBottom);
            base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.Name = "ResultStylesForm";
            base.Padding = new Padding(5, 6, 5, 5);
            base.StartPosition = FormStartPosition.Manual;
            this.Text = "LINQPad Stylesheet Editor";
            this.panBottom.ResumeLayout(false);
            this.panBottom.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            this.splitContainer.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        internal static void Launch()
        {
            if (Instance != null)
            {
                Action method = new Action(Instance.Activate);
                if (Instance.InvokeRequired)
                {
                    Instance.Invoke(method);
                }
                else
                {
                    method();
                }
            }
            else
            {
                Rectangle r = Screen.GetWorkingArea(MainForm.Instance);
                Thread thread2 = new Thread(() => new ResultStylesForm(Options.CustomStyleSheetLocation, r).ShowDialog()) {
                    IsBackground = true,
                    Name = "Results Styles Editor"
                };
                thread2.SetApartmentState(ApartmentState.STA);
                thread2.Start();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Instance = null;
            base.OnClosed(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.customEditor.get_Document().get_Modified())
            {
                DialogResult result = MessageBox.Show("Save changes to stylesheet?", "LINQPad", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if ((result == DialogResult.Yes) && !this.SaveChanges())
                {
                    e.Cancel = true;
                }
            }
            base.OnClosing(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Rectangle rectangle = this._screenBounds;
            base.Left = rectangle.Right - base.Width;
            base.Height = (rectangle.Height * 3) / 4;
            base.Top = rectangle.Top + (rectangle.Height / 8);
        }

        private bool SaveChanges()
        {
            try
            {
                if (this.customEditor.get_Document().get_Text().Trim().Length > 0)
                {
                    this.customEditor.get_Document().SaveFile(this.Path, 2);
                }
                else if (File.Exists(this.Path))
                {
                    File.Delete(this.Path);
                }
                this.customEditor.get_Document().set_Modified(false);
                this.EnableControls();
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error saving file " + this.Path + "\r\n" + exception.Message, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
        }
    }
}

