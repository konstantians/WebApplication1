using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Viewmodels
{
    public class HomeDetailsViewModel
    {
        //a view model summarizes information that should be sent to a view
        //because with a model you might not be able to send all the information
        //which are required in this instance the Pagetitle
        public Employee Employee { get; set; }
        public string PageTitle { get; set; }
    }
}
