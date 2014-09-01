using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceDemo
{
    internal class Desktop
    {
        private IntPtr m_hCurWinsta = IntPtr.Zero;
        private IntPtr m_hCurDesktop = IntPtr.Zero;
        private IntPtr m_hWinsta = IntPtr.Zero;
        private IntPtr m_hDesk = IntPtr.Zero;

        /// <summary>
        /// associate the current thread to the default desktop
        /// </summary>
        /// <returns></returns>
        internal bool BeginInteraction()
        {
            EndInteraction();
            m_hCurWinsta = User32DLL.GetProcessWindowStation();
            if (m_hCurWinsta == IntPtr.Zero)
                return false;

            m_hCurDesktop = User32DLL.GetDesktopWindow();
            if (m_hCurDesktop == IntPtr.Zero)
                return false;

            m_hWinsta = User32DLL.OpenWindowStation("winsta0", false,
                WindowStationAccessRight.WINSTA_ACCESSCLIPBOARD |
                WindowStationAccessRight.WINSTA_ACCESSGLOBALATOMS |
                WindowStationAccessRight.WINSTA_CREATEDESKTOP |
                WindowStationAccessRight.WINSTA_ENUMDESKTOPS |
                WindowStationAccessRight.WINSTA_ENUMERATE |
                WindowStationAccessRight.WINSTA_EXITWINDOWS |
                WindowStationAccessRight.WINSTA_READATTRIBUTES |
                WindowStationAccessRight.WINSTA_READSCREEN |
                WindowStationAccessRight.WINSTA_WRITEATTRIBUTES
                );
            if (m_hWinsta == IntPtr.Zero)
                return false;

            User32DLL.SetProcessWindowStation(m_hWinsta);

            m_hDesk = User32DLL.OpenDesktop("default", OpenDesktopFlag.DF_NONE, false,
                    DesktopAccessRight.DESKTOP_CREATEMENU |
                    DesktopAccessRight.DESKTOP_CREATEWINDOW |
                    DesktopAccessRight.DESKTOP_ENUMERATE |
                    DesktopAccessRight.DESKTOP_HOOKCONTROL |
                    DesktopAccessRight.DESKTOP_JOURNALPLAYBACK |
                    DesktopAccessRight.DESKTOP_JOURNALRECORD |
                    DesktopAccessRight.DESKTOP_READOBJECTS |
                    DesktopAccessRight.DESKTOP_SWITCHDESKTOP |
                    DesktopAccessRight.DESKTOP_WRITEOBJECTS
                    );
            if (m_hDesk == IntPtr.Zero)
                return false;

            User32DLL.SetThreadDesktop(m_hDesk);

            return true;
        }

        /// <summary>
        /// restore
        /// </summary>
        internal void EndInteraction()
        {
            if (m_hCurWinsta != IntPtr.Zero)
                User32DLL.SetProcessWindowStation(m_hCurWinsta);

            if (m_hCurDesktop != IntPtr.Zero)
                User32DLL.SetThreadDesktop(m_hCurDesktop);

            if (m_hWinsta != IntPtr.Zero)
                User32DLL.CloseWindowStation(m_hWinsta);

            if (m_hDesk != IntPtr.Zero)
                User32DLL.CloseDesktop(m_hDesk);
        }
    }
}
