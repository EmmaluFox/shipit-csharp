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
    public class OutboundOrderControllerTests : AbstractBaseTest
    {
        private const string Gtin = "0000";

        private static readonly Employee Employee = new EmployeeBuilder().CreateEmployee();
        private static readonly Company Company = new CompanyBuilder().CreateCompany();
        private static readonly int WarehouseId = Employee.WarehouseId;
        private readonly CompanyRepository _companyRepository = new CompanyRepository();
        private readonly EmployeeRepository _employeeRepository = new EmployeeRepository();

        private readonly OutboundOrderController _outboundOrderController = new OutboundOrderController(
            new StockRepository(),
            new ProductRepository()
        );

        private Product _product;
        private int _productId;
        private readonly ProductRepository _productRepository = new ProductRepository();
        private readonly StockRepository _stockRepository = new StockRepository();

        public new void OnSetUp()
        {
            base.OnSetUp();
            _employeeRepository.AddEmployees(new List<Employee> {Employee});
            _companyRepository.AddCompanies(new List<Company> {Company});
            var productDataModel = new ProductBuilder().SetGtin(Gtin).CreateProductDatabaseModel();
            _productRepository.AddProducts(new List<ProductDataModel> {productDataModel});
            _product = new Product(_productRepository.GetProductByGtin(Gtin));
            _productId = _product.Id;
        }

        [TestMethod]
        public void TestOutboundOrder()
        {
            OnSetUp();
            _stockRepository.AddStock(WarehouseId, new List<StockAlteration> {new StockAlteration(_productId, 10)});
            var outboundOrder = new OutboundOrderRequestModel
            {
                WarehouseId = WarehouseId,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = 3
                    }
                }
            };

            _outboundOrderController.Post(outboundOrder);

            var stock =
                _stockRepository.GetStockByWarehouseAndProductIds(WarehouseId, new List<int> {_productId})[_productId];
            Assert.AreEqual(stock.Held, 7);
        }

        [TestMethod]
        public void TestOutboundOrderInsufficientStock()
        {
            OnSetUp();
            _stockRepository.AddStock(WarehouseId, new List<StockAlteration> {new StockAlteration(_productId, 10)});
            var outboundOrder = new OutboundOrderRequestModel
            {
                WarehouseId = WarehouseId,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = 11
                    }
                }
            };

            try
            {
                _outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (InsufficientStockException e)
            {
                Assert.IsTrue(e.Message.Contains(Gtin));
            }
        }

        [TestMethod]
        public void TestOutboundOrderStockNotHeld()
        {
            OnSetUp();
            var noStockGtin = Gtin + "XYZ";
            _productRepository.AddProducts(new List<ProductDataModel>
                {new ProductBuilder().SetGtin(noStockGtin).CreateProductDatabaseModel()});
            _stockRepository.AddStock(WarehouseId, new List<StockAlteration> {new StockAlteration(_productId, 10)});

            var outboundOrder = new OutboundOrderRequestModel
            {
                WarehouseId = WarehouseId,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = 8
                    },
                    new OrderLine
                    {
                        Gtin = noStockGtin,
                        Quantity = 1000
                    }
                }
            };

            try
            {
                _outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (InsufficientStockException e)
            {
                Assert.IsTrue(e.Message.Contains(noStockGtin));
                Assert.IsTrue(e.Message.Contains("no stock held"));
            }
        }

        [TestMethod]
        public void TestOutboundOrderBadGtin()
        {
            OnSetUp();
            var badGtin = Gtin + "XYZ";

            var outboundOrder = new OutboundOrderRequestModel
            {
                WarehouseId = WarehouseId,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = 1
                    },
                    new OrderLine
                    {
                        Gtin = badGtin,
                        Quantity = 1
                    }
                }
            };

            try
            {
                _outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(badGtin));
            }
        }

        [TestMethod]
        public void TestOutboundOrderDuplicateGtins()
        {
            OnSetUp();
            _stockRepository.AddStock(WarehouseId, new List<StockAlteration> {new StockAlteration(_productId, 10)});
            var outboundOrder = new OutboundOrderRequestModel
            {
                WarehouseId = WarehouseId,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = 1
                    },
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = 1
                    }
                }
            };

            try
            {
                _outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(Gtin));
            }
        }
    }
}