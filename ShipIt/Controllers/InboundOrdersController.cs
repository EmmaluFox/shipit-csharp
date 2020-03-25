using System;
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
    public class InboundOrderController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICompanyRepository companyRepository;

        private readonly IEmployeeRepository employeeRepository;
        private readonly IProductRepository productRepository;
        private readonly IStockRepository stockRepository;

        public InboundOrderController(IEmployeeRepository employeeRepository, ICompanyRepository companyRepository,
            IProductRepository productRepository, IStockRepository stockRepository)
        {
            this.employeeRepository = employeeRepository;
            this.stockRepository = stockRepository;
            this.companyRepository = companyRepository;
            this.productRepository = productRepository;
        }

        // GET api/status/{warehouseId}
        public InboundOrderResponse Get(int warehouseId)
        {
            log.Info("orderIn for warehouseId: " + warehouseId);

            var operationsManager = new Employee(employeeRepository.GetOperationsManager(warehouseId));

            log.Debug(string.Format("Found operations manager: {0}", operationsManager));

            var allStock = stockRepository.GetStockByWarehouseId(warehouseId);

            var orderlinesByCompany = new Dictionary<Company, List<InboundOrderLine>>();
            foreach (var stock in allStock)
            {
                var product = new Product(productRepository.GetProductById(stock.ProductId));
                if (stock.held < product.LowerThreshold && !product.Discontinued)
                {
                    var company = new Company(companyRepository.GetCompany(product.Gcp));

                    var orderQuantity = Math.Max(product.LowerThreshold * 3 - stock.held, product.MinimumOrderQuantity);

                    if (!orderlinesByCompany.ContainsKey(company))
                        orderlinesByCompany.Add(company, new List<InboundOrderLine>());

                    orderlinesByCompany[company].Add(
                        new InboundOrderLine
                        {
                            gtin = product.Gtin,
                            name = product.Name,
                            quantity = orderQuantity
                        });
                }
            }

            log.Debug(string.Format("Constructed order lines: {0}", orderlinesByCompany));

            var orderSegments = orderlinesByCompany.Select(ol => new OrderSegment
            {
                OrderLines = ol.Value,
                Company = ol.Key
            });

            log.Info("Constructed inbound order");

            return new InboundOrderResponse
            {
                OperationsManager = operationsManager,
                WarehouseId = warehouseId,
                OrderSegments = orderSegments
            };
        }

        public void Post([FromBody] InboundManifestRequestModel requestModel)
        {
            log.Info("Processing manifest: " + requestModel);

            var gtins = new List<string>();

            foreach (var orderLine in requestModel.OrderLines)
            {
                if (gtins.Contains(orderLine.gtin))
                    throw new ValidationException(string.Format("Manifest contains duplicate product gtin: {0}",
                        orderLine.gtin));
                gtins.Add(orderLine.gtin);
            }

            var productDataModels = productRepository.GetProductsByGtin(gtins);
            var products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));

            log.Debug(string.Format("Retrieved products to verify manifest: {0}", products));

            var lineItems = new List<StockAlteration>();
            var errors = new List<string>();

            foreach (var orderLine in requestModel.OrderLines)
            {
                if (!products.ContainsKey(orderLine.gtin))
                {
                    errors.Add(string.Format("Unknown product gtin: {0}", orderLine.gtin));
                    continue;
                }

                var product = products[orderLine.gtin];
                if (!product.Gcp.Equals(requestModel.Gcp))
                    errors.Add(string.Format("Manifest GCP ({0}) doesn't match Product GCP ({1})",
                        requestModel.Gcp, product.Gcp));
                else
                    lineItems.Add(new StockAlteration(product.Id, orderLine.quantity));
            }

            if (errors.Count() > 0)
            {
                log.Debug(string.Format("Found errors with inbound manifest: {0}", errors));
                throw new ValidationException(string.Format("Found inconsistencies in the inbound manifest: {0}",
                    string.Join("; ", errors)));
            }

            log.Debug(string.Format("Increasing stock levels with manifest: {0}", requestModel));
            stockRepository.AddStock(requestModel.WarehouseId, lineItems);
            log.Info("Stock levels increased");
        }
    }
}