namespace LINQPad
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    internal static class Native
    {
        private static StringBuilder _currentProcessText = new StringBuilder(0x3e8);
        public const int GW_HWNDFIRST = 0;
        public const int GW_HWNDLAST = 1;
        public const int GW_HWNDNEXT_below = 2;
        public const int GW_HWNDPREV_above = 3;
        public const uint MA_ACTIVATE = 1;
        public const uint MA_ACTIVATEANDEAT = 2;
        public const uint MA_NOACTIVATE = 3;
        public const uint MA_NOACTIVATEANDEAT = 4;
        public const uint SB_BOTTOM = 7;
        public const uint SB_ENDSCROLL = 8;
        public const int SB_HORZ = 0;
        public const uint SB_LINEDOWN = 1;
        public const uint SB_LINEUP = 0;
        public const uint SB_PAGEDOWN = 3;
        public const uint SB_PAGEUP = 2;
        public const uint SB_THUMBPOSITION = 4;
        public const uint SB_THUMBTRACK = 5;
        public const uint SB_TOP = 6;
        public const int SB_VERT = 1;
        public const uint SC_RESTORE = 0xf120;
        public const int WM_ACTIVATE = 6;
        public const uint WM_HSCROLL = 0x114;
        public const uint WM_MOUSEACTIVATE = 0x21;
        public const int WM_NCACTIVATE = 0x86;
        public const uint WM_NCLBUTTONDBLCLK = 0xa3;
        public const uint WM_SYSCOMMAND = 0x112;
        public const uint WM_SYSKEYDOWN = 260;
        public const uint WM_VSCROLL = 0x115;

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(uint threadId, EnumThreadDelegate threadFunc, IntPtr lParam);
        public static string GetCurrentProcessName()
        {
            uint num;
            GetWindowThreadProcessId(GetForegroundWindow(), out num);
            IntPtr hWnd = OpenProcess(0x410, false, num);
            try
            {
                _currentProcessText.Length = 0;
                GetModuleBaseName(hWnd, IntPtr.Zero, _currentProcessText, _currentProcessText.Capacity);
            }
            finally
            {
                try
                {
                    CloseHandle(hWnd);
                }
                catch
                {
                }
            }
            return _currentProcessText.ToString();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        public static int GetLineCount(TextBox textBox)
        {
            return SendMessage(textBox.Handle, 0xba, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }

        [DllImport("psapi.dll")]
        public static extern uint GetModuleBaseName(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);
        public static Point GetScrollPos(Control c)
        {
            try
            {
                return new Point(GetScrollPos(c.Handle, 0), GetScrollPos(c.Handle, 1));
            }
            catch
            {
                return new Point(-1, -1);
            }
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetScrollPos(IntPtr hWnd, int nBar);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        public static IntPtr SendMessage(IntPtr hWnd, uint msg)
        {
            return SendMessage(hWnd, msg, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.DLL")]
        public static extern int SetForegroundWindow(IntPtr hwnd);
        public static void SetScrollPos(Control c, Point scrollPosition)
        {
            if (scrollPosition != new Point(-1, -1))
            {
                try
                {
                    SetScrollPos(c.Handle, 0, scrollPosition.X, true);
                    SetScrollPos(c.Handle, 1, scrollPosition.Y, true);
                }
                catch
                {
                }
            }
        }

        [DllImport("user32.dll")]
        public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint flags);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);

        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        public enum ShowWindowCommands
        {
            ForceMinimize = 11,
            Hide = 0,
            Maximize = 3,
            Minimize = 6,
            Normal = 1,
            Restore = 9,
            Show = 5,
            ShowDefault = 10,
            ShowMaximized = 3,
            ShowMinimized = 2,
            ShowMinNoActive = 7,
            ShowNA = 8,
            ShowNoActivate = 4
        }
    }
}

