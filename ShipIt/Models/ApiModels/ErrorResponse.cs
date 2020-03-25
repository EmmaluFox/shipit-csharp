using ShipIt.Exceptions;

namespace ShipIt.Models.ApiModels
{
    public class ErrorResponse : Response
    {
        public ErrorResponse()
        {
            Success = false;
        }

        public ErrorCode Code { get; set; }
        public string Error { get; set; }
    }
}