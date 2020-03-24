namespace ShipIt.Exceptions
{
    public class InvalidStateException : ClientVisibleException
    {
        public InvalidStateException(string message) : base(message, ErrorCode.InvalidState)
        {
        }

        public override ErrorCode ErrorCode { get; set; }
    }
}