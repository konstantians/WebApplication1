using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    //the class that implements IEmployeeRepository.cs and thus it does all the data manipulation
    //a repository should do all the important operations such as create,read,update and delete
    //we mostly use in memory repository for testing purposes
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employeeList;

        public MockEmployeeRepository()
        {
            //add some static data but the data must be in the database so
            //thats not the way to do it, it is only temporary
            _employeeList = new List<Employee>()
            {
                new Employee
                {
                    Id = 1,
                    Name = "Konstantinos",
                    Email = "yolo@gmail.com",
                    Department = Dept.HR //because of the webmodels1.Models import we
                    //just write Dept.HR like we would with employee for example employee.Name
                },
                new Employee
                {
                    Id = 2,
                    Name = "Giorgos",
                    Email = "jesus@gmail.com",
                    Department = Dept.IT
                },
                new Employee
                {
                    Id = 3,
                    Name = "Konstantinos",
                    Email = "swag@gmail.com",
                    Department = Dept.IT
                }
            };
        }
        
        public Employee GetEmployee(int id)
        {
            //e => means the becomes that which happens after => this is a lambda expression
            //e.Id == id which means if e.Id == id then it e.id becomes id it seems that with this way this
            //does a loop
            return _employeeList.FirstOrDefault(e => e.Id == id);
        }

        public IEnumerable<Employee> GetAllEmployee()
        {
            return _employeeList;
        }

        //here the employee has employee name employee email and employee department
        //because of the form but it doesnt have an id so we must create it
        public Employee Add(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1; //creates new employee id
            _employeeList.Add(employee); //adds the new employee to the list
            return employee; //returns that employee

        }

        public Employee Update(Employee employeechanges)
        {
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == employeechanges.Id);
            if (employeechanges != null)
            {
                employee.Name = employeechanges.Name;
                employee.Email = employeechanges.Email;
                employee.Department = employeechanges.Department;
            }
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee employee = _employeeList.FirstOrDefault(e => e.Id == id);
            if(employee != null)
            {
                _employeeList.Remove(employee);
            }
            return employee;
            
        }
    }

}
