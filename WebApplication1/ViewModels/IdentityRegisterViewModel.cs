using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Utilities;

namespace WebApplication1.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Remote("isemailinuse","account")] //it is being used to run the method isemailinuse in the account controller
        //to include our custom validation attribute we use its name with the attribute in the end or without it for example valid email domain
        //and we must include the directory that it is in in our case Webapplication/utilities
        [ValidEmailDomain(allowedDomain:"gmail.com",ErrorMessage = "email domain must be gmail.com")] //our custom validation attribute inherits all the
        //properties of the abstact ValidationAttribute class which contains the errormessate property too so that the reason we can use it here
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password",ErrorMessage ="Password and confirmation does not match")]
        public string ConfirmPassword { get; set; }

        //[Required]
        public string City { get; set; }
    }
}
