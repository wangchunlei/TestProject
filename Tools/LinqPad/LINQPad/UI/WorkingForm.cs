namespace LINQPad.UI
{
    using LINQPad;
    using System;
    using System.Windows.Forms;

    internal class WorkingForm : BaseForm
    {
        public WorkingForm(string text, int msTimeout)
        {
            base.StartPosition = FormStartPosition.CenterScreen;
            base.ControlBox = false;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            Label label = new Label {
                Text = text,
                AutoSize = true,
                Padding = new Padding(30)
            };
            base.Controls.Add(label);
            this.AutoSize = true;
            base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Program.RunOnWinFormsTimer(new Action(this.Close), msTimeout);
        }

        public static WorkingForm FlashForm(string text, int msTimeout)
        {
            WorkingForm form = new WorkingForm(text, msTimeout);
            Native.ShowWindow(form.Handle, Native.ShowWindowCommands.ShowNoActivate);
            form.Update();
            return form;
        }
    }
}

