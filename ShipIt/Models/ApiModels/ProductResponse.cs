namespace ShipIt.Models.ApiModels
{
    public class ProductResponse : Response
    {
        public ProductResponse(Product product)
        {
            Product = product;
            Success = true;
        }

        public ProductResponse()
        {
        }

        public Product Product { get; set; }
    }
}