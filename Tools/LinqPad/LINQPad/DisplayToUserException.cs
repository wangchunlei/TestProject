namespace LINQPad
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class DisplayToUserException : Exception, ISerializable
    {
        public DisplayToUserException(string msg) : base(msg)
        {
        }

        public DisplayToUserException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DisplayToUserException(string msg, Exception inner) : base(msg, inner)
        {
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}

