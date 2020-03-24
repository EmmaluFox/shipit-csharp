using System.Collections.Generic;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;

namespace ShipItTest.Builders
{
    public class EmployeeBuilder
    {
        private string _ext = "73996";
        private string _name = "Gissell Sadeem";
        private EmployeeRole _role = EmployeeRole.OperationsManager;
        private int _warehouseId = 1;

        public EmployeeBuilder SetName(string name)
        {
            _name = name;
            return this;
        }

        public EmployeeBuilder SetWarehouseId(int warehouseId)
        {
            _warehouseId = warehouseId;
            return this;
        }

        public EmployeeBuilder SetRole(EmployeeRole role)
        {
            _role = role;
            return this;
        }

        public EmployeeBuilder SetExt(string ext)
        {
            _ext = ext;
            return this;
        }

        public EmployeeDataModel CreateEmployeeDataModel()
        {
            return new EmployeeDataModel
            {
                Name = _name,
                WarehouseId = _warehouseId,
                Role = _role.ToString(),
                Ext = _ext
            };
        }

        public Employee CreateEmployee()
        {
            return new Employee
            {
                Name = _name,
                WarehouseId = _warehouseId,
                Role = _role,
                Ext = _ext
            };
        }

        public AddEmployeesRequest CreateAddEmployeesRequest()
        {
            return new AddEmployeesRequest
            {
                Employees = new List<Employee>
                {
                    new Employee
                    {
                        Name = _name,
                        WarehouseId = _warehouseId,
                        Role = _role,
                        Ext = _ext
                    }
                }
            };
        }
    }
}