namespace LINQPad
{
    using System;
    using System.Threading;
    using System.Windows.Threading;

    internal class DispatcherThrottler
    {
        private Action _action;
        private Action _finalAction;
        private bool _isAborted;
        private object _locker = new object();
        private DispatcherTimer _timer;

        public void Run(Dispatcher dispatcher, Action todo, bool isFinal, CancelToken cancelToken, Thread subscribingThread)
        {
            if ((cancelToken != null) && cancelToken.IsCancellationRequested)
            {
                if (this._timer != null)
                {
                    this._timer.Stop();
                }
                if (subscribingThread == Thread.CurrentThread)
                {
                    cancelToken.ThrowIfCancelRequested();
                }
                else if (!this._isAborted)
                {
                    this._isAborted = true;
                    try
                    {
                        subscribingThread.Abort();
                    }
                    catch
                    {
                    }
                }
            }
            if (this._timer == null)
            {
                this._timer = new DispatcherTimer(TimeSpan.FromMilliseconds(50.0), DispatcherPriority.Normal, new EventHandler(this.Tick), dispatcher);
                this._timer.Start();
            }
            lock (this._locker)
            {
                if (isFinal)
                {
                    this._finalAction = todo;
                }
                else
                {
                    this._action = todo;
                }
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            Action action;
            Action action2;
            lock (this._locker)
            {
                action = this._action;
                action2 = this._finalAction;
                this._action = null;
                this._finalAction = null;
            }
            if (action2 != null)
            {
                this._timer.Stop();
            }
            if (action != null)
            {
                action();
            }
            if (action2 != null)
            {
                action2();
            }
        }
    }
}

