namespace LINQPad
{
    using System;

    internal class Disposable : IDisposable
    {
        private Action _onDispose;

        private Disposable(Action onDispose)
        {
            this._onDispose = onDispose;
        }

        public static IDisposable Create(Action onDispose)
        {
            return new Disposable(onDispose);
        }

        public void Dispose()
        {
            if (this._onDispose != null)
            {
                this._onDispose();
            }
        }
    }
}

