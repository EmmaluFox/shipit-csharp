﻿using System;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICompanyRepository _companyRepository;

        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;

        public InboundOrderController(IEmployeeRepository employeeRepository, ICompanyRepository companyRepository,
            IProductRepository productRepository, IStockRepository stockRepository)
        {
            this._employeeRepository = employeeRepository;
            this._stockRepository = stockRepository;
            this._companyRepository = companyRepository;
            this._productRepository = productRepository;
        }

        // GET api/status/{warehouseId}
        public InboundOrderResponse Get(int warehouseId)
        {
            Log.Info("orderIn for warehouseId: " + warehouseId);

            var operationsManager = new Employee(_employeeRepository.GetOperationsManager(warehouseId));

            Log.Debug($"Found operations manager: {operationsManager}");

            var allStock = _stockRepository.GetStockByWarehouseId(warehouseId);

            var orderlinesByCompany = new Dictionary<Company, List<InboundOrderLine>>();
            foreach (var stock in allStock)
            {
                var product = new Product(_productRepository.GetProductById(stock.ProductId));
                if (stock.Held < product.LowerThreshold && !product.Discontinued)
                {
                    var company = new Company(_companyRepository.GetCompany(product.Gcp));

                    var orderQuantity = Math.Max(product.LowerThreshold * 3 - stock.Held, product.MinimumOrderQuantity);

                    if (!orderlinesByCompany.ContainsKey(company))
                        orderlinesByCompany.Add(company, new List<InboundOrderLine>());

                    orderlinesByCompany[company].Add(
                        new InboundOrderLine
                        {
                            Gtin = product.Gtin,
                            Name = product.Name,
                            Quantity = orderQuantity
                        });
                }
            }

            Log.Debug($"Constructed order lines: {orderlinesByCompany}");

            var orderSegments = orderlinesByCompany.Select(ol => new OrderSegment
            {
                OrderLines = ol.Value,
                Company = ol.Key
            });

            Log.Info("Constructed inbound order");

            return new InboundOrderResponse
            {
                OperationsManager = operationsManager,
                WarehouseId = warehouseId,
                OrderSegments = orderSegments
            };
        }

        public void Post([FromBody] InboundManifestRequestModel requestModel)
        {
            Log.Info("Processing manifest: " + requestModel);

            var gtins = new List<string>();

            foreach (var orderLine in requestModel.OrderLines)
            {
                if (gtins.Contains(orderLine.Gtin))
                    throw new ValidationException($"Manifest contains duplicate product gtin: {orderLine.Gtin}");
                gtins.Add(orderLine.Gtin);
            }

            var productDataModels = _productRepository.GetProductsByGtin(gtins);
            var products = productDataModels.ToDictionary(p => p.Gtin, p => new Product(p));

            Log.Debug($"Retrieved products to verify manifest: {products}");

            var lineItems = new List<StockAlteration>();
            var errors = new List<string>();

            foreach (var orderLine in requestModel.OrderLines)
            {
                if (!products.ContainsKey(orderLine.Gtin))
                {
                    errors.Add($"Unknown product gtin: {orderLine.Gtin}");
                    continue;
                }

                var product = products[orderLine.Gtin];
                if (!product.Gcp.Equals(requestModel.Gcp))
                    errors.Add($"Manifest GCP ({requestModel.Gcp}) doesn't match Product GCP ({product.Gcp})");
                else
                    lineItems.Add(new StockAlteration(product.Id, orderLine.Quantity));
            }

            if (errors.Count() > 0)
            {
                Log.Debug($"Found errors with inbound manifest: {errors}");
                throw new ValidationException(
                    $"Found inconsistencies in the inbound manifest: {string.Join("; ", errors)}");
            }

            Log.Debug($"Increasing stock levels with manifest: {requestModel}");
            _stockRepository.AddStock(requestModel.WarehouseId, lineItems);
            Log.Info("Stock levels increased");
        }
    }
}