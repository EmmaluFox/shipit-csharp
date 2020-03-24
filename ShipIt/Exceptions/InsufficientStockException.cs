namespace ShipIt.Exceptions
{
    public class InsufficientStockException : ClientVisibleException
    {
        public InsufficientStockException(string message) : base(message, ErrorCode.InsufficientStock)
        {
        }

        public override ErrorCode ErrorCode { get; set; }
    }
}