namespace LINQPad.UI
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;

    internal class FixedLinkLabel : Label
    {
        private Font _defaultFont;
        private bool _inDesigner;
        private bool _mouseDown;
        private Color _oldColor = Color.Blue;

        public event EventHandler LinkClicked;

        public FixedLinkLabel()
        {
            this.Cursor = Cursors.Hand;
            this.ForeColor = (SystemColors.Window.GetBrightness() < 0.5f) ? SystemColors.HotTrack : Color.Blue;
            this._defaultFont = new Font(FontManager.GetDefaultFont(), FontStyle.Underline);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.LinkClicked(this, e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                this._oldColor = this.ForeColor;
                this.ForeColor = Color.Red;
                this._mouseDown = true;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (this._mouseDown)
            {
                this._mouseDown = false;
                this.ForeColor = this._oldColor;
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (!this._inDesigner)
            {
                this.Font = this._defaultFont;
            }
        }

        public override void ResetCursor()
        {
            this.Cursor = Cursors.Hand;
        }

        public override void ResetForeColor()
        {
            this.ForeColor = Color.Blue;
        }

        private bool ShouldSerializeCursor()
        {
            return (this.Cursor != Cursors.Hand);
        }

        private bool ShouldSerializeForeColor()
        {
            return (this.ForeColor != Color.Blue);
        }

        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;
                this._inDesigner = true;
            }
        }
    }
}

