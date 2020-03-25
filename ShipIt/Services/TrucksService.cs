﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
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


        public static IDbConnection CreateSqlConnection()
        {
            return new NpgsqlConnection(ConnectionHelper.GetConnectionString());
        }

        public OutboundOrderResponse GetTrucksForOrder(List<StockAlteration> lineItems)
        {
            OutboundOrderResponse outboundOrder = new OutboundOrderResponse();
            List<Truck> truckList = new List<Truck>();
            outboundOrder.Trucks = truckList;
            Truck truck = new Truck();
            truckList.Add(truck);
            List<Case> cases = GetFilledCases(lineItems);
            truck.Cases = cases;
           
            return outboundOrder;
        }

        public List<Case> GetFilledCases(List<StockAlteration> lineItems)
        {
            List<Case> cases = new List<Case>();
            foreach (var item in lineItems)
            {
                var newProduct = new Product(_productRepository.GetProductById(item.ProductId));
                foreach (var caseItem in cases)
                {
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
            }
            return cases;
        }
        
    }
}