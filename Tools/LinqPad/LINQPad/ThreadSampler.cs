namespace LINQPad
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class ThreadSampler : IDisposable
    {
        private string _filePath;
        private ManualResetEvent _stop = new ManualResetEvent(false);
        private Stopwatch _stopWatch = Stopwatch.StartNew();
        private Thread _toSample;
        public volatile bool Enabled;

        public ThreadSampler(string outputFilePath)
        {
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
            this._filePath = outputFilePath;
            this._toSample = Thread.CurrentThread;
            new Thread(new ThreadStart(this.Work)) { IsBackground = true, Name = "ThreadSampler" }.Start();
        }

        public void Dispose()
        {
            this.Enabled = false;
            this._stop.Set();
        }

        public void ResetTiming()
        {
            this._stopWatch.Restart();
        }

        private void Work()
        {
            while (!this._stop.WaitOne(1))
            {
                if (this.Enabled)
                {
                    StackTrace trace = null;
                    bool flag = false;
                    try
                    {
                        flag = (this._toSample.ThreadState & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin;
                        this._toSample.Suspend();
                        trace = new StackTrace(this._toSample, true);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        try
                        {
                            this._toSample.Resume();
                        }
                        catch
                        {
                        }
                    }
                    if (trace != null)
                    {
                        IEnumerable<string> values = from frame in trace.GetFrames()
                            let method = frame.GetMethod()
                            let type = (method == null) ? null : ((IEnumerable<string>) method.DeclaringType)
                            select (((type == null) || type.Assembly.GlobalAssemblyCache) ? "*" : "") + ((type == null) ? "?" : type.Name) + "." + ((method == null) ? "?" : method.Name);
                        object[] objArray = new object[] { flag.ToString()[0].ToString(), this._stopWatch.ElapsedMilliseconds, " ", string.Join("|", values), "\r\n" };
                        File.AppendAllText(this._filePath, string.Concat(objArray));
                    }
                }
            }
        }
    }
}

