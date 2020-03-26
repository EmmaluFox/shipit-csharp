using System.Collections.Generic;
using ShipIt.Controllers;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Parsers;

namespace ShipItTest.Builders
{
    public class ProductBuilder
    {
        private int _discontinued;
        private string _gcp = "0000346";
        private string _gtin = "0099346374235";
        private int _id = 1;
        private int _lowerThreshold = 322;
        private int _minimumOrderQuantity = 108;
        private string _name = "2 Count 1 T30 Torx Bit Tips TX";
        private float _weight = 300.0f;

        public ProductBuilder SetId(int id)
        {
            _id = id;
            return this;
        }

        public ProductBuilder SetGtin(string gtin)
        {
            _gtin = gtin;
            return this;
        }

        public ProductBuilder SetGcp(string gcp)
        {
            _gcp = gcp;
            return this;
        }

        public ProductBuilder SetName(string name)
        {
            _name = name;
            return this;
        }

        public ProductBuilder SetWeight(float weight)
        {
            _weight = weight;
            return this;
        }

        public ProductBuilder SetLowerThreshold(int lowerThreshold)
        {
            _lowerThreshold = lowerThreshold;
            return this;
        }

        public ProductBuilder SetDiscontinued(int discontinued)
        {
            _discontinued = discontinued;
            return this;
        }

        public ProductBuilder SetMinimumOrderQuantity(int minimumOrderQuantity)
        {
            _minimumOrderQuantity = minimumOrderQuantity;
            return this;
        }

        public ProductDataModel CreateProductDatabaseModel()
        {
            return new ProductDataModel
            {
                Discontinued = _discontinued,
                Gcp = _gcp,
                Gtin = _gtin,
                Id = _id,
                LowerThreshold = _lowerThreshold,
                MinimumOrderQuantity = _minimumOrderQuantity,
                Name = _name,
                Weight = _weight
            };
        }

        public Product CreateProduct()
        {
            return new Product
            {
                Discontinued = _discontinued == 1,
                Gcp = _gcp,
                Gtin = _gtin,
                Id = _id,
                LowerThreshold = _lowerThreshold,
                MinimumOrderQuantity = _minimumOrderQuantity,
                Name = _name,
                Weight = _weight
            };
        }

        public ProductsRequestModel CreateProductRequest()
        {
            return new ProductsRequestModel
            {
                Products = new List<ProductRequestModel>
                {
                    new ProductRequestModel
                    {
                        Discontinued = _discontinued == 1 ? "true" : "false",
                        Gcp = _gcp,
                        Gtin = _gtin,
                        LowerThreshold = _lowerThreshold.ToString(),
                        MinimumOrderQuantity = _minimumOrderQuantity.ToString(),
                        Name = _name,
                        Weight = _weight.ToString()
                    }
                }
            };
        }

        public ProductsRequestModel CreateDuplicateProductRequest()
        {
            return new ProductsRequestModel
            {
                Products = new List<ProductRequestModel>
                {
                    new ProductRequestModel
                    {
                        Discontinued = _discontinued == 1 ? "true" : "false",
                        Gcp = _gcp,
                        Gtin = _gtin,
                        LowerThreshold = _lowerThreshold.ToString(),
                        MinimumOrderQuantity = _minimumOrderQuantity.ToString(),
                        Name = _name,
                        Weight = _weight.ToString()
                    },
                    new ProductRequestModel
                    {
                        Discontinued = _discontinued == 1 ? "true" : "false",
                        Gcp = _gcp,
                        Gtin = _gtin,
                        LowerThreshold = _lowerThreshold.ToString(),
                        MinimumOrderQuantity = _minimumOrderQuantity.ToString(),
                        Name = _name,
                        Weight = _weight.ToString()
                    }
                }
            };
        }
    }
}