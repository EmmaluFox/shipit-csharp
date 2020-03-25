using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Services
{
    public class OutboundOrderResponse
    {
        public List<Truck> Trucks { get; set; }
        public double NumberOfTrucks => Trucks.Count();
    }

    public class Truck
    {
        public List<Case> Cases { get; set; }
        public double TotalWeight => Cases.Sum(batch => batch.TotalWeight);
        public int Id { get; set; }
    }

    public class Case
    {
        public string Gtin { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double WeightPerItem { get; set; }
        public double TotalWeight => WeightPerItem * Quantity;
    }
}