using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace WebApplication1.Controllers
{
    //dont forget controllers must inherit from the class controller
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;
        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger;
        }

        //attribute routing
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {

            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "sorry we could not find the resource you requested";
                    logger.LogWarning($"404 error occured. Path = {statusCodeResult.OriginalPath}" +
                        $"Query string = {statusCodeResult.OriginalQueryString}");
                    break;
                case 500:
                    ViewBag.ErrorMessage = "for some reason 500 error";
                    break;
            }

            return View("NotFound");

            
        }


        [Route("Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            //string interpolation pretty much makes a string the outcome of what is in the curly bracets
            logger.LogError($"the path {exceptionDetails.Path} " +
                $"threw an exception {exceptionDetails.Error}");
            //we dont want these things to be displayed to the user so we log them
            //ViewBag.Path = exceptionDetails.Path;
            //ViewBag.StackTrace = exceptionDetails.Error.StackTrace;
            //ViewBag.ExceptionMessage = exceptionDetails.Error.Message;

            return View("Error");
        }
    }
}
