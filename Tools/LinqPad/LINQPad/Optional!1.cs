namespace LINQPad
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Optional<T> : IOptional
    {
        private readonly T _value;
        private readonly bool _hasValue;
        public static readonly Optional<T> NoValue;
        public T Value
        {
            get
            {
                if (!this.HasValue)
                {
                    throw new InvalidOperationException("Optional<T> has no value");
                }
                return this._value;
            }
        }
        public bool HasValue
        {
            get
            {
                return this._hasValue;
            }
        }
        public Optional(T value)
        {
            this._value = value;
            this._hasValue = true;
        }

        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        public static explicit operator T(Optional<T> value)
        {
            return value.Value;
        }

        public override bool Equals(object other)
        {
            if (!(other is Optional<T>))
            {
                return false;
            }
            Optional<T> optional = (Optional<T>) other;
            if (!this._hasValue)
            {
                return !optional._hasValue;
            }
            if (!optional._hasValue)
            {
                return false;
            }
            return object.Equals(this._value, optional._value);
        }

        public override int GetHashCode()
        {
            if (!(this._hasValue && (this._value != null)))
            {
                return 0;
            }
            return this._value.GetHashCode();
        }

        public override string ToString()
        {
            if (!this._hasValue)
            {
                return "No value";
            }
            if (this._value == null)
            {
                return "null";
            }
            return this._value.ToString();
        }

        object IOptional.Value
        {
            get
            {
                return this._value;
            }
        }
        static Optional()
        {
            Optional<T>.NoValue = new Optional<T>();
        }
    }
}

