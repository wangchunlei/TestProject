namespace LINQPad
{
    using System;
    using System.Windows.Forms;

    internal static class VersionCheck
    {
        public static bool Check()
        {
            if (((Environment.Version.Major == 4) && (Environment.Version.Minor == 0)) && (Environment.Version.Build < 0x520e))
            {
                MessageBox.Show("LINQPad 4 will not on on Framework 4.0 releases earlier than Beta 2", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            return true;
        }

        private static bool Is35Available()
        {
            try
            {
                return (Type.GetType("System.Data.Linq.Mapping.ColumnAttribute, System.Data.Linq, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089") != null);
            }
            catch
            {
                return false;
            }
        }
    }
}

