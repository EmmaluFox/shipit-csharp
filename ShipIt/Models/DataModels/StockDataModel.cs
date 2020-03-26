using System.Data;

namespace ShipIt.Models.DataModels
{
    public class StockDataModel : DataModel
    {
        public StockDataModel(IDataReader dataReader) : base(dataReader)
        {
        }

        [DatabaseColumnName("p_id")] public int ProductId { get; set; }

        [DatabaseColumnName("w_id")] public int WarehouseId { get; set; }

        [DatabaseColumnName("hld")] public int Held { get; set; }
    }
}