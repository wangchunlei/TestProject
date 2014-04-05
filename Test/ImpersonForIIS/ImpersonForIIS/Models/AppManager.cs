using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImpersonForIIS.Models
{
    public class AppManager
    {
        public string RunApp(string appFullName)
        {
            var results = "ok";
            // Get the full file path
            //string strFilePath = "c:\\temp\\test.bat";
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(appFullName);
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;
            //psi.WorkingDirectory = "c:\\temp\\";

            // Start the process
            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);
            // Open the batch file for reading
            //System.IO.StreamReader strm = System.IO.File.OpenText(strFilePath);
            //// Attach the output for reading
            //System.IO.StreamReader sOut = proc.StandardOutput;
            //// Attach the in for writing
            //System.IO.StreamWriter sIn = proc.StandardInput;

            //// Write each line of the batch file to standard input
            //while (strm.Peek() != -1)
            //{
            //    sIn.WriteLine(strm.ReadLine());
            //}
            //strm.Close();
            //// Exit CMD.EXE
            //string stEchoFmt = "# {0} run successfully. Exiting";
            //sIn.WriteLine(String.Format(stEchoFmt, strFilePath));
            //sIn.WriteLine("EXIT");
            //// Close the process
            //proc.Close();
            //// Read the sOut to a string.
            //string results = sOut.ReadToEnd().Trim();
            //// Close the io Streams;
            //sIn.Close();
            //sOut.Close();

            return results;
        }
    }
}