using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;
using ShipIt.Services;

namespace ShipIt.Controllers
{
    public class OutboundOrderController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IProductRepository _productRepository;
        private ITrucksService _trucksService;
        private readonly IStockRepository _stockRepository;

        public OutboundOrderController(IStockRepository stockRepository, IProductRepository productRepository)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
        }

        public void Post([FromBody] OutboundOrderRequestModel request)
        {
            Log.Info($"Processing outbound order: {request}");
            var products = GetProductsFromRequest(request);
            var lineItems = new List<StockAlteration>();
            var productIds = new List<int>();
            var errors = new List<string>();
            float weightCalculation = 0;
            
            foreach (var orderLine in request.OrderLines)
                if (!products.ContainsKey(orderLine.Gtin))
                {
                    errors.Add($"Unknown product gtin: {orderLine.Gtin}");
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
                    errors.Add($"Product: {orderLine.Gtin}, no stock held");
                    continue;
                }

                var item = stock[lineItem.ProductId];
                if (lineItem.Quantity > item.Held)
                    errors.Add(
                        $"Product: {orderLine.Gtin}, stock held: {item.Held}, stock to remove: {lineItem.Quantity}");
            }

            if (errors.Count > 0) throw new InsufficientStockException(string.Join("; ", errors));
            if (weightCalculation > 1000) throw new TruckOverloadedException(string.Join("; ", errors));

            _stockRepository.RemoveStock(request.WarehouseId, lineItems);
            _trucksService.GetTrucksForOrder(lineItems);
        }

        private Dictionary<string, Product> GetProductsFromRequest(OutboundOrderRequestModel request)
        {
            var gtins = new List<string>();
            foreach (var orderLine in request.OrderLines)
            {
                if (gtins.Contains(orderLine.Gtin))
                    throw new ValidationException(
                        $"Outbound order request contains duplicate product gtin: {orderLine.Gtin}");
                gtins.Add(orderLine.Gtin);
            }

            var productDataModels = _productRepository.GetProductsByGtin(gtins);
            var products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));
            return products;
        }
    }
}