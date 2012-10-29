namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    internal class ResultsDockForm : BaseForm
    {
        private bool _active;
        private MainForm _mainForm;
        private bool _resultsTorn;
        private bool _shown;
        private IContainer components = null;

        public ResultsDockForm(MainForm mainForm)
        {
            this._mainForm = mainForm;
            this.InitializeComponent();
            base.Icon = mainForm.Icon;
            base.Padding = new Padding(3);
            this.AutoPosition();
        }

        private void AutoPosition()
        {
            Screen screen = Screen.AllScreens.FirstOrDefault<Screen>(a => a.DeviceName != Screen.FromControl(this._mainForm).DeviceName);
            if (screen != null)
            {
                base.WindowState = FormWindowState.Normal;
                base.Bounds = screen.WorkingArea;
                base.WindowState = FormWindowState.Maximized;
            }
        }

        internal void CheckQuery(QueryControl qc)
        {
            if ((this._resultsTorn && (qc != null)) && (this.FindQueryControlPanel(qc) == null))
            {
                Control control = qc.DetachResultsControl();
                control.Tag = qc;
                base.Controls.Add(control);
                control.BringToFront();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private Control FindQueryControlPanel(QueryControl qc)
        {
            return base.Controls.Cast<Control>().FirstOrDefault<Control>(c => (c.Tag == qc));
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x288, 0x21a);
            base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            base.Name = "ResultsDockForm";
            base.StartPosition = FormStartPosition.Manual;
            base.ShowInTaskbar = false;
            this.Text = "LINQPad Results";
            base.ResumeLayout(false);
        }

        protected override void OnActivated(EventArgs e)
        {
            this._active = true;
            base.OnActivated(e);
            if (!this._shown)
            {
                this._shown = true;
                this.AutoPosition();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this._resultsTorn)
            {
                this.UntearResults();
            }
            base.Hide();
            e.Cancel = true;
            base.OnClosing(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            this._active = false;
            base.OnDeactivate(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            Action a = delegate {
                foreach (Control control in base.Controls.Cast<Control>().ToArray<Control>())
                {
                    QueryControl tag = control.Tag as QueryControl;
                    if (tag != null)
                    {
                        tag.AncestorMoved();
                    }
                }
            };
            a();
            Program.RunOnWinFormsTimer(a, 100);
        }

        internal void QueryActivated(QueryControl qc)
        {
            if (this._resultsTorn && (qc != null))
            {
                Control control = this.FindQueryControlPanel(qc);
                if (control != null)
                {
                    control.Show();
                    control.BringToFront();
                }
            }
        }

        internal void QueryClosed(QueryControl qc)
        {
            if (this._resultsTorn && (qc != null))
            {
                Control control = this.FindQueryControlPanel(qc);
                if (control != null)
                {
                    control.Dispose();
                }
            }
        }

        private void TearResults()
        {
            foreach (QueryControl control in this._mainForm.GetQueryControls())
            {
                Control control2 = control.DetachResultsControl();
                control2.Tag = control;
                base.Controls.Add(control2);
            }
            this._resultsTorn = true;
            this.QueryActivated(this._mainForm.CurrentQueryControl);
            Rectangle r = base.Bounds;
            if ((r.Width > 5) && (r.Height > 5))
            {
                r.Inflate(-5, -5);
            }
            if (!Screen.AllScreens.Any<Screen>(s => s.WorkingArea.IntersectsWith(r)))
            {
                this.AutoPosition();
            }
            base.Show();
            this._mainForm.Activate();
        }

        public void ToggleResultsDock()
        {
            if (!this._resultsTorn)
            {
                this.TearResults();
            }
            else
            {
                this.UntearResults();
            }
        }

        private void UntearResults()
        {
            foreach (Control control in base.Controls.Cast<Control>().ToArray<Control>())
            {
                QueryControl tag = control.Tag as QueryControl;
                if (!((tag == null) || tag.IsDisposed))
                {
                    control.Parent = null;
                    tag.AttachResultsControl(control);
                }
                else
                {
                    control.Dispose();
                }
            }
            this._resultsTorn = false;
            base.Hide();
        }

        public bool AreResultsTorn
        {
            get
            {
                return this._resultsTorn;
            }
        }

        public bool IsActive
        {
            get
            {
                return this._active;
            }
        }
    }
}

