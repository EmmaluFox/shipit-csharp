using System.Web.Http;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    public class Status
    {
        public int WarehouseCount { get; set; }
        public int EmployeeCount { get; set; }
        public int ItemsTracked { get; set; }
        public int StockHeld { get; set; }
        public int ProductCount { get; set; }
        public int CompanyCount { get; set; }
    }

    public class StatusController : ApiController
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;

        public StatusController(IEmployeeRepository employeeRepository, ICompanyRepository companyRepository,
            IProductRepository productRepository, IStockRepository stockRepository)
        {
            _employeeRepository = employeeRepository;
            _stockRepository = stockRepository;
            _companyRepository = companyRepository;
            _productRepository = productRepository;
        }

        // GET api/status
        public Status Get()
        {
            return new Status
            {
                EmployeeCount = _employeeRepository.GetCount(),
                ItemsTracked = _stockRepository.GetTrackedItemsCount(),
                CompanyCount = _companyRepository.GetCount(),
                ProductCount = _productRepository.GetCount(),
                StockHeld = _stockRepository.GetStockHeldSum(),
                WarehouseCount = _employeeRepository.GetWarehouseCount()
            };
        }
    }
}