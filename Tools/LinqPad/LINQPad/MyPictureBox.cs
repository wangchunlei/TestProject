namespace LINQPad
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;

    public class MyPictureBox : PictureBox
    {
        public override object InitializeLifetimeService()
        {
            File.AppendAllText(@"d:\ls.txt", string.Concat(new object[] { "ILS: ", DateTime.Now, " ", new StackTrace(), "\r\n\r\n" }));
            return base.InitializeLifetimeService();
        }
    }
}

