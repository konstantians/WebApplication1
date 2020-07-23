using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    //the reason we litterally copy paste the model employee and create this viewmodel is because to work well with the
    //photopath(photo) we ineed iformfile and if we do that on the model it will complicate matters , so we copy paste
    //the model code and change the type from string to iformfile
    //because of the model biding the iformfile will connect with the string in the employee model so we will be fine
    public class EmployeeCreateViewmodel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Name can not exceed 50 characters")]
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Invalid Email format")]
        [Display(Name = "Office Email")]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }
        public IFormFile Photo { get; set; } //or photopath whatever you want to call it
        //visual studio refactoring tools allow as to rename a variable wherever it is from then photo
        //to photos for example, to use them we use control period
    }
}
