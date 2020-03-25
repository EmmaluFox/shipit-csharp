using System;

namespace ShipIt.Exceptions
{
    public abstract class ClientVisibleException : Exception
    {
        protected ClientVisibleException(string message, ErrorCode errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public abstract ErrorCode ErrorCode { get; set; }
    }
}