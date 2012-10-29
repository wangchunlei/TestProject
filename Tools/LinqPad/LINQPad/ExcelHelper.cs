namespace LINQPad
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class ExcelHelper
    {
        public static void OpenInExcel(string filePath, bool autoFitColumns)
        {
            object obj2;
            object obj3;
            object obj4;
            object obj5;
            object obj6;
            object obj7;
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            object target = obj7 = obj6 = obj5 = obj4 = obj3 = obj2 = null;
            try
            {
                Type typeFromProgID;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                try
                {
                    try
                    {
                        typeFromProgID = Type.GetTypeFromProgID("Excel.Application", true);
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.InnerException;
                    }
                }
                catch (COMException exception2)
                {
                    Log.Write(exception2, "Export to Excel");
                    MessageBox.Show("Cannot open Excel - is it installed?", "LINQPad", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                target = Activator.CreateInstance(typeFromProgID);
                Type type2 = target.GetType();
                obj7 = type2.InvokeMember("Workbooks", BindingFlags.GetProperty, null, target, null);
                obj6 = obj7.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, obj7, new object[] { filePath });
                try
                {
                    obj5 = type2.InvokeMember("ActiveWindow", BindingFlags.GetProperty, null, target, null);
                    obj5.GetType().InvokeMember("DisplayGridlines", BindingFlags.SetProperty, null, obj5, new object[] { true });
                }
                catch
                {
                }
                if (autoFitColumns)
                {
                    try
                    {
                        obj4 = obj6.GetType().InvokeMember("ActiveSheet", BindingFlags.GetProperty, null, obj6, null);
                        obj3 = obj4.GetType().InvokeMember("Columns", BindingFlags.GetProperty, null, obj4, null);
                        obj2 = obj3.GetType().InvokeMember("EntireColumn", BindingFlags.GetProperty, null, obj3, null);
                        obj2.GetType().InvokeMember("AutoFit", BindingFlags.InvokeMethod, null, obj2, null);
                    }
                    catch
                    {
                    }
                }
                try
                {
                    obj6.GetType().InvokeMember("Saved", BindingFlags.SetProperty, null, obj6, new object[] { true });
                }
                catch
                {
                }
                type2.InvokeMember("Visible", BindingFlags.SetProperty, null, target, new object[] { true });
                try
                {
                    object obj9 = type2.InvokeMember("Hwnd", BindingFlags.GetProperty, null, target, null);
                    if ((obj9 is int) && (((int) obj9) != 0))
                    {
                        Native.SetForegroundWindow((IntPtr) ((int) obj9));
                    }
                }
                catch
                {
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
                foreach (object obj10 in new object[] { target, obj7, obj6, obj5, obj4, obj3, obj2 }.Reverse<object>())
                {
                    if (obj10 != null)
                    {
                        try
                        {
                            Marshal.FinalReleaseComObject(obj10);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }
    }
}

