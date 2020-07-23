using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
//using System.Web.Http;
using WebApplication1.Models;
using WebApplication1.Viewmodels;
using WebApplication1.ViewModels;

//the controller does the routing the default routing is homecontroller/index
//we can change the default controller and method
//the controller must always implement the controller class
namespace WebApplication1.Controllers
{
    //we can add authorize propery which help us authorize users based on if they are logged in or not, on the controller too
    //[Authorize]
    //a controller responds to the http request accordingly(for example by providing a view)
    //if we use allowanonymous attribute in controller level it makes everything accesible by everyone even if we specifically
    //use the authorize attribute in a method , so we dont do it
    public class HomeController : Controller
    {
        //cunstructor injector
        private readonly IEmployeeRepository _employeeRepository; //read only for not adding by mistake
        //in a action something to it, it is only for reading,good practise
        private readonly IHostingEnvironment hostingEnvironment;
        //<x> are generic parrameters , illoger uses one for example.Generic parameter tell you to give the type of object that you want for example int
        //string or your own class or interface
        private readonly ILogger<HomeController> logger;

        //to get root filepath we have to inject in the controller the ihostingenvironment
        public HomeController(IEmployeeRepository employeeRepository, IHostingEnvironment hostingEnvironment, ILogger<HomeController> logger
            )
        {

            this.logger = logger;
            this.hostingEnvironment = hostingEnvironment;
            _employeeRepository = employeeRepository;
        }

        //this atribute allows users who havent signed in and users that have signed in to use/view this method
        [AllowAnonymous]
        //the controller works with action methods such as index
        //when a user writes localhost:4000/home/details
        //the home is mapped on the homecontroller and the index is mapped in the details action
        //if we want to pass any parameter like id it goes something like localhost:4000/home/details/1
        //the id which is the parameter it is mapped  
        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployee();
            return View(model);
        }

        //[Authorize]//allows only signed in users to use this method, if it you are not signed it by default gets you to the sign in page
        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Department = employee.Department,
                Email = employee.Email,
                ExistingPhoto = employee.Photopath
            };
            return View(employeeEditViewModel);

        }

        //[Authorize]
        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                if (model.Photo != null)
                {
                    if (model.ExistingPhoto != null)
                    {
                        string filepath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExistingPhoto);
                        //to delete a file we use this class
                        System.IO.File.Delete(filepath);
                    }
                    employee.Photopath = ProcessUploadedFile(model);
                }

                _employeeRepository.Update(employee);
                return RedirectToAction("index");

            }

            return View();



        }



        //a form consists of 2 creates action one when it gets the httpget to complete the form
        //and the other when the form is submitted
        //in the get you just pass the view
        [HttpGet]
        //[Authorize]  
        public ViewResult Create()
        {
            return View();
        }

        //the post gets from the form the employee atributes, here we get based on the form
        //employee name ,employee email and employee department
        //so we must give that to the new create action for it to build the model
        //and then we can redirect to some action
        [HttpPost]
        //[Authorize]
        public IActionResult Create(EmployeeCreateViewmodel model)
        {
            //to check if validation has suceeded
            if (ModelState.IsValid)
            {
                //because we use this in the edit view we can refactore it to do that we select the lines of code
                //and then extract so it creates its own method bellow 
                string uniqueFilename = ProcessUploadedFile(model);

                Employee newEmployee = new Employee
                {
                    Id = model.Id,
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    Photopath = uniqueFilename
                };

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }

            //else return create view
            return View();

            //new {id = newEmployee.Id is used to pass the new id to the details
            //because the id is the arguement in the path controller:home action:details
            //arguement:id it could be abc if the arguement/parameter on the action was abc
        }

        private string ProcessUploadedFile(EmployeeCreateViewmodel model)
        {
            string uniqueFilename = null;
            if (model.Photo != null)
            {
                //hostingEnvironment.Webrootpath returns the wwwrootpath and then to get to the images
                //we have to combine the path with the images folder
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                //Guid = global unique identifier
                uniqueFilename = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFilename);
                //copy the new filenamepath to the sqlserver to save the image
                //we use the using to avoid certain problems with sequences like someone creating and editing the image
                //that the added to the web page immediately, its good practise
                using (var filestream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(filestream);
                }

            }

            return uniqueFilename;
        }

        [AllowAnonymous]
        public ViewResult Details(int? id)
        {
            //log outputs based on their level
            logger.LogTrace("Trace from details, level 0");
            logger.LogDebug("Debug from details, level 1");
            logger.LogInformation("Information from details, level 2");
            logger.LogWarning("Warning from details, level 3");
            logger.LogError("Error from details, level 4");
            logger.LogCritical("Critical from details, level 5");
            //throw new Exception("exception motherfuckers");

            //you need the id.Value if you are using int? type
            //we pass response status with response.statuscode
            Employee employee = _employeeRepository.GetEmployee(id.Value);
            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", id.Value);
            }

            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel()
            {
                Employee = employee,
                PageTitle = "Employee Details"
            };
            return View(homeDetailsViewModel);


            //Employee model = _employeeRepository.GetEmployee(1); //the controller creates/passes the model
            //and then it passes to the view the model and return the view

            //ViewData["Employee"] = model; //one way to pass the model to the view is viewdata
            //ViewData["PageTitle"] = "Employee Details";

            //ViewBag.Employee = model; //viewbag is another way to pass data to the view, it is an extension of view data
            //ViewBag.PageTitle = "Employee Details";

            //most of the times we uses strongly typed view which is the following

            //we can pass model or the viewmodel instance
            //default paths for view are /views/home/details.cshtml, /views/shared/details.cshtml, /pages/shared/details.cshtml
            //the details in and the home are derived from the homecontroller(home) and the action details(details.cshtml)
            //if you dont want the default you can use
            //return View("path",model); path for example Views/Details.cshtml 

            //with view you can use an absolute path or a relative path
            //an absolute path starts from route something like that /Views/Home/Details.cshtml
            //or using a ~ in front  ~/Views/Home/Details.cshtml, in an absolute path the extension must be added .cshtml here
            //a relative path depends on the position of the home folder compaired to the destination
            // ../Views/Test/Index (test,index is an example) .. is used to go back we used that here because we start in the Home folder
            // and we had to go to the root folder , in a relative path name extensions are not needed


        }
    }
}
