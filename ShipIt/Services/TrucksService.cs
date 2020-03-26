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
            var outboundOrder = new OutboundOrderResponse();
            var cases = GetFilledCases(lineItems);
            var truckList = GetTrucksFromLoadingBay(cases);
            outboundOrder.Trucks = truckList;
            return outboundOrder;
        }

        private List<Case> GetFilledCases(List<StockAlteration> lineItems)
        {
            var cases = new List<Case>();
            foreach (var item in lineItems)
            {
                var newProduct = new Product(_productRepository.GetProductById(item.ProductId));
                var orderWeight = item.Quantity * newProduct.Weight;
                var caseWeight = newProduct.MaxItemsPerCase * newProduct.Weight;
                var fullCasesRequired = orderWeight - (orderWeight % caseWeight) / caseWeight;
                int partFilledCaseItemQuantity =
                    (int) (item.Quantity - (newProduct.MaxItemsPerCase * fullCasesRequired));

                foreach (var caseItem in cases)
                {
                    for (var i = 0; i < fullCasesRequired; i++)
                    {
                        var newFullCase = new Case
                        {
                            Gtin = newProduct.Gtin,
                            Quantity = newProduct.MaxItemsPerCase,
                            Name = newProduct.Name,
                            WeightPerItem = newProduct.Weight
                        };
                        cases.Add(newFullCase);
                    }

                    if (newProduct.Gtin == caseItem.Gtin && partFilledCaseItemQuantity > 0)
                    {
                        if (partFilledCaseItemQuantity + caseItem.Quantity < newProduct.MaxItemsPerCase)
                        {
                            caseItem.Quantity += partFilledCaseItemQuantity;
                        }
                        else
                        {
                            var remainingItemSpace = newProduct.MaxItemsPerCase - caseItem.Quantity;
                            var leftOver = partFilledCaseItemQuantity - remainingItemSpace;
                            caseItem.Quantity = newProduct.MaxItemsPerCase;
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

        static List<Truck> GetTrucksFromLoadingBay(List<Case> unloadedCases)
        {
            var truckList = new List<Truck>();
            List<Case> fullCases = new List<Case>();
            List<Case> partFilledCases = new List<Case>();
            foreach (var unloadedCase in unloadedCases)
            {
                Product product = new Product() {Gtin = unloadedCase.Gtin};
                if (unloadedCase.Quantity == product.MaxItemsPerCase)
                {
                    fullCases.Add(unloadedCase);
                }
                else
                {
                    partFilledCases.Add(unloadedCase);
                }

                int i = 1;
                foreach (var fullCase in fullCases)
                {
                    var truck = new Truck() {Id = i};
                    truck.Cases.Add(fullCase);
                    truckList.Add(truck);
                }
            }

            // truck.Cases.AddRange(unloadedCases.Where(uniqueCase =>
                //     uniqueCase.TotalWeight + truck.TotalWeight < 2001));
                return truckList;
            }
        }
    }

