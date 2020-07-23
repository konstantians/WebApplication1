using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    //here are where we create all the claims templates
    public static class ClaimsStore
    {
        public static List<Claim> AllClaims = new List<Claim>()
        {
            //a claim has type and a value which seemingly are the same
            //a claim can be imported from a configuration file, a database or other , we can also create claims in the model
            new Claim("Create Role","Create Role"),
            new Claim("Edit Role","Edit Role"),
            new Claim("Delete Role","Delete Role")
        };
    }
}
