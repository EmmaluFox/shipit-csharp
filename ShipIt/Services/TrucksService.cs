using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using Npgsql;
using ShipIt.Exceptions;
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
            if (lineItems.Count == 0)
            {
                throw new MalformedRequestException("Order must contain at least one line");
            }
            var outboundOrder = new OutboundOrderResponse();
            var truckList = new List<Truck>();
            outboundOrder.Trucks = truckList;
            for (var i = 0; i < lineItems.Count; i++)
            {
                var truckX = GetTruckFromLoadingBay(GetFilledCases(lineItems));
                truckX.Id = i;
                truckList.Add(truckX);
            }
            return outboundOrder;
        }

        private List<Case> GetFilledCases(List<StockAlteration> lineItems)
        {
            var cases = new List<Case>();
            var initialCase = new Case();
            cases.Add(initialCase);
            foreach (var item in lineItems)
            {
                var newProduct = new Product(_productRepository.GetProductById(item.ProductId));
                if (initialCase.Gtin == null)
                {
                    initialCase.Gtin = newProduct.Gtin;
                }

                foreach (var caseItem in cases)
                    if (newProduct.Gtin == caseItem.Gtin && (caseItem.TotalWeight + newProduct.Weight < 2000))
                    {
                        caseItem.Quantity += item.Quantity;
                        caseItem.Name = newProduct.Name;
                        caseItem.WeightPerItem = newProduct.Weight;
                        lineItems.Remove(item);
                    }
                    else
                    {
                        var newCase = new Case
                        {
                            Gtin = newProduct.Gtin,
                            Quantity = item.Quantity,
                            Name = newProduct.Name,
                            WeightPerItem = newProduct.Weight
                        };
                        lineItems.Remove(item);
                        cases.Add(newCase);
                    }
            }

            return cases;
        }
        
        private Truck GetTruckFromLoadingBay(List<Case> unloadedCases) {
            var truck = new Truck();
            var caseLoad = new List<Case>();
            var caseLoadTotalWeight = caseLoad.Sum(load => load.TotalWeight);
            caseLoad.AddRange(unloadedCases.Where(uniqueCase => uniqueCase.TotalWeight + caseLoadTotalWeight < 2000));
            truck.Cases = caseLoad;
            return truck;
        }


    
}





}