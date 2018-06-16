using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Models
{
    public class DirectionSaveViewModel
    {
        public int Sid { get; set; }
        public List<DirectionOrderViewModel> Orders { get; set; } 
    }
    public class DirectionOrderViewModel
    {
        public int Did { get; set; }
        public int Order { get; set; }
    }
}