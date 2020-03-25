namespace ShipIt.Exceptions
{
    public class MalformedRequestException : ClientVisibleException
    {
        public MalformedRequestException(string message) : base(message, ErrorCode.MALFORMED_REQUEST)
        {
        }

        public override ErrorCode ErrorCode { get; set; }
    }
}