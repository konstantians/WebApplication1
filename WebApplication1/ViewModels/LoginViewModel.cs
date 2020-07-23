using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModels
{
    public class LoginViewModel
    {
        //the validation attributes such as required use server side validation
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        //display is useful if we want to have 2 words which we cant have with a property
        //[Required] because we want to allow to be able to not pick it , usefull in checkboxes
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        //used for the authentication with google
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        //and the return Url
        public string ReturnUrl { get; set; }
    }
}
