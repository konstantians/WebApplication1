using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using WebApplication1.Models;
using WebApplication1.Security;

namespace WebApplication1
{
    public class Startup
    {

        private IConfiguration _config;

        public Startup( IConfiguration config)
        //this creates a object Iconfiguration _config 
        {
            _config = config;
            //the constructor brings the Iconfiguration interface with the variable config
            //the we pass it to _config (for example part of this is the appsettings.json )
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        //all services in the container(configure Services start with services)
        public void configureServices(IServiceCollection services)
        {
            //just use AddDbContextPool it is better than the other because it doesnt always
            //create new instance like the addbcontext
            //then specify database provider in the() with options => options.x whatever we want in this instant microsoft server
            //and finaly we use a connection string
            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer("server=(localdb)\\MSSQLLocalDB;database=EmployeeDB;Trusted_Connection=true"));
            //options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection")));


            //we add the service for the identity and we pass it 2 generic parameters , one is user (IdentityUser) and the other is role(IdentityRole)
            //identityUser give as the default data for the user and the identity role how they are supposed to work
            //and then we store these users and their role in the database with addentityframeworkstores<(our DbContext class)> service
            //we can alse set the defaults for identityoptions which is in identity manager and configures the password of each user
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 3;
                //make sure that the email is confirmed
                options.SignIn.RequireConfirmedEmail = true;
                //options.Password.RequireNonAlphanumeric = false;
                //we use default token providers to create tokens for email confirmation and other stuff
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            //one way to do it with services.configure but we are using services.addiddentity so we do that from there
            //services.Configure<IdentityOptions>(options =>
            //    {
            //        _ = options.Password.RequiredLength == 8;
            //        _ = options.Password.RequiredUniqueChars == 3;
            //        _ = options.Password.RequireNonAlphanumeric == false;

            //    });

            //we use the lambda expression(the options => thing) to make the authorization global with a policy
            //we crate a new AuthorizationPolicyBuilder object then we configure that to do something the user must be authorized and then we build it
            //then we pass it to the filters
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                //we pass the policy in the constructor of the new authorizefilter we create
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });


