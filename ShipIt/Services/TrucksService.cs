using System.Collections.Generic;
using System.Data;
using Npgsql;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;

namespace ShipIt.Services
{
    public interface ITrucksService
    {
        OutboundOrderResponse GetTrucksForOrder(List<StockAlteration> lineItems);
    }

    public class TrucksService : ITrucksService
    {
        private readonly IProductRepository _productRepository;

        public TrucksService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public OutboundOrderResponse GetTrucksForOrder(List<StockAlteration> lineItems)
        {
            var outboundOrder = new OutboundOrderResponse();
            var truckList = new List<Truck>();
            outboundOrder.Trucks = truckList;
            var truck = new Truck();
            truckList.Add(truck);
            var cases = GetFilledCases(lineItems);
            truck.Cases = cases;

            return outboundOrder;
        }


        public static IDbConnection CreateSqlConnection()
        {
            return new NpgsqlConnection(ConnectionHelper.GetConnectionString());
        }

        public List<Case> GetFilledCases(List<StockAlteration> lineItems)
        {
            var cases = new List<Case>();
            foreach (var item in lineItems)
            {
                var newProduct = new Product(_productRepository.GetProductById(item.ProductId));
                foreach (var caseItem in cases)
                    if (newProduct.Gtin == caseItem.Gtin)
                    {
                        caseItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        var newCase = new Case {Gtin = newProduct.Gtin, Quantity = item.Quantity};
                        cases.Add(newCase);
                    }
            }

            return cases;
        }
    }
}