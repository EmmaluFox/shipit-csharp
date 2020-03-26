using System.Collections.Generic;
using ShipIt.Models.ApiModels;

namespace ShipItTest.Builders
{
    public class CompanyBuilder
    {
        private string _addr2 = "1800 West Central";
        private string _addr3 = "";
        private string _addr4 = "IL";
        private string _city = "Mount Prospect";
        private string _gcp = "0000346";
        private string _mail = "info@gs1us.org";
        private string _name = "Robert Bosch Tool Corporation";
        private string _postalCode = "60056";
        private string _tel = "(224) 232-2407";

        public CompanyBuilder SetGcp(string gcp)
        {
            _gcp = gcp;
            return this;
        }

        public CompanyBuilder SetName(string name)
        {
            _name = name;
            return this;
        }

        public CompanyBuilder SetAddr2(string addr2)
        {
            _addr2 = addr2;
            return this;
        }

        public CompanyBuilder SetAddr3(string addr3)
        {
            _addr3 = addr3;
            return this;
        }

        public CompanyBuilder SetAddr4(string addr4)
        {
            _addr4 = addr4;
            return this;
        }

        public CompanyBuilder SetPostalCode(string postalCode)
        {
            _postalCode = postalCode;
            return this;
        }

        public CompanyBuilder SetCity(string city)
        {
            _city = city;
            return this;
        }

        public CompanyBuilder SetTel(string tel)
        {
            _tel = tel;
            return this;
        }

        public CompanyBuilder SetMail(string mail)
        {
            _mail = mail;
            return this;
        }

        public Company CreateCompany()
        {
            return new Company
            {
                Gcp = _gcp,
                Name = _name,
                Addr2 = _addr2,
                Addr3 = _addr3,
                Addr4 = _addr4,
                PostalCode = _postalCode,
                City = _city,
                Tel = _tel,
                Mail = _mail
            };
        }

        public AddCompaniesRequest CreateAddCompaniesRequest()
        {
            return new AddCompaniesRequest
            {
                Companies = new List<Company>
                {
                    new Company
                    {
                        Gcp = _gcp,
                        Name = _name,
                        Addr2 = _addr2,
                        Addr3 = _addr3,
                        Addr4 = _addr4,
                        PostalCode = _postalCode,
                        City = _city,
                        Tel = _tel,
                        Mail = _mail
                    }
                }
            };
        }
    }
}