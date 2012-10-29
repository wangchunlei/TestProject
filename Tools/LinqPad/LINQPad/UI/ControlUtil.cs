namespace LINQPad.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class ControlUtil
    {
        public static T FindAncestorOfType<T>(Control c)
        {
            return GetAncestors(c).OfType<T>().FirstOrDefault<T>();
        }

        public static IEnumerable<Control> GetAncestors(Control c)
        {
            return new <GetAncestors>d__0(-2) { <>3__c = c };
        }

        public static Image ResizeImage(Image img, int width, int height, bool addMargin)
        {
            Image image = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                if (addMargin)
                {
                    graphics.DrawImage(img, (int) (width / 8), (int) (height / 8), (int) ((width * 3) / 4), (int) ((height * 3) / 4));
                    return image;
                }
                graphics.DrawImage(img, 0, 0, width, height);
            }
            return image;
        }

        [CompilerGenerated]
        private sealed class <GetAncestors>d__0 : IEnumerable<Control>, IEnumerable, IEnumerator<Control>, IEnumerator, IDisposable
        {
            private bool $__disposing;
            private int <>1__state;
            private Control <>2__current;
            public Control <>3__c;
            private int <>l__initialThreadId;
            public Control c;

            [DebuggerHidden]
            public <GetAncestors>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                try
                {
                    if (this.<>1__state != 1)
                    {
                        if (this.<>1__state == -1)
                        {
                            return false;
                        }
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.c = this.c.Parent;
                    }
                    else
                    {
                        if (this.$__disposing)
                        {
                            return false;
                        }
                        this.<>1__state = 0;
                        this.c = this.c.Parent;
                    }
                    if (this.c != null)
                    {
                        this.<>2__current = this.c;
                        this.<>1__state = 1;
                        return true;
                    }
                }
                catch (Exception)
                {
                    this.<>1__state = -1;
                    throw;
                }
                this.<>1__state = -1;
                return false;
            }

            [DebuggerHidden]
            IEnumerator<Control> IEnumerable<Control>.GetEnumerator()
            {
                ControlUtil.<GetAncestors>d__0 d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new ControlUtil.<GetAncestors>d__0(0);
                }
                d__.c = this.<>3__c;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Windows.Forms.Control>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                this.$__disposing = true;
                this.MoveNext();
                this.<>1__state = -1;
            }

            Control IEnumerator<Control>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

