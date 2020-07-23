using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModels
{
    public class EditRoleViewModel
    {

        public EditRoleViewModel()
        {
            Users = new List<String>();
        }

        public string Id { get; set; }
        
        [Required(ErrorMessage = "Role Name is Required")]
        public string RoleName { get; set; }

        public List<String> Users { get; set; }
    }
}
