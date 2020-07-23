using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    //[Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        //we inject usermanager which then takes as a generic parameter identityuser
        //user manager allow as to use create async, delete async , update async etc methods
        private readonly UserManager<ApplicationUser> userManager;
        //configure/inject the same way as user manager, allow us to us sign-in async sign-out async , isSignedIn etc methods
        private readonly SignInManager<ApplicationUser> signInManager;

        private readonly ILogger<AccountController> logger;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //the async task<generic parameter> and await is used because identity uses async methods such as create async
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            //check if validation was fine
            if (ModelState.IsValid)
            {
                //create a userF with identity user(it has certain default properties such as username,email etc)
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, City = model.City };
                //then create that user with usermanager and store the result in result
                var result = await userManager.CreateAsync(user, model.Password);
                //check if the result succeeded
                if (result.Succeeded)
                {

                    //this is how to create a confirmation link
                    //first we create a token with the generateemailconfirmationtokenasync method
                    //then we use Url.Action to redirect to add to the confirmation link where the user should be redirected when they click on it
                    //in this case it will be in the controller account the action confirmemail and the parameters are the id of the registered user
                    //and the token, the request.scheme allows the user to be able to click on the link instead of copy pasting it on the browser
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                    logger.LogWarning(confirmationLink);

                    //if we are already registered and we create a new user we shouldnt log in in that user these lines of code do that
                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Listusers", "account");
                    }
                    //sign in the user with percistence false, that means that when we close the browser window the user deauthanticates
                    //so it creates a session cookie, while with persistence true that means that we create a permament cookie which doesnt
                    //deauthenticate the user even if we close the window/browser


                    //redirect to the index page
                    //await signInManager.SignInAsync(user, false);
                    //return RedirectToAction(actionName: "Index", controllerName: "home");

                    //redirect the user to a view where it says to him that he must click on the confirmation link which is either stored locally or sent throught
                    //email externally etc
                    ViewBag.ErrorTitle = "Registration Succesful";
                    ViewBag.ErrorMessage = "Before you login , please confirm your email by clicking the confirmation link we sent to your email account";
                    return View("error");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        //it checks for each error in the results and sends to the modelstate all the errors which when it gets out of the it
                        //passes to the view all the errors in order to display them in the all cattegory
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userid, string token)
        {

            //if one of the 2(userid or the token) is null then redirect the user to the index action
            if (userid == null || token == null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await userManager.FindByIdAsync(userid);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"User with {userid} was not Found";
                return View("NotFound");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View();
            }
            else
            {
                ViewBag.ErrorTitle = "Account Confirmation failed";
                ViewBag.ErrorMessage = "The confirmation has failed, try to copy and paste in the url the confirmation link" +
                    "or contact us";
                return View("error");
            }

            
        }

        //[HttpGet][HttpPost] we can combine these 2 with
        //this will be a remote methof which will be called by the remote validation attribute from the model(in this case the view model registerviewmodel , when it is needed
        //remote methods are very usefull when we want to do comparisons with a lot of data which are in the server in the real time
        //such as to see if the email has been taken or not
        //the string email is binded to this method throught the remote validation attribute and model biding
        [AcceptVerbs("Get","Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"the email {email} is already being used");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel
            {
                //the url to return back throught model binding
                ReturnUrl = returnUrl,
                //externalLogins returns us the list of all configured external login providers
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        //the resulturl is created when someone is prompted to log in if he is not authorized to use/view a certain method
        //because of moddel bidding the returnUrl can be saved in a string with our name in this case just returnUrl
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {

                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View(model);
                }

                //checks validation
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded) {

                    //the resulturl is created when someone is prompted to log in if he is not authorized to use/view a certain method
                    //we use that here to redirect that user to the action that fired the request to create or edit or whatever it did
                    //instead of the default index,home
                    /*returnUrl != null is ok but it is worse for mantainability and a bit on the performance but in general
                     it can be used with no problems*/
                    //if we dont want an excepsion which happens with localredirect, we can check in the if statement if the url is local with 
                    //url.islocalurl(oururl) and if is not it just redirects us to the index action 
                    if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                        //return LocalRedirect(returnUrl); 
                        //we use localRedirect vs RedirectToaction if we dont know where we want to send the user and the redirect is based on the user
                        //if we have a certain path that would always be tha path of the redirection then we use the redirectToaction method
                        //we can use redirect but it is prone to openRedirect vulnerability, so we should
                        //never use it , LocalRedirect is not prone to it because you can only get redirected if you get redirected from this website, not externally
                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }
                }
                //string.empty = '' or "" in almost all cases
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            //because based on the ui when the user is not signed up it wont show up we dont need that if statement
            //if (signInManager.IsSignedIn(User))
            //{
            //}
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider,string returnUrl)
        {
            //the string provider is provided to us throught model biding(because of the name attribute and because of course they match'provider'
            //from the attribute value in the button
            //we make the redirect Value to return to externallogincallback action throught the account controller and pass the return url as a parameter
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            
            //challenge result provides the view of the login provider page
            return new ChallengeResult(provider,properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null,string remoteError = null)
        {
            //if null which is the default we state if it doesnt work
            //?? returns the value of the left side before the ?? operator if it is not null else it return the right part
            //returnUrl is used to return us to the action that called the login action for example createUser
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"error from external provider : {remoteError}");

                return View("Login", loginViewModel);
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            //gets various information about the user, such as  loginprovider, provider key etc
            if(info == null)
            {
                ModelState.AddModelError(string.Empty, "error external login information");

                return View("Login", loginViewModel);
            }


            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = null;
            //to check if the email is confirmed
            //in case of the external login we dont check if the login was right because we dont have the password , so that happens on the external
            //provider side
            if(email != null)
            {
                //i do based on the name because my table is fucked(aspnetusers)
                user = await userManager.FindByNameAsync(email);

                if(user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "email is not confirmed yet");
                    return View("Login", loginViewModel);
                }
            }


            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            //if the user doesnt have a external account created (for example with google or facebook or whatever) in the database then the login in fails
            //and the following code create that account in the database, in asp.netUserlogins table
            else
            {
                //these lines search if the user has already an email and if they have then it adds
                //a row to the table netuserlogins(links external account with local account)
                //and it signs in the user, and then it redirects the user to the action prior to the login action
                
                if(email != null)
                { 

                    if(user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Name)
                        };

                        await userManager.CreateAsync(user);
                    }

                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, false);

                    return LocalRedirect(returnUrl);
                }
                //if we dont receive the email from the external login provider
                else
                {
                    ViewBag.ErrorTitle = $"email claim was not received from{info.LoginProvider}";
                    ViewBag.ErrorMessage = "Please contact us at konstantinos@gmail.com";
                    return View("error");
                }
            }

        }



    }
}