            //thats how you add authorization
            services.AddAuthorization(options =>
            {
                //the name which we utilize with the authorization attribute(on controllers or on actions) and the requireclaim where the
                //delete claim lies within the ClaimStore in models
                options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role", "true"));

                //options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role", "true", "yes"));
                //the first string is the claim type and the 2 other strings are its values, in order for the delete policy to
                //be satisfied the user must have the delete role claim with either a value of true or yes

                //the first parameter is the type(edit role) and the second is the value(there can be many values)
                //options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role", "true"));

                //essentionally roles are claims , this happened because in the beginning there were only roles and now claims contain roles
                options.AddPolicy("AdminRolePolicy", policy => policy.RequireClaim("Admin"));

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Admin").RequireClaim("Edit Role", "true")
                //.RequireClaim("SuperAdmin")); this is connected throught and , in this case this means the user must be admin he must
                //have edit role type claim with value true and his role must be super admin too to access controllers or actions guarded with the editrolepolicy

                //we use the following statement with the help of the func(lambda) to have an or
                //in this case the user must be admin, he must have edit role claim with value true OR he must be a superadmin
                //options.AddPolicy("EditRolePolicy", policy =>
                //{
                //    policy.RequireAssertion(context =>
                //      context.User.IsInRole("Admin") && context.User.HasClaim("Edit Role", "true")
                //      || context.User.IsInRole("SuperAdmin"));
                //});


                //to add custom requirement policy and handlers
                //and then every time we implement the interface IAuthorizationRequirement we have to pass a Manageadminrolesandclaimsrequirement instance
                options.AddPolicy("EditRolePolicy", policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

                //options.InvokeHandlersAfterFailure = false; this lines mean stop executing handlers code after failure because in case of failure on a handler
                //no matter if the other handlers succeed the outcome will be access denied, so we can save time not running useless code with this property
            });

            //add authentication service(google) and pass configuration the client id and the client secret
            services.AddAuthentication().AddGoogle(options => {
                options.ClientId = "434378697919-tqein0b55jboi72mr1cnnr37jdqteqtu.apps.googleusercontent.com";
                options.ClientSecret = "pAGYbyA9jVfySGvMpxP0y6Cg";
            }).AddFacebook(options => {
                options.AppId = "4693048180721387";
                options.AppSecret = "676443d649ef9a51deb84993995199d4";
                });
            
            //services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();//
            //the lines mean when someone want to use this interface give them a singleton of the MockEmployeeRepository
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>(); //if we want to change it we just change the sql to mock like the above commented line
            //there is singleton transient and scoped , singleton survives till the end of the application
            //transient service is created its time it is requested and
            //scoped service is create its time it is requested in the scope

            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            //the middleware start from start to finish except if the middleware breakes the chain
            //which means the middleware wont continue and it will reverse back that why exception errors
            //must be first (to end the chain in case of an error immediately), middlewares must be in the correct order


            //middleware configurations are based on their name for example if you want to configure fileserver
            //middleware you use fileserveroptions object the same goes for all it is the name of the middle
            //plus options


            //first middleware
            if (env.IsDevelopment())
            {
                DeveloperExceptionPageOptions exceptionoptions = new DeveloperExceptionPageOptions
                {
                    SourceCodeLineCount = 10

                };
                app.UseDeveloperExceptionPage(); //get exception(if something goes wrong) only on developer environment
                //from other middlewares
            }
            else
            {
                //we can use usestatuscode but it is really bad because it literally has almost no ui, the default 404 error is better
                //also we can use usestatuscodepagewithredirects but we dont use it because it is semantically worse and for a minor change in the ui
                //error and the {0} is a placeholder which can mean for example 404
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            //to serve static files all files must be in wwwroot folder
            //DefaultFilesOptions options = new DefaultFilesOptions(); //creates an object defaultfileoptions
            //options.DefaultFileNames.Clear(); //clears the default path
            //options.DefaultFileNames.Add("foo.html"); //adds a new path
            //app.UseDefaultFiles(options); //the middleware that changes the default files path to default.html or index.html
            ////it must be before the usestaticfiles() middleware
            //app.UseStaticFiles();

            //app.usefileserver middleware does what app.defaultfilenames and app.usestaticfiles do
            //+ optionally app.usedirectory browsing which allows users to see the files in the directories if needed


            //FileServerOptions options = new FileServerOptions(); //creates an object fileserveroptions
            //options.DefaultFilesOptions.DefaultFileNames.Clear();//clears the default path
            //options.DefaultFilesOptions.DefaultFileNames.Add("foo.html"); //adds a new path
            //app.UseFileServer(options); 
            app.UseStaticFiles();

            //it is used with identity core to authenticate users(login-logout) and it must be before mvc because the authentication comes first
            app.UseAuthentication();

            //app.UseMvcWithDefaultRoute(); //the staticfiles middleware must be before the mvc bcause if the 
            //request is for a static file the use staticfiles middleware stops the pipeline saving time
            //app.addMvc is the mvc that contains everything and app.addMvcCore is the mvc that contains
            //core functionality of the mvc in most cases app.addMvc should be use(it is a wrapper of the app.addmvccore)
            //usemvcwithdefaultroute sets the default route to {controller = home}/{action = index}/{id?}
            app.UseMvc(routes =>
            {
                //the = are used for defaults if they are not specified in the search bar in the webrowser
                //this is called conventional routing
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
                //routes.MapRoute("second", "{controller=Error}/{action=Error}/{statusCode?}");

            }); //same as usemvcwithdefaultroute() but it doesnt have a default route 

            //app.Run(async (context) =>
            //{
            //    //Exception exception = new Exception("you piece of shit");
            //    //throw exception;
            //    //await context.response.writeasync to write something in the screen when the webhost get an http request 
            //    await context.Response.WriteAsync("Hello World");
            //});

            //use not terminal middleware run terminal middleware (terminal middleware = stops the pipeline and returns back)
            //with use add another arguement next and then in the end of the miidleware use await next() to use next middleware
        }
    }
}
