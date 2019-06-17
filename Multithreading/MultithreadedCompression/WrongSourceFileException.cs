using System;
using System.Runtime.Serialization;

namespace MultithreadedCompression
{
    [Serializable]
    public class WrongSourceFileException : Exception
    {
        public WrongSourceFileException()
        {
        }

        public WrongSourceFileException(string message) : base(message)
        {
        }

        public WrongSourceFileException(string message, Exception inner) : base(message, inner)
        {
        }

        protected WrongSourceFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

