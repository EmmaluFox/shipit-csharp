using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;
using ShipItTest.Builders;

namespace ShipItTest
{
    [TestClass]
    public class EmployeeControllerTests : AbstractBaseTest
    {
        private const string Name = "Gissell Sadeem";
        private const int WarehouseId = 1;
        private readonly EmployeeController _employeeController = new EmployeeController(new EmployeeRepository());
        private readonly EmployeeRepository _employeeRepository = new EmployeeRepository();

        [TestMethod]
        public void TestRoundtripEmployeeRepository()
        {
            OnSetUp();
            var employee = new EmployeeBuilder().CreateEmployee();
            _employeeRepository.AddEmployees(new List<Employee> {employee});
            Assert.AreEqual(_employeeRepository.GetEmployeeByName(employee.Name).Name, employee.Name);
            Assert.AreEqual(_employeeRepository.GetEmployeeByName(employee.Name).Ext, employee.Ext);
            Assert.AreEqual(_employeeRepository.GetEmployeeByName(employee.Name).WarehouseId, employee.WarehouseId);
        }

        [TestMethod]
        public void TestGetEmployeeByName()
        {
            OnSetUp();
            var employeeBuilder = new EmployeeBuilder().SetName(Name);
            _employeeRepository.AddEmployees(new List<Employee> {employeeBuilder.CreateEmployee()});
            var result = _employeeController.Get(Name);

            var correctEmployee = employeeBuilder.CreateEmployee();
            Assert.IsTrue(EmployeesAreEqual(correctEmployee, result.Employees.First()));
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void TestGetEmployeesByWarehouseId()
        {
            OnSetUp();
            var employeeBuilderA = new EmployeeBuilder().SetWarehouseId(WarehouseId).SetName("A");
            var employeeBuilderB = new EmployeeBuilder().SetWarehouseId(WarehouseId).SetName("B");
            _employeeRepository.AddEmployees(new List<Employee>
                {employeeBuilderA.CreateEmployee(), employeeBuilderB.CreateEmployee()});
            var result = _employeeController.Get(WarehouseId).Employees.ToList();

            var correctEmployeeA = employeeBuilderA.CreateEmployee();
            var correctEmployeeB = employeeBuilderB.CreateEmployee();

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(EmployeesAreEqual(correctEmployeeA, result.First()));
            Assert.IsTrue(EmployeesAreEqual(correctEmployeeB, result.Last()));
        }

        [TestMethod]
        public void TestGetNonExistentEmployee()
        {
            OnSetUp();
            try
            {
                _employeeController.Get(Name);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(Name));
            }
        }

        [TestMethod]
        public void TestGetEmployeeInNonexistentWarehouse()
        {
            OnSetUp();
            try
            {
                var employees = _employeeController.Get(WarehouseId).Employees.ToList();
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(WarehouseId.ToString()));
            }
        }

        [TestMethod]
        public void TestAddEmployees()
        {
            OnSetUp();
            var employeeBuilder = new EmployeeBuilder().SetName(Name);
            var addEmployeesRequest = employeeBuilder.CreateAddEmployeesRequest();

            var response = _employeeController.Post(addEmployeesRequest);
            var databaseEmployee = _employeeRepository.GetEmployeeByName(Name);
            var correctDatabaseEmploye = employeeBuilder.CreateEmployee();

            Assert.IsTrue(response.Success);
            Assert.IsTrue(EmployeesAreEqual(new Employee(databaseEmployee), correctDatabaseEmploye));
        }

        [TestMethod]
        public void TestDeleteEmployees()
        {
            OnSetUp();
            var employeeBuilder = new EmployeeBuilder().SetName(Name);
            _employeeRepository.AddEmployees(new List<Employee> {employeeBuilder.CreateEmployee()});

            var removeEmployeeRequest = new RemoveEmployeeRequest {Name = Name};
            _employeeController.Delete(removeEmployeeRequest);

            try
            {
                _employeeController.Get(Name);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(Name));
            }
        }

        [TestMethod]
        public void TestDeleteNonexistentEmployee()
        {
            OnSetUp();
            var removeEmployeeRequest = new RemoveEmployeeRequest {Name = Name};

            try
            {
                _employeeController.Delete(removeEmployeeRequest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(Name));
            }
        }

        [TestMethod]
        public void TestAddDuplicateEmployee()
        {
            OnSetUp();
            var employeeBuilder = new EmployeeBuilder().SetName(Name);
            _employeeRepository.AddEmployees(new List<Employee> {employeeBuilder.CreateEmployee()});
            var addEmployeesRequest = employeeBuilder.CreateAddEmployeesRequest();

            try
            {
                _employeeController.Post(addEmployeesRequest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
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