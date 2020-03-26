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
            foreach (var item in lineItems)
            {
                var newProduct = new Product(_productRepository.GetProductById(item.ProductId));
                var orderWeight = item.Quantity * newProduct.Weight;
                int maxQuantityPerCase = (int) (2000 / newProduct.Weight);
                var caseWeight = maxQuantityPerCase * newProduct.Weight;
                var fullCasesRequired = orderWeight - (orderWeight % caseWeight) / caseWeight;
                int partFilledCaseItemQuantity = (int) (item.Quantity - (maxQuantityPerCase * fullCasesRequired));

                foreach (var caseItem in cases)
                {
                    for (var i = 0; i < fullCasesRequired; i++)
                    {
                        var newFullCase = new Case
                        {
                            Gtin = newProduct.Gtin,
                            Quantity = maxQuantityPerCase,
                            Name = newProduct.Name,
                            WeightPerItem = newProduct.Weight
                        };
                        cases.Add(newFullCase);
                    }

                    if (newProduct.Gtin == caseItem.Gtin && partFilledCaseItemQuantity > 0)
                    {
                        if (partFilledCaseItemQuantity + caseItem.Quantity < maxQuantityPerCase)
                        {
                            caseItem.Quantity += partFilledCaseItemQuantity;
                        }
                        else
                        {
                            var remainingItemSpace = maxQuantityPerCase - caseItem.Quantity;
                            var leftOver = partFilledCaseItemQuantity - remainingItemSpace;
                            caseItem.Quantity = maxQuantityPerCase;
                            var newPartFilledCase = new Case()
                            {
                                Gtin = newProduct.Gtin,
                                Quantity = leftOver,
                                Name = newProduct.Name,
                                WeightPerItem = newProduct.Weight
                            };
                            cases.Add(newPartFilledCase);
                        }
                    }
                }
            }
            return cases;
        }
        static Truck GetTruckFromLoadingBay(List<Case> unloadedCases)
        {
            var truck = new Truck();
            var caseLoad = new List<Case>();
            var caseLoadTotalWeight = caseLoad.Sum(load => load.TotalWeight);
            caseLoad.AddRange(unloadedCases.Where(uniqueCase =>
                uniqueCase.TotalWeight + caseLoadTotalWeight < 2001));
            truck.Cases = caseLoad;
            return truck;
        }
    }
}
