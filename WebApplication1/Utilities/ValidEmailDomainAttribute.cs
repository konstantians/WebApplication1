using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

//this is how to create custom validation attributes
namespace WebApplication1.Utilities
{
    //it must be derived from an abstract class which is provided by asp.net core
    //we get the allowed domain from the constructor, the constructor gets it from the validation attribute in the model
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        private readonly string allowedDomain;
        public ValidEmailDomainAttribute(string allowedDomain)
        {
            this.allowedDomain = allowedDomain;
        }
        //we can overide methods that the validationAttribute abstract class has such as isvalid which is a virtual class
        //the object here is bound to the property with which the validation attribute is bounded with in our case email
        public override bool IsValid(object value)
        {
            //we expect string so we cast the object value to string
            string[] strings = value.ToString().Split('@');
            //if the allowedDomain is equal to the part after the @ such as gmail.com in our case then return true else return false 
            return strings[1].ToUpper() == allowedDomain.ToUpper();
        }
    }
}
