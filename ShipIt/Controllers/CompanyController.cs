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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICompanyRepository _companyRepository;

        public CompanyController(ICompanyRepository companyRepository)
        {
            this._companyRepository = companyRepository;
        }

        public CompanyResponse Get(string gcp)
        {
            if (gcp == null) throw new MalformedRequestException("Unable to parse gcp from request parameters");

            Log.Info($"Looking up company by name: {gcp}");

            var companyDataModel = _companyRepository.GetCompany(gcp);
            var company = new Company(companyDataModel);

            Log.Info("Found company: " + company);

            return new CompanyResponse(company);
        }

        public Response Post([FromBody] AddCompaniesRequest requestModel)
        {
            var companiesToAdd = requestModel.Companies;

            if (companiesToAdd.Count == 0) throw new MalformedRequestException("Expected at least one <company> tag");

            Log.Info("Adding companies: " + companiesToAdd);

            _companyRepository.AddCompanies(companiesToAdd);

            Log.Debug("Companies added successfully");

            return new Response {Success = true};
        }
    }
}