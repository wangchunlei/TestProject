namespace LINQPad
{
    using System;

    internal class CancelToken
    {
        private bool _cancelRequest;
        private object _locker = new object();

        public void Cancel()
        {
            lock (this._locker)
            {
                this._cancelRequest = true;
            }
        }

        public void ThrowIfCancelRequested()
        {
            if (this.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        public bool IsCancellationRequested
        {
            get
            {
                lock (this._locker)
                {
                    return this._cancelRequest;
                }
            }
        }
    }
}

