namespace ShipIt.Exceptions
{
    public class TruckOverloadedException : ClientVisibleException
    {
        public TruckOverloadedException(string message) : base(message, ErrorCode.TruckOverloaded)
        {
        }

        public override ErrorCode ErrorCode { get; set; }
    }
}