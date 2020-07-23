using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//the interface that has all the methods for the data manipulation that the class MockEmployeeRepository.cs has
//to implement , we do this for dependency injection because it requires an interface
namespace WebApplication1.Models
{
    public interface IEmployeeRepository
    {
        Employee GetEmployee(int id);
        IEnumerable<Employee> GetAllEmployee();
        Employee Add(Employee employee);
        Employee Update(Employee employechanges);
        Employee Delete(int id);
    }
}
