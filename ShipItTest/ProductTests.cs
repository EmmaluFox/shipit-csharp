using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using ShipItTest.Builders;

namespace ShipItTest
{
    [TestClass]
    public class ProductControllerTests : AbstractBaseTest
    {
        private const int WarehouseId = 1;

        // private static readonly Employee EMPLOYEE = new EmployeeBuilder().setWarehouseId(WAREHOUSE_ID).CreateEmployee();
        private const string Gtin = "0000346374230";
        private readonly ProductController _productController = new ProductController(new ProductRepository());
        private readonly ProductRepository _productRepository = new ProductRepository();

        [TestMethod]
        public void TestRoundtripProductRepository()
        {
            OnSetUp();
            var product = new ProductBuilder().CreateProductDatabaseModel();
            _productRepository.AddProducts(new List<ProductDataModel> {product});
            Assert.AreEqual(_productRepository.GetProductByGtin(product.Gtin).Name, product.Name);
            Assert.AreEqual(_productRepository.GetProductByGtin(product.Gtin).Gtin, product.Gtin);
        }

        [TestMethod]
        public void TestGetProduct()
        {
            OnSetUp();
            var productBuilder = new ProductBuilder().SetGtin(Gtin);
            _productRepository.AddProducts(new List<ProductDataModel> {productBuilder.CreateProductDatabaseModel()});
            var result = _productController.Get(Gtin);

            var correctProduct = productBuilder.CreateProduct();
            Assert.IsTrue(ProductsAreEqual(correctProduct, result.Product));
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void TestGetNonexistentProduct()
        {
            OnSetUp();
            try
            {
                _productController.Get(Gtin);
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(Gtin));
            }
        }

        [TestMethod]
        public void TestAddProducts()
        {
            OnSetUp();
            var productBuilder = new ProductBuilder().SetGtin(Gtin);
            var productRequest = productBuilder.CreateProductRequest();

            var response = _productController.Post(productRequest);
            var databaseProduct = _productRepository.GetProductByGtin(Gtin);
            var correctDatabaseProduct = productBuilder.CreateProductDatabaseModel();

            Assert.IsTrue(response.Success);
            ProductsAreEqual(new Product(databaseProduct), new Product(correctDatabaseProduct));
        }

        [TestMethod]
        public void TestAddPreexistingProduct()
        {
            OnSetUp();
            var productBuilder = new ProductBuilder().SetGtin(Gtin);
            _productRepository.AddProducts(new List<ProductDataModel> {productBuilder.CreateProductDatabaseModel()});
            var productRequest = productBuilder.CreateProductRequest();

            try
            {
                _productController.Post(productRequest);
                Assert.Fail();
            }
            catch (MalformedRequestException e)
            {
                Assert.IsTrue(e.Message.Contains(Gtin));
            }
        }

        [TestMethod]
        public void TestAddDuplicateProduct()
        {
            OnSetUp();
            var productBuilder = new ProductBuilder().SetGtin(Gtin);
            var productRequest = productBuilder.CreateDuplicateProductRequest();

            try
            {
                _productController.Post(productRequest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (MalformedRequestException e)
            {
                Assert.IsTrue(e.Message.Contains(Gtin));
            }
        }

        [TestMethod]
        public void TestDiscontinueProduct()
        {
            OnSetUp();
            var productBuilder = new ProductBuilder().SetGtin(Gtin);
            _productRepository.AddProducts(new List<ProductDataModel> {productBuilder.CreateProductDatabaseModel()});

            _productController.Discontinue(Gtin);
            var result = _productController.Get(Gtin);

            Assert.IsTrue(result.Product.Discontinued);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void TestDiscontinueNonexistentProduct()
        {
            OnSetUp();
            try
            {
                _productController.Discontinue(Gtin);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(Gtin));
            }
        }

        [TestMethod]
        public void TestDiscontinueNonexistantProduct()
        {
            OnSetUp();
            var nonExistantGtin = "12345678";
            try
            {
                _productController.Discontinue(nonExistantGtin);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(nonExistantGtin));
            }
        }

        private bool ProductsAreEqual(Product a, Product b)
        {
            const double floatingPointTolerance = 10 * float.Epsilon;
            return a.Discontinued == b.Discontinued
                   && a.Gcp == b.Gcp
                   && a.Gtin == b.Gtin
                   && a.LowerThreshold == b.LowerThreshold
                   && a.MinimumOrderQuantity == b.MinimumOrderQuantity
                   && a.Name == b.Name
                   && Math.Abs(a.Weight - b.Weight) < floatingPointTolerance;
        }
    }
}