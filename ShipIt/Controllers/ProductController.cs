using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Parsers;
using ShipIt.Repositories;
using ShipIt.Validators;

namespace ShipIt.Controllers
{
    public class ProductController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public ProductResponse Get(string gtin)
        {
            if (gtin == null) throw new MalformedRequestException("Unable to parse gtin from request parameters");

            Log.Info("Looking up product by gtin: " + gtin);

            var product = new Product(_productRepository.GetProductByGtin(gtin));

            Log.Info("Found product: " + product);

            return new ProductResponse(product);
        }

        public Response Post([FromBody] ProductsRequestModel requestModel)
        {
            var parsedProducts = new List<Product>();

            foreach (var requestProduct in requestModel.Products)
            {
                var parsedProduct = requestProduct.Parse();
                new ProductValidator().Validate(parsedProduct);
                parsedProducts.Add(parsedProduct);
            }

            Log.Info("Adding products: " + parsedProducts);

            var dataProducts = parsedProducts.Select(p => new ProductDataModel(p));
            _productRepository.AddProducts(dataProducts);

            Log.Debug("Products added successfully");

            return new Response {Success = true};
        }

        [HttpPatch]
        public Response Discontinue(string gtin)
        {
            if (gtin == null) throw new MalformedRequestException("Unable to parse gtin from request parameters");

            Log.Info("Discontinuing up product by gtin: " + gtin);

            _productRepository.DiscontinueProductByGtin(gtin);

            Log.Info("Discontinued product: " + gtin);

            return new Response {Success = true};
        }
    }
}