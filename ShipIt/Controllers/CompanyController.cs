using System.Reflection;
using System.Web.Http;
using log4net;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    public class CompanyController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICompanyRepository companyRepository;

        public CompanyController(ICompanyRepository companyRepository)
        {
            this.companyRepository = companyRepository;
        }

        public CompanyResponse Get(string gcp)
        {
            if (gcp == null) throw new MalformedRequestException("Unable to parse gcp from request parameters");

            log.Info(string.Format("Looking up company by name: {0}", gcp));

            var companyDataModel = companyRepository.GetCompany(gcp);
            var company = new Company(companyDataModel);

            log.Info("Found company: " + company);

            return new CompanyResponse(company);
        }

        public Response Post([FromBody] AddCompaniesRequest requestModel)
        {
            var companiesToAdd = requestModel.companies;

            if (companiesToAdd.Count == 0) throw new MalformedRequestException("Expected at least one <company> tag");

            log.Info("Adding companies: " + companiesToAdd);

            companyRepository.AddCompanies(companiesToAdd);

            log.Debug("Companies added successfully");

            return new Response {Success = true};
        }
    }
}