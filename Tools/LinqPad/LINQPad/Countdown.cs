namespace LINQPad
{
    using System;
    using System.Threading;

    internal class Countdown
    {
        private Action _continuations = null;
        private object _locker = new object();
        private int _value;

        public void Add(int amount)
        {
            Action continuations = null;
            lock (this._locker)
            {
                this._value += amount;
                if (this._value <= 0)
                {
                    Monitor.PulseAll(this._locker);
                    continuations = this._continuations;
                    this._continuations = null;
                }
            }
            if (continuations != null)
            {
                ThreadPool.QueueUserWorkItem(_ => continuations());
            }
        }

        public void ContinueWith(Action a)
        {
            WaitCallback callBack = null;
            lock (this._locker)
            {
                if (this._value <= 0)
                {
                    if (callBack == null)
                    {
                        callBack = _ => a();
                    }
                    ThreadPool.QueueUserWorkItem(callBack);
                }
                else
                {
                    this._continuations = (Action) Delegate.Combine(this._continuations, a);
                }
            }
        }

        public void Decrement()
        {
            this.Add(-1);
        }

        public void Increment()
        {
            this.Add(1);
        }

        public void Reset()
        {
            lock (this._locker)
            {
                this._value = 0;
                Monitor.PulseAll(this._locker);
            }
        }

        public void Wait()
        {
            Thread.MemoryBarrier();
            if (this._value > 0)
            {
                lock (this._locker)
                {
                    while (this._value > 0)
                    {
                        Monitor.Wait(this._locker);
                    }
                }
            }
        }

        public bool Wait(int timeout)
        {
            Thread.MemoryBarrier();
            if (this._value > 0)
            {
                DateTime time2 = DateTime.Now.AddMilliseconds((double) timeout);
                lock (this._locker)
                {
                    while (this._value > 0)
                    {
                        TimeSpan span = (TimeSpan) (time2 - DateTime.Now);
                        if (span <= TimeSpan.Zero)
                        {
                            return false;
                        }
                        Monitor.Wait(this._locker, span);
                    }
                }
            }
            return true;
        }

        public int Value
        {
            get
            {
                lock (this._locker)
                {
                    return this._value;
                }
            }
        }
    }
}

