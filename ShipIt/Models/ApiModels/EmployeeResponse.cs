using System.Collections.Generic;

namespace ShipIt.Models.ApiModels
{
    public class EmployeeResponse : Response
    {
        public EmployeeResponse(Employee employee)
        {
            Employees = new List<Employee> {employee};
            Success = true;
        }

        public EmployeeResponse(IEnumerable<Employee> employees)
        {
            Employees = employees;
            Success = true;
        }

        public EmployeeResponse()
        {
        }

        public IEnumerable<Employee> Employees { get; set; }
    }
}