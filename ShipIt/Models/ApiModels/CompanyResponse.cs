namespace ShipIt.Models.ApiModels
{
    public class CompanyResponse : Response
    {
        public CompanyResponse(Company company)
        {
            Company = company;
            Success = true;
        }

        public CompanyResponse()
        {
        }

        public Company Company { get; set; }
    }
}