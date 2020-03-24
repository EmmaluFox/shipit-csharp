using System.Configuration;
using System.Data;
using Npgsql;
using ShipIt.Repositories;

namespace ShipItTest
{
    public abstract class AbstractBaseTest
    {
        protected EmployeeRepository EmployeeRepository { get; set; }
        protected ProductRepository ProductRepository { get; set; }
        protected CompanyRepository CompanyRepository { get; set; }
        protected StockRepository StockRepository { get; set; }

        public static IDbConnection CreateSqlConnection()
        {
            return new NpgsqlConnection(ConfigurationManager.ConnectionStrings["MyPostgres"].ConnectionString);
        }

        public void OnSetUp()
        {
            // Start from a clean slate
            var sql =
                "TRUNCATE TABLE em;"
                + "TRUNCATE TABLE stock;"
                + "TRUNCATE TABLE gcp;"
                + "TRUNCATE TABLE gtin CASCADE;";

            using (var connection = CreateSqlConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText = sql;
                connection.Open();
                var reader = command.ExecuteReader();
                try
                {
                    reader.Read();
                }
                finally
                {
                    reader.Close();
                }
            }
        }
    }
}