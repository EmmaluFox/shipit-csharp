using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    public class OutboundOrderController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IProductRepository _productRepository;

        private readonly IStockRepository _stockRepository;

        public OutboundOrderController(IStockRepository stockRepository, IProductRepository productRepository)
        {
            this._stockRepository = stockRepository;
            this._productRepository = productRepository;
        }

        public void Post([FromBody] OutboundOrderRequestModel request)
        {
            Log.Info(string.Format("Processing outbound order: {0}", request));

            var gtins = new List<string>();
            foreach (var orderLine in request.OrderLines)
            {
                if (gtins.Contains(orderLine.Gtin))
                    throw new ValidationException(
                        string.Format("Outbound order request contains duplicate product gtin: {0}", orderLine.Gtin));
                gtins.Add(orderLine.Gtin);
            }

            var productDataModels = _productRepository.GetProductsByGtin(gtins);
            var products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));

            var lineItems = new List<StockAlteration>();
            var productIds = new List<int>();
            var errors = new List<string>();

            foreach (var orderLine in request.OrderLines)
                if (!products.ContainsKey(orderLine.Gtin))
                {
                    errors.Add(string.Format("Unknown product gtin: {0}", orderLine.Gtin));
                }
                else
                {
                    var product = products[orderLine.Gtin];
                    lineItems.Add(new StockAlteration(product.Id, orderLine.Quantity));
                    productIds.Add(product.Id);
                }

            if (errors.Count > 0) throw new NoSuchEntityException(string.Join("; ", errors));

            var stock = _stockRepository.GetStockByWarehouseAndProductIds(request.WarehouseId, productIds);

            var orderLines = request.OrderLines.ToList();
            errors = new List<string>();

            for (var i = 0; i < lineItems.Count; i++)
            {
                var lineItem = lineItems[i];
                var orderLine = orderLines[i];

                if (!stock.ContainsKey(lineItem.ProductId))
                {
                    errors.Add(string.Format("Product: {0}, no stock held", orderLine.Gtin));
                    continue;
                }

                var item = stock[lineItem.ProductId];
                if (lineItem.Quantity > item.Held)
                    errors.Add(
                        string.Format("Product: {0}, stock held: {1}, stock to remove: {2}", orderLine.Gtin, item.Held,
                            lineItem.Quantity));
            }

            if (errors.Count > 0) throw new InsufficientStockException(string.Join("; ", errors));

            _stockRepository.RemoveStock(request.WarehouseId, lineItems);
        }
    }
}