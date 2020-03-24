using System;
using System.Text;
using ShipIt.Models.DataModels;

namespace ShipIt.Models.ApiModels
{
    public class Employee
    {
        public Employee(EmployeeDataModel dataModel)
        {
            Name = dataModel.Name;
            WarehouseId = dataModel.WarehouseId;
            Role = MapDatabaseRoleToApiRole(dataModel.Role);
            Ext = dataModel.Ext;
        }

        //Empty constructor needed for Xml serialization
        public Employee()
        {
        }

        public string Name { get; set; }
        public int WarehouseId { get; set; }
        public EmployeeRole Role { get; set; }
        public string Ext { get; set; }

        private EmployeeRole MapDatabaseRoleToApiRole(string databaseRole)
        {
            if (databaseRole == DataBaseRoles.Cleaner) return EmployeeRole.Cleaner;
            if (databaseRole == DataBaseRoles.Manager) return EmployeeRole.Manager;
            if (databaseRole == DataBaseRoles.OperationsManager) return EmployeeRole.OperationsManager;
            if (databaseRole == DataBaseRoles.Picker) return EmployeeRole.Picker;
            throw new ArgumentOutOfRangeException("databaseRole");
        }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendFormat("name: {0}, ", Name)
                .AppendFormat("warehouseId: {0}, ", WarehouseId)
                .AppendFormat("role: {0}, ", Role)
                .AppendFormat("ext: {0}", Ext)
                .ToString();
        }
    }
}