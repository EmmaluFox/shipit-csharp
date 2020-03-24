namespace ShipIt.Exceptions
{
    public class NoSuchEntityException : ClientVisibleException
    {
        public NoSuchEntityException(string message) : base(message, ErrorCode.NoSuchEntityException)
        {
        }

        public override ErrorCode ErrorCode { get; set; }
    }
}