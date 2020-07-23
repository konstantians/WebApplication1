using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    //if we want to expand the identityuser class then we must create class that inherites from the identityuser class and add to it
    public class ApplicationUser : IdentityUser
    {
        //we must replace all our instances with the new class which we created which expands the identityuser class
        //we add our new properties
        public string City { get; set; }

        //in order to sync the models and the database we will have to do a migration everytime we change the applicationuser
    }
}
