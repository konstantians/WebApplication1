using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//we create this extension class to not have the model build class full of code and keep it clean
//extensions classes must be static
namespace WebApplication1.Models
{
    public static class ModelBuilderExtensions
    {
        public static void  Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                    new Employee
                    {
                        Id = 1,
                        Name = "Mary",
                        Department = Dept.IT,
                        Email = "mary@gmail.com"
                    },
                    new Employee
                    {
                        Id = 2,
                        Name = "John",
                        Department = Dept.HR,
                        Email = "john@gmail.com"
                    }
                );
        }
    }
}
