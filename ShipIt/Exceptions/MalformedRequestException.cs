namespace ShipIt.Exceptions
{
    public class MalformedRequestException : ClientVisibleException
    {
        public MalformedRequestException(string message) : base(message, ErrorCode.MalformedRequest)
        {
        }

        public override ErrorCode ErrorCode { get; set; }
    }
}