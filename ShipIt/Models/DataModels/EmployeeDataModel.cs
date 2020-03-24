using System;
using System.Data;
using ShipIt.Models.ApiModels;

namespace ShipIt.Models.DataModels
{
    public class EmployeeDataModel : DataModel
    {
        public EmployeeDataModel(IDataReader dataReader) : base(dataReader)
        {
        }

        public EmployeeDataModel()
        {
        }

        public EmployeeDataModel(Employee employee)
        {
            Name = employee.Name;
            WarehouseId = employee.WarehouseId;
            Role = MapApiRoleToDatabaseRole(employee.Role);
            Ext = employee.Ext;
        }

        [DatabaseColumnName("name")] public string Name { get; set; }

        [DatabaseColumnName("w_id")] public int WarehouseId { get; set; }

        [DatabaseColumnName("role")] public string Role { get; set; }

        [DatabaseColumnName("ext")] public string Ext { get; set; }

        private string MapApiRoleToDatabaseRole(EmployeeRole employeeRole)
        {
            if (employeeRole == EmployeeRole.Cleaner) return DataBaseRoles.Cleaner;
            if (employeeRole == EmployeeRole.Manager) return DataBaseRoles.Manager;
            if (employeeRole == EmployeeRole.OperationsManager) return DataBaseRoles.OperationsManager;
            if (employeeRole == EmployeeRole.Picker) return DataBaseRoles.Picker;
            throw new ArgumentOutOfRangeException("employeeRole");
        }
    }
}