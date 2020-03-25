using System.Collections.Generic;
using FakeItEasy;
using NUnit.Framework;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using ShipIt.Services;

namespace ShipItTest
{
    public class TrucksServiceTests
    {
        private readonly ProductDataModel _testProduct = new ProductDataModel
        {
            Id = 17,
            Weight = 100,
            Name = "Test ID",
            Gtin = "test-id"
        };

        private IProductRepository _productRepository;
        private TrucksService _trucksService;

        [SetUp]
        public void SetUp()
        {
            _productRepository = A.Fake<IProductRepository>();
            _trucksService = new TrucksService(_productRepository);
            A.CallTo(() => _productRepository.GetProductById(17)).Returns(_testProduct);
        }

        [Test]
        public void SmallOrderPlacedUsingSingleTruck()
        {
            var lineItems = new List<StockAlteration>
            {
                new StockAlteration(17, 3)
            };

            var trucks = _trucksService.GetTrucksForOrder(lineItems);
            Assert.AreEqual(1, trucks.NumberOfTrucks);
            Assert.AreEqual(300, trucks.Trucks[0].TotalWeight);
        }
    }
}