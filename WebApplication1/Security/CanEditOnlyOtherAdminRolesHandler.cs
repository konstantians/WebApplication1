using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication1.Security
{
    public class CanEditOnlyOtherAdminRolesHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimsRequirement requirement)
        {
            //context resource returns the actions that we are protecting
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            if(authFilterContext == null)
            {
                //return task.completedtask means it just passes to the next controller and it hasnt suceeded yer
                return Task.CompletedTask;
            }

            //name identifier has the id of the us which is passed as a parameter on the action, here we return who is the logged admin
            string loggedAdminId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            //adminBeingEdited is the current user who is being edited, by finding the user throught the query string in the url in this case userId
            string adminBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];

            if(context.User.IsInRole("admin") && context.User.HasClaim(c => c.Type == "Edit Role" && c.Value == "true") &&
                adminBeingEdited.ToLower() != loggedAdminId.ToLower())
            {
                context.Succeed(requirement);
            }
            //else
            //{
            //    context.Fail(); //we shouldnt use failures in general because failure overules successes on other handlers , so they should ony be used
            //    //in very specific cases
            //}

            return Task.CompletedTask;


        }
    }
}
