using System.Collections.Generic;

namespace ShipIt.Models.ApiModels
{
    public class ProductsRequestModel
    {
        public IEnumerable<ProductRequestModel> Products { get; set; }
    }
}