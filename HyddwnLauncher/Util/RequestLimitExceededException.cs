using System;
using System.Runtime.Serialization;

namespace HyddwnLauncher.Util
{
    [Serializable]
    public class RequestLimitExceededException : Exception
    {
        public RequestLimitExceededException()
            : base("You have exceeded the maximum number of request allowed.")
        {
        }

        /// <exception cref="SerializationException">
        ///     The class name is null or <see cref="P:System.Exception.HResult" /> is zero
        ///     (0).
        /// </exception>
        protected RequestLimitExceededException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}