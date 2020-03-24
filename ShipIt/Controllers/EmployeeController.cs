using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    public class EmployeeController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeController(IEmployeeRepository employeeRepository)
        {
            this._employeeRepository = employeeRepository;
        }

        public EmployeeResponse Get(string name)
        {
            Log.Info(string.Format("Looking up employee by name: {0}", name));

            var employee = new Employee(_employeeRepository.GetEmployeeByName(name));

            Log.Info("Found employee: " + employee);
            return new EmployeeResponse(employee);
        }

        public EmployeeResponse Get(int warehouseId)
        {
            Log.Info(string.Format("Looking up employee by id: {0}", warehouseId));

            var employees = _employeeRepository
                .GetEmployeesByWarehouseId(warehouseId)
                .Select(e => new Employee(e));

            Log.Info(string.Format("Found employees: {0}", employees));

            return new EmployeeResponse(employees);
        }

        public Response Post([FromBody] AddEmployeesRequest requestModel)
        {
            var employeesToAdd = requestModel.Employees;

            if (employeesToAdd.Count == 0) throw new MalformedRequestException("Expected at least one <employee> tag");

            Log.Info("Adding employees: " + employeesToAdd);

            _employeeRepository.AddEmployees(employeesToAdd);

            Log.Debug("Employees added successfully");

            return new Response {Success = true};
        }

        public void Delete([FromBody] RemoveEmployeeRequest requestModel)
        {
            var name = requestModel.Name;
            if (name == null) throw new MalformedRequestException("Unable to parse name from request parameters");

            try
            {
                _employeeRepository.RemoveEmployee(name);
            }
            catch (NoSuchEntityException)
            {
                throw new NoSuchEntityException("No employee exists with name: " + name);
            }
        }
    }
}