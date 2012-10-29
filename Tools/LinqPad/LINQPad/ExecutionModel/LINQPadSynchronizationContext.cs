namespace LINQPad.ExecutionModel
{
    using LINQPad;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Timers;
    using System.Windows.Forms;

    internal class LINQPadSynchronizationContext : SynchronizationContext
    {
        private bool _detectDeadlocks;
        private SynchronizationContext _host;
        private bool _showBounces;
        private Thread _thread;

        internal LINQPadSynchronizationContext(SynchronizationContext inner)
        {
            this._host = new WindowsFormsSynchronizationContext();
            this._host = inner;
        }

        public LINQPadSynchronizationContext(bool detectDeadlocks, bool showBounces) : this(Thread.CurrentThread, detectDeadlocks, showBounces)
        {
        }

        private LINQPadSynchronizationContext(Thread thread, bool detectDeadlocks, bool showBounces)
        {
            this._host = new WindowsFormsSynchronizationContext();
            this._thread = thread;
            this._detectDeadlocks = detectDeadlocks;
            this._showBounces = showBounces;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new LINQPadSynchronizationContext(this._thread, this._detectDeadlocks, this._showBounces);
        }

        public override void OperationCompleted()
        {
            if (this._showBounces)
            {
                Util.Highlight("Synchronization Context OperationCompleted (thread " + Thread.CurrentThread.ManagedThreadId + ")").Dump<object>();
            }
            base.OperationCompleted();
        }

        public override void OperationStarted()
        {
            if (this._showBounces)
            {
                Util.Highlight("Synchronization Context OperationStarted (thread " + Thread.CurrentThread.ManagedThreadId + ")").Dump<object>();
            }
            base.OperationStarted();
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            bool _done;
            if (this._showBounces)
            {
                this.Report("Post");
            }
            if (!this._detectDeadlocks)
            {
                this._host.Post(d, state);
            }
            else
            {
                _done = false;
                try
                {
                    int blockSample = 0;
                    if ((this._thread.ThreadState & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin)
                    {
                        blockSample++;
                    }
                    int i = 0;
                    StackTrace trace = null;
                    Timer tmr = new Timer(100.0);
                    tmr.Elapsed += delegate (object sender, ElapsedEventArgs e) {
                        try
                        {
                            if (_done)
                            {
                                tmr.Dispose();
                            }
                            else
                            {
                                Thread.MemoryBarrier();
                                if (((this._thread.ThreadState & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin) && ((Interlocked.Increment(ref blockSample) > 10) && (trace == null)))
                                {
                                    try
                                    {
                                        this._thread.Suspend();
                                        if ((this._thread.ThreadState & ThreadState.WaitSleepJoin) > ThreadState.Running)
                                        {
                                            trace = new StackTrace(this._thread, true);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    try
                                    {
                                        this._thread.Resume();
                                    }
                                    catch
                                    {
                                    }
                                }
                                if (Interlocked.Increment(ref i) > 30)
                                {
                                    tmr.Dispose();
                                    if (blockSample >= 15)
                                    {
                                        Server currentServer = Server.CurrentServer;
                                        if ((currentServer != null) && (trace != null))
                                        {
                                            string message = "LINQPad has detected a possible deadlock in your code";
                                            currentServer.ReportSpecialMessage(trace, null, message, true, false);
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            tmr.Dispose();
                        }
                    };
                    tmr.Start();
                }
                catch
                {
                }
                this._host.Post(delegate (object _) {
                    d(_);
                    Thread.MemoryBarrier();
                    _done = true;
                    Thread.MemoryBarrier();
                }, state);
            }
        }

        private void Report(string kind)
        {
            if (Server.CurrentServer != null)
            {
                Util.Highlight(string.Concat(new object[] { "Synchronization Context ", kind, " (thread ", Thread.CurrentThread.ManagedThreadId, " to thread ", this._thread.ManagedThreadId, ")" })).Dump<object>();
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (this._showBounces)
            {
                this.Report("Send");
            }
            this._host.Send(d, state);
        }
    }
}

