namespace LINQPad
{
    using System;

    internal class ObservableHelper
    {
        public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> onSubscribe)
        {
            return new AnonObservable<T>(onSubscribe);
        }

        public static IDisposable Subscribe<TSource>(IObservable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
        {
            return source.Subscribe(new AnonObserver<TSource>(onNext, onError, onCompleted));
        }

        private class AnonObservable<T> : IObservable<T>
        {
            private Func<IObserver<T>, IDisposable> _onSubscribe;

            public AnonObservable(Func<IObserver<T>, IDisposable> onSubscribe)
            {
                this._onSubscribe = onSubscribe;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return this._onSubscribe(observer);
            }
        }

        private class AnonObserver<T> : IObserver<T>
        {
            private Action _completed;
            private Action<Exception> _error;
            private bool _finished;
            private Action<T> _next;

            public AnonObserver(Action<T> next, Action<Exception> error, Action completed)
            {
                this._next = next;
                this._error = error;
                this._completed = completed;
            }

            public void OnCompleted()
            {
                if (!this._finished)
                {
                    this._finished = true;
                    this._completed();
                }
            }

            public void OnError(Exception exception)
            {
                if (exception == null)
                {
                    throw new ArgumentNullException("exception");
                }
                if (!this._finished)
                {
                    this._finished = true;
                    this._error(exception);
                }
            }

            public void OnNext(T value)
            {
                if (!this._finished)
                {
                    this._next(value);
                }
            }
        }
    }
}

