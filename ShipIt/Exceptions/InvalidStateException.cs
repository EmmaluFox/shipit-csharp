namespace ShipIt.Exceptions
{
    public class InvalidStateException : ClientVisibleException
    {
        public InvalidStateException(string message) : base(message, ErrorCode.INVALID_STATE)
        {
        }

        public override ErrorCode ErrorCode { get; set; }
    }
}