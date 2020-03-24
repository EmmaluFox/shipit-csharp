using System.Collections.Generic;
using System.Linq;
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
    public class InboundOrderControllerTests : AbstractBaseTest
    {
        private const string Gtin = "0000";

        private static readonly Employee OpsManager = new EmployeeBuilder().CreateEmployee();
        private static readonly Company Company = new CompanyBuilder().CreateCompany();
        private static readonly int WarehouseId = OpsManager.WarehouseId;
        private static readonly string Gcp = Company.Gcp;
        private readonly CompanyRepository _companyRepository = new CompanyRepository();
        private readonly EmployeeRepository _employeeRepository = new EmployeeRepository();

        private readonly InboundOrderController _inboundOrderController = new InboundOrderController(
            new EmployeeRepository(),
            new CompanyRepository(),
            new ProductRepository(),
            new StockRepository()
        );

        private Product _product;
        private int _productId;
        private readonly ProductRepository _productRepository = new ProductRepository();
        private readonly StockRepository _stockRepository = new StockRepository();

        public new void OnSetUp()
        {
            base.OnSetUp();
            _employeeRepository.AddEmployees(new List<Employee> {OpsManager});
            _companyRepository.AddCompanies(new List<Company> {Company});
            var productDataModel = new ProductBuilder().SetGtin(Gtin).CreateProductDatabaseModel();
            _productRepository.AddProducts(new List<ProductDataModel> {productDataModel});
            _product = new Product(_productRepository.GetProductByGtin(Gtin));
            _productId = _product.Id;
        }

        [TestMethod]
        public void TestCreateOrderNoProductsHeld()
        {
            OnSetUp();

            var inboundOrder = _inboundOrderController.Get(WarehouseId);

            Assert.AreEqual(inboundOrder.WarehouseId, WarehouseId);
            Assert.IsTrue(EmployeesAreEqual(inboundOrder.OperationsManager, OpsManager));
            Assert.AreEqual(inboundOrder.OrderSegments.Count(), 0);
        }

        [TestMethod]
        public void TestCreateOrderProductHoldingNoStock()
        {
            OnSetUp();
            _stockRepository.AddStock(WarehouseId, new List<StockAlteration> {new StockAlteration(_productId, 0)});

            var inboundOrder = _inboundOrderController.Get(WarehouseId);

            Assert.AreEqual(inboundOrder.OrderSegments.Count(), 1);
            var orderSegment = inboundOrder.OrderSegments.First();
            Assert.AreEqual(orderSegment.Company.Gcp, Gcp);
        }

        [TestMethod]
        public void TestCreateOrderProductHoldingSufficientStock()
        {
            OnSetUp();
            _stockRepository.AddStock(WarehouseId,
                new List<StockAlteration> {new StockAlteration(_productId, _product.LowerThreshold)});

            var inboundOrder = _inboundOrderController.Get(WarehouseId);

            Assert.AreEqual(inboundOrder.OrderSegments.Count(), 0);
        }

        [TestMethod]
        public void TestCreateOrderDiscontinuedProduct()
        {
            OnSetUp();
            _stockRepository.AddStock(WarehouseId,
                new List<StockAlteration> {new StockAlteration(_productId, _product.LowerThreshold - 1)});
            _productRepository.DiscontinueProductByGtin(Gtin);

            var inboundOrder = _inboundOrderController.Get(WarehouseId);

            Assert.AreEqual(inboundOrder.OrderSegments.Count(), 0);
        }

        [TestMethod]
        public void TestProcessManifest()
        {
            OnSetUp();
            var quantity = 12;
            var inboundManifest = new InboundManifestRequestModel
            {
                WarehouseId = WarehouseId,
                Gcp = Gcp,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = quantity
                    }
                }
            };

            _inboundOrderController.Post(inboundManifest);

            var stock =
                _stockRepository.GetStockByWarehouseAndProductIds(WarehouseId, new List<int> {_productId})[_productId];
            Assert.AreEqual(stock.Held, quantity);
        }

        [TestMethod]
        public void TestProcessManifestRejectsDodgyGcp()
        {
            OnSetUp();
            var quantity = 12;
            var dodgyGcp = Gcp + "XYZ";
            var inboundManifest = new InboundManifestRequestModel
            {
                WarehouseId = WarehouseId,
                Gcp = dodgyGcp,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = quantity
                    }
                }
            };

            try
            {
                _inboundOrderController.Post(inboundManifest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(dodgyGcp));
            }
        }

        [TestMethod]
        public void TestProcessManifestRejectsUnknownProduct()
        {
            OnSetUp();
            var quantity = 12;
            var unknownGtin = Gtin + "XYZ";
            var inboundManifest = new InboundManifestRequestModel
            {
                WarehouseId = WarehouseId,
                Gcp = Gcp,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = quantity
                    },
                    new OrderLine
                    {
                        Gtin = unknownGtin,
                        Quantity = quantity
                    }
                }
            };

            try
            {
                _inboundOrderController.Post(inboundManifest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(unknownGtin));
            }
        }

        [TestMethod]
        public void TestProcessManifestRejectsDuplicateGtins()
        {
            OnSetUp();
            var quantity = 12;
            var inboundManifest = new InboundManifestRequestModel
            {
                WarehouseId = WarehouseId,
                Gcp = Gcp,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = quantity
                    },
                    new OrderLine
                    {
                        Gtin = Gtin,
                        Quantity = quantity
                    }
                }
            };

            try
            {
                _inboundOrderController.Post(inboundManifest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(Gtin));
            }
        }

        private bool EmployeesAreEqual(Employee a, Employee b)
        {
            return a.WarehouseId == b.WarehouseId
                   && a.Name == b.Name
                   && a.Role == b.Role
                   && a.Ext == b.Ext;
        }
    }
}