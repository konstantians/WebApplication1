using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Employee
    {
        //fastest way to create a property is by typing prop and then pressing tab
        //this is the class that has the data
        public int Id { get; set; }
        [Required] //to make it be required in a form these attributes must be 
        //on the top, before the variable in this instance name like the attribute [route]
        //which in used in attribute routing
        [MaxLength(50,ErrorMessage = "Name can not exceed 50 characters")]
        //maxlength doesnt allow a variable to exceed a certain amount of characters(minlength does the opposite)
        //it can have a custom errormessage with the second parameter errormessage
        public  string Name { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Invalid Email format")]
        //to customize regular expression with a custom error message you use errormessage and then write the message
        //i am not certain how to make the regular expression obviously a-z means you can right from a to z but i dont know
        //why we write that 3 time  seems when it reaches + @ means after @ and +\. means after the . 
        //and the ^means start and the +$ means end of the expression, the expression checks that we have not more
        //than these characters in certain parts of the string
        [Display(Name = "Office Email")]
        //you can stack this [...] attributes for example email has regularexpression, required
        //and display
        public string Email { get; set; } 
        
        //[Required] in select the required attribute is not required because dept is by default required
        //we can make it not required by adding a ? in the end , for example Dept?
        //then you can make the Dept? required by adding before the Dept the property [Required]
        [Required]
        public Dept? Department { get; set; }

        //once we do a change in the model we have to do a migration to the model and the database schema in sync
        //for example when we add a property photopath we have to do a migration
        public string Photopath { get; set; }
    }
}
