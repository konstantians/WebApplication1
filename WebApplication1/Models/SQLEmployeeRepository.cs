using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    //the repository that is connected with the sql server , it should implement the IEmployeeRepository
    //as the mockEmployeeRepository does
    public class SQLEmployeeRepository : IEmployeeRepository
    {

        private readonly AppDbContext context;
        private readonly ILogger<SQLEmployeeRepository> logger;

        //with this injection the context variable gets all the methods of the DBContext class
        public SQLEmployeeRepository(AppDbContext context, ILogger<SQLEmployeeRepository> logger)
        {
            this.logger = logger;
            this.context = context;
        }
        
        //Appdbcontext has Employees(one of the types we set with the dbset<>
        //add to add another employee method based on the dbcontext class
        //and savechanges is another method of the dbcontext class that saves changes
        public Employee Add(Employee employee)
        {
            context.Employees.Add(employee);
            context.SaveChanges();
            return employee;
        }

        //find is another method of the dbcontext class that finds an item based on an id or a name or whatever
        public Employee Delete(int id)
        {
            Employee employee = context.Employees.Find(id);
            if(employee != null)
            {
                context.Employees.Remove(employee);
                context.SaveChanges();
                
            }
            return employee;
        }

        public IEnumerable<Employee> GetAllEmployee()
        {
            return context.Employees;
        }

        public Employee GetEmployee(int id)
        {
            return context.Employees.Find(id);
        }

        //attach makes the changes based on the primary key? at least this is what i get
        public Employee Update(Employee employeechanges)
        {
            var employee = context.Employees.Attach(employeechanges);
            employee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return employeechanges;
        }
    }
}
