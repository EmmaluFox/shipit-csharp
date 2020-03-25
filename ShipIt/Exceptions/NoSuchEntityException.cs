namespace ShipIt.Exceptions
{
    public class NoSuchEntityException : ClientVisibleException
    {
        public NoSuchEntityException(string message) : base(message, ErrorCode.NO_SUCH_ENTITY_EXCEPTION)
        {
        }

        public override ErrorCode ErrorCode { get; set; }
    }
}