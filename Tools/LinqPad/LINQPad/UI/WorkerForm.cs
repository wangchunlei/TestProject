namespace LINQPad.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class WorkerForm : BaseForm
    {
        private bool _canceled;
        private bool _closeOnComplete;
        private bool _done;
        private bool _error;
        private BackgroundWorker _worker;
        private Button btnClose;
        private IContainer components;
        private Label lblProgress;
        private Panel panel1;
        private ProgressBar progressBar;

        public WorkerForm(BackgroundWorker worker, string workingCaption, bool closeOnComplete)
        {
            ProgressChangedEventHandler handler = null;
            this.components = null;
            this._worker = worker;
            this._closeOnComplete = closeOnComplete;
            this.InitializeComponent();
            if (worker.WorkerReportsProgress)
            {
                this.progressBar.Style = ProgressBarStyle.Continuous;
                if (handler == null)
                {
                    handler = delegate (object sender, ProgressChangedEventArgs e) {
                        this.progressBar.Value = e.ProgressPercentage;
                        if (e.UserState is string)
                        {
                            this.Text = this.lblProgress.Text = (string) e.UserState;
                        }
                    };
                }
                this._worker.ProgressChanged += handler;
            }
            this.Text = this.lblProgress.Text = workingCaption;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (this._done)
            {
                base.DialogResult = this._error ? DialogResult.Cancel : DialogResult.OK;
            }
            else
            {
                this._canceled = true;
                this._worker.CancelAsync();
                base.DialogResult = DialogResult.Cancel;
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

        private void InitializeComponent()
        {
            this.panel1 = new Panel();
            this.btnClose = new Button();
            this.lblProgress = new Label();
            this.progressBar = new ProgressBar();
            this.panel1.SuspendLayout();
            base.SuspendLayout();
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = DockStyle.Bottom;
            this.panel1.Location = new Point(12, 0x59);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new Padding(0, 0x17, 0, 0);
            this.panel1.Size = new Size(0x146, 0x34);
            this.panel1.TabIndex = 1;
            this.btnClose.DialogResult = DialogResult.Cancel;
            this.btnClose.Dock = DockStyle.Right;
            this.btnClose.Location = new Point(0xf6, 0x17);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(80, 0x1d);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Cancel";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            this.lblProgress.AutoSize = true;
            this.lblProgress.Dock = DockStyle.Top;
            this.lblProgress.Location = new Point(12, 12);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new Size(70, 0x13);
            this.lblProgress.TabIndex = 0;
            this.lblProgress.Text = "Working...";
            this.progressBar.Dock = DockStyle.Bottom;
            this.progressBar.Location = new Point(12, 0x42);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(0x146, 0x17);
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 2;
            base.AutoScaleDimensions = new SizeF(7f, 17f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.CancelButton = this.btnClose;
            base.ClientSize = new Size(350, 0x99);
            base.ControlBox = false;
            base.Controls.Add(this.progressBar);
            base.Controls.Add(this.lblProgress);
            base.Controls.Add(this.panel1);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Margin = new Padding(3, 4, 3, 4);
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "WorkerForm";
            base.Padding = new Padding(12);
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Working...";
            this.panel1.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!this._canceled && !base.IsDisposed)
            {
                if (e.Error != null)
                {
                    this.progressBar.Hide();
                    base.DialogResult = DialogResult.Cancel;
                    this._done = true;
                    this._error = true;
                    Exception error = e.Error;
                    if ((error is TargetInvocationException) && (error.InnerException != null))
                    {
                        error = error.InnerException;
                    }
                    string text = "Error: " + error.Message;
                    if (error.InnerException != null)
                    {
                        text = text + "\r\n\r\n" + error.InnerException.Message;
                    }
                    MessageBox.Show(text, "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    this.lblProgress.Text = "Completed.";
                    this.Result = e.Result;
                    this.btnClose.Text = "Close";
                    base.AcceptButton = this.btnClose;
                    this._done = true;
                    if (!(!this._closeOnComplete || this._error))
                    {
                        base.DialogResult = DialogResult.OK;
                    }
                }
            }
        }

        public object Result { get; private set; }
    }
}

