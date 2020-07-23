using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    //DbContext is the main class in entity framework
    //the main class that we use to interact with whatever database we have
    //it is the class that manages the database connection
    //and also is the class that is used to save and retrieve data
    //the name doesnt matter but the class must inherit from DbContext
    //to use Identity with entity framework we inherite from IdentityDbContext from which the DbContext inherites
    //identity help us manage users authentication(sign-in , sign-out etc)
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        //if we want to pass our own changes to the identityUser which we do by creating a class which inherits from it and adds more properties etc
        //we will have to pass it as a generic parameter to the identitydbContext which by default has the identityuser as a parameter

        //we inject Dbcontexoptions class with the constructor in our class else it wont do anything
        //and then we must pass the parameter to the DbContext by using the base class constructor
        //(the custructor of the DbContext) and then we pass it the options(which is the parameter)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        //we have then to pass all properties of our application in our database(those that we made)
        //for now we only have the employee type in the Employee.cs folder we could have Department.cs
        //which we would have to pass it too , we pass this types to the database with the Dbset function

        public DbSet<Employee> Employees { get; set; }

        //seeding is the creation of a table with initial data(fake or real)
        //to do that we use the new overide method
        //type override on and then enter and it creates it for you
        //as the method that we create implies it says on model creation do something
        //then on modelbuilder we pick the entity we want in this case employee(we could have a department class too for example)
        //and then we use the hasdata method

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //with the base keyword we override the function from dbcontext and intentitydbcontext
            base.OnModelCreating(modelBuilder);
            //we creared our own custom seed function in ModelBuilderExtensions in order for this page to look clean
            modelBuilder.Seed();


            //these lines change the bahavious from cascade to no action which means that if a column is to be deleted
            //which has child elements on another column we have an errorr
            //cascade means that if the column is deleted while it has child columns in another table these child columns get deleted too
            foreach(var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e=> e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
            
        }
    }
}
