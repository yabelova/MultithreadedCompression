using System;
using System.Runtime.Serialization;

namespace MultithreadedCompression
{
    [Serializable]
    public class WrongCallException : Exception
    {
        public WrongCallException()
        {
        }

        public WrongCallException(string message) : base(message)
        {
        }

        public WrongCallException(string message, Exception inner) : base(message, inner)
        {
        }

        protected WrongCallException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

