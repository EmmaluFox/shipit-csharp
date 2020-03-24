using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;

namespace ShipIt.Repositories
{
    public interface IEmployeeRepository
    {
        int GetCount();
        int GetWarehouseCount();
        EmployeeDataModel GetEmployeeByName(string name);
        IEnumerable<EmployeeDataModel> GetEmployeesByWarehouseId(int warehouseId);
        EmployeeDataModel GetOperationsManager(int warehouseId);
        void AddEmployees(IEnumerable<Employee> employees);
        void RemoveEmployee(string name);
    }

    public class EmployeeRepository : RepositoryBase, IEmployeeRepository
    {
        public int GetCount()
        {
            using (var connection = CreateSqlConnection())
            {
                var command = connection.CreateCommand();
                var employeeCountSql = "SELECT COUNT(*) FROM em";
                command.CommandText = employeeCountSql;
                connection.Open();
                var reader = command.ExecuteReader();

                try
                {
                    reader.Read();
                    return (int) reader.GetInt64(0);
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        public int GetWarehouseCount()
        {
            using (var connection = CreateSqlConnection())
            {
                var command = connection.CreateCommand();
                var employeeCountSql = "SELECT COUNT(DISTINCT w_id) FROM em";
                command.CommandText = employeeCountSql;
                connection.Open();
                var reader = command.ExecuteReader();

                try
                {
                    reader.Read();
                    return (int) reader.GetInt64(0);
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        public EmployeeDataModel GetEmployeeByName(string name)
        {
            var sql = "SELECT name, w_id, role, ext FROM em WHERE name = @name";
            var parameter = new NpgsqlParameter("@name", name);
            var noProductWithIdErrorMessage = string.Format("No employees found with name: {0}", name);
            return RunSingleGetQuery(sql, reader => new EmployeeDataModel(reader), noProductWithIdErrorMessage,
                parameter);
        }

        public IEnumerable<EmployeeDataModel> GetEmployeesByWarehouseId(int warehouseId)
        {
            var sql = "SELECT name, w_id, role, ext FROM em WHERE w_id = @w_id";
            var parameter = new NpgsqlParameter("@w_id", warehouseId);
            var noProductWithIdErrorMessage =
                string.Format("No employees found with Warehouse Id: {0}", warehouseId);
            return RunGetQuery(sql, reader => new EmployeeDataModel(reader), noProductWithIdErrorMessage, parameter);
        }

        public EmployeeDataModel GetOperationsManager(int warehouseId)
        {
            var sql = "SELECT name, w_id, role, ext FROM em WHERE w_id = @w_id AND role = @role";
            var parameters = new[]
            {
                new NpgsqlParameter("@w_id", warehouseId),
                new NpgsqlParameter("@role", DataBaseRoles.OperationsManager)
            };

            var noProductWithIdErrorMessage =
                string.Format("No employees found with Warehouse Id: {0}", warehouseId);
            return RunSingleGetQuery(sql, reader => new EmployeeDataModel(reader), noProductWithIdErrorMessage,
                parameters);
        }

        public void AddEmployees(IEnumerable<Employee> employees)
        {
            var sql = "INSERT INTO em (name, w_id, role, ext) VALUES(@name, @w_id, @role, @ext)";

            var parametersList = new List<NpgsqlParameter[]>();
            foreach (var employee in employees)
            {
                var employeeDataModel = new EmployeeDataModel(employee);
                parametersList.Add(employeeDataModel.GetNpgsqlParameters().ToArray());
            }

            RunTransaction(sql, parametersList);
        }
        public void RemoveEmployee(string name)
        {
            var sql = "DELETE FROM em WHERE name = @name";
            var parameter = new NpgsqlParameter("@name", name);
            var rowsDeleted = RunSingleQueryAndReturnRecordsAffected(sql, parameter);
            if (rowsDeleted == 0)
                throw new NoSuchEntityException("Incorrect result size: expected 1, actual 0");
            if (rowsDeleted > 1)
                throw new InvalidStateException("Unexpectedly deleted " + rowsDeleted +
                                                " rows, but expected a single update");
        }

        public static IDbConnection CreateSqlConnection()
        {
            return new NpgsqlConnection(ConnectionHelper.GetConnectionString());
        }
    }
}