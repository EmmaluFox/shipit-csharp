using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using ShipItTest.Builders;

namespace ShipItTest
{
    [TestClass]
    public class StockControllerTests : AbstractBaseTest
    {
        private const string Gtin = "0000";
        private readonly CompanyRepository _companyRepository = new CompanyRepository();
        private readonly ProductRepository _productRepository = new ProductRepository();
        private readonly StockRepository _stockRepository = new StockRepository();

        public new void OnSetUp()
        {
            base.OnSetUp();
            _companyRepository.AddCompanies(new List<Company> {new CompanyBuilder().CreateCompany()});
            _productRepository.AddProducts(new List<ProductDataModel>
                {new ProductBuilder().SetGtin(Gtin).CreateProductDatabaseModel()});
        }

        [TestMethod]
        public void TestAddNewStock()
        {
            OnSetUp();
            var productId = _productRepository.GetProductByGtin(Gtin).Id;

            _stockRepository.AddStock(1, new List<StockAlteration> {new StockAlteration(productId, 1)});

            var databaseStock = _stockRepository.GetStockByWarehouseAndProductIds(1, new List<int> {productId});
            Assert.AreEqual(databaseStock[productId].Held, 1);
        }

        [TestMethod]
        public void TestUpdateExistingStock()
        {
            OnSetUp();
            var productId = _productRepository.GetProductByGtin(Gtin).Id;
            _stockRepository.AddStock(1, new List<StockAlteration> {new StockAlteration(productId, 2)});

            _stockRepository.AddStock(1, new List<StockAlteration> {new StockAlteration(productId, 5)});

            var databaseStock = _stockRepository.GetStockByWarehouseAndProductIds(1, new List<int> {productId});
            Assert.AreEqual(databaseStock[productId].Held, 7);
        }
    }
}