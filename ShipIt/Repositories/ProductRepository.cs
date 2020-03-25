using System.Collections.Generic;
using System.Linq;
using Npgsql;
using ShipIt.Exceptions;
using ShipIt.Models.DataModels;

namespace ShipIt.Repositories
{
    public interface IProductRepository
    {
        int GetCount();
        ProductDataModel GetProductByGtin(string gtin);
        IEnumerable<ProductDataModel> GetProductsByGtin(List<string> gtins);
        ProductDataModel GetProductById(int id);
        void AddProducts(IEnumerable<ProductDataModel> products);
        void DiscontinueProductByGtin(string gtin);
    }

    public class ProductRepository : RepositoryBase, IProductRepository
    {
        public int GetCount()
        {
            var EmployeeCountSQL = "SELECT COUNT(*) FROM gcp";
            return (int) QueryForLong(EmployeeCountSQL);
        }

        public ProductDataModel GetProductByGtin(string gtin)
        {
            var sql = "SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE gtin_cd = @gtin_cd";
            var parameter = new NpgsqlParameter("@gtin_cd", gtin);
            return RunSingleGetQuery(sql, reader => new ProductDataModel(reader),
                string.Format("No products found with gtin of value {0}", gtin), parameter);
        }

        public IEnumerable<ProductDataModel> GetProductsByGtin(List<string> gtins)
        {
            var sql = string.Format(
                "SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE gtin_cd IN ('{0}')",
                string.Join("','", gtins));
            return RunGetQuery(sql, reader => new ProductDataModel(reader), "No products found with given gtin ids",
                null);
        }

        public ProductDataModel GetProductById(int id)
        {
            var sql = "SELECT p_id, gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt FROM gtin WHERE p_id = @p_id";
            var parameter = new NpgsqlParameter("@p_id", id);
            var noProductWithIdErrorMessage = string.Format("No products found with id of value {0}", id.ToString());
            return RunSingleGetQuery(sql, reader => new ProductDataModel(reader), noProductWithIdErrorMessage,
                parameter);
        }

        public void DiscontinueProductByGtin(string gtin)
        {
            var sql = "UPDATE gtin SET ds = 1 WHERE gtin_cd = @gtin_cd";
            var parameter = new NpgsqlParameter("@gtin_cd", gtin);
            var noProductWithGtinErrorMessage =
                string.Format("No products found with gtin of value {0}", gtin);

            RunSingleQuery(sql, noProductWithGtinErrorMessage, parameter);
        }

        public void AddProducts(IEnumerable<ProductDataModel> products)
        {
            var sql =
                "INSERT INTO gtin (gtin_cd, gcp_cd, gtin_nm, m_g, l_th, ds, min_qt) VALUES (@gtin_cd, @gcp_cd, @gtin_nm, @m_g, @l_th, @ds, @min_qt)";

            var parametersList = new List<NpgsqlParameter[]>();
            var gtins = new List<string>();

            foreach (var product in products)
            {
                if (gtins.Contains(product.Gtin))
                    throw new MalformedRequestException(string.Format("Cannot add products with duplicate gtins: {0}",
                        product.Gtin));
                gtins.Add(product.Gtin);
                parametersList.Add(product.GetNpgsqlParameters().ToArray());
            }

            var conflicts = TryGetProductsByGtin(gtins);
            if (conflicts.Any())
                throw new MalformedRequestException(string.Format("Cannot add products with existing gtins: {0}",
                    string.Join(", ", conflicts.Select(c => c.Gtin))));

            RunTransaction(sql, parametersList);
        }

        private IEnumerable<ProductDataModel> TryGetProductsByGtin(List<string> gtins)
        {
            try
            {
                var products = GetProductsByGtin(gtins).ToList();
                return products;
            }
            catch (NoSuchEntityException)
            {
                return new List<ProductDataModel>();
            }
        }
    }
}