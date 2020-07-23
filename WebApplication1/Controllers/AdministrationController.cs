using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    //[Authorize] this is simple authentication which means that the user can use the controller if they are signed in
    //[Authorize(Roles = "Admin,User")] is a roles based authorization which means that the user can access the controller if he is part of the role
    //admin or user
    /*[Authorize(Roles = "Admin")]
      [Authorize(Roles = "User")] means that the user must be part of both roles  */
    //so there are 3 ways and authorization doesnt need to be on the controller to overide authorization on controller we either use 
    //authorize validation attribute with its parameters(whatever we want) on an action or we can use allowanonymous(which allows anyone to use that action)
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {

        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ILogger<AdministrationController> logger;
        public AdministrationController(RoleManager<IdentityRole> roleManager ,UserManager<ApplicationUser> userManager, ILogger<AdministrationController> logger)
        {
            this.logger = logger;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole { Name = model.RoleName };
                IdentityResult result = await roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("listroles", "administration");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError( "", error.Description);
                    }
                }
            }
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"user with id = {user.Id} cannot be found";
                return View("NotFound");
            }

            var userRoles = await userManager.GetRolesAsync(user);
            var userClaims = await userManager.GetClaimsAsync(user);

            var model = new EditUserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                City = user.City,
                Roles = userRoles,
                Claims = userClaims.Select(c => c.Type + " : " + c.Value).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"user with id = {user.Id} cannot be found";
                return View("NotFound");
            }
            user.UserName = model.UserName;
            user.City = model.City;
            user.Email = model.Email;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("listusers", "administration");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }  
        }

        [HttpGet]
        //[Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if ( role == null)
            {
                ViewBag.ErrorMessage = $"role with id = {role.Id} cannot be found";
                return View("NotFound");
            }
            var model = new EditRoleViewModel()
            {
                Id = role.Id,
                RoleName = role.Name,
            };

            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    //we add the user which is an instance of application user to the Users which is an instance of the editroleviewmodel
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if(role == null)
            {
                ViewBag.ErrorMessage = $"role with id = {model.Id} cannot be found";
                return View("NotFound");
            }
            role.Name = model.RoleName;
            var result = await roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction("listroles", "administration");
            }
            else
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);
            if(role == null)
            {
                ViewBag.ErrorMessage = "Role was not found";
                return View("NotFound");
            }
            var model = new List<UserRoleViewModel>();

            foreach(var user in userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName
                };

                if(await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }

                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model,string roleid)
        {
            var role = await roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                ViewBag.ErrorMessage = "Role was not found";
                return View("NotFound");
            }

            for(int i = 0; i< model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].Id);
                IdentityResult result = null;
                if(await userManager.IsInRoleAsync(user,role.Name) && !model[i].IsSelected)
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else if(!(await userManager.IsInRoleAsync(user,role.Name)) && model[i].IsSelected)
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if(i < (model.Count - 1))
                    {
                        continue;
                    }
                    else
                    {
                        return RedirectToAction("editrole", new { id = role.Id });
                    }
                }
            }

            return RedirectToAction("editrole", new { id = role.Id });

        }


        [HttpGet]
        public IActionResult ListRoles()
        {
            //iquaryable is like a list but better on this cenario this is what the rolemanager.roles return an iquaryable object
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }

        [HttpPost]
        [Authorize(Roles ="DeleteRolePolicy")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id = {user.Id} was not found";
                return View("NotFound");
            }
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("listusers", "administration");
            }
            else
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("listusers");
            }
            

        }

        [HttpPost]
        //this is claim based authorization, to use we use the keyword policy and we specify the claim which is based on the startup authorization service
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with id = {role.Id} was not found";
                return View("NotFound");
            }
            try
            {
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("listroles", "administration");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("listroles");
                }

            }
            //to find the name of the exception add general exception ex and then a breakpoint , once the exception happens and it stops on the breakpoint
            //it reaveals the name of the exception in this case DbUpdateException, in the nonstatic part on the arrow the moment you reach the
            // exception breakpoint,when you hover over the ex, there is the classname of the exception
            //then you can redirect after the catch the user to a view passing data throught viewbag or strongly typed view/model
            //and typicaly we log the exception with a local logging provider or an external logging provider for debugging
            catch(DbUpdateException ex)
            {
                logger.LogError($"Error deleting role {ex}"); //in our case it is logged with nlog
                ViewBag.ErrorTitle = $"{role.Name} role is in use";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted , to delete the role first remove all the users from that role";
                return View("error");
            }

        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userid)
        {
            ViewBag.userid = userid;
            var user = await userManager.FindByIdAsync(userid);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"User with id = {user.Id} was not found";
                return View("NotFound");
            }
            var model = new List<ManageUserRolesViewModel>()
            {

            };
            
            foreach(var role in roleManager.Roles)
            {
                var manageUserRolesViewModel = new ManageUserRolesViewModel()
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                if(await userManager.IsInRoleAsync(user, role.Name))
                {
                    manageUserRolesViewModel.IsSelected = true;
                }
                else
                {
                    manageUserRolesViewModel.IsSelected = false;
                }

                model.Add(manageUserRolesViewModel);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(List<ManageUserRolesViewModel> model,string userid)
        {
            var user = await userManager.FindByIdAsync(userid);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id = {user.Id} was not found";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "cannot remove user from role");
                return View(model);
            }

            result = await userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "cannot add user to role");
                return View(model);
            }

            return RedirectToAction("listusers", "administration");


        }

        [HttpGet]
        //[Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(string userid)
        {
            var user = await userManager.FindByIdAsync(userid);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"User with id = {user.Id} was not found";
                return View("NotFound");
            }

            var ExistingUserClaims = await userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel{
                UserId = userid
            };

            foreach(Claim claim in ClaimsStore.AllClaims)
            {

                UserClaim userclaim = new UserClaim
                {
                    ClaimType = claim.Type,
                    
                };

                if(ExistingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true"))
                {
                    userclaim.IsSelected = true;
                }

                model.Claims.Add(userclaim);
                    
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model, string userid)
        {
            var user = await userManager.FindByIdAsync(userid);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with id = {user.Id} was not found";
                return View("NotFound");
            }

            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "problem with removing claims");
                return View(model);
            }

            //if we go with the claimtype = claimvalue 
            //result = await userManager.AddClaimsAsync(user, model.Claims.Where(x => x.IsSelected).Select(y => new Claim (y.ClaimType , y.ClaimType)));

            //claim value is string
            result = await userManager.AddClaimsAsync(user, model.Claims.Select(y => new Claim(y.ClaimType, y.IsSelected ? "true" : "false")));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "problem with adding claims");
                return View(model);
            }

            return RedirectToAction("EditUser", "administration", new {Id = model.UserId });

        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
