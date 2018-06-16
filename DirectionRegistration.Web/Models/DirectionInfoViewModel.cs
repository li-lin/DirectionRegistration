using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Models
{
    public class DirectionInfoViewModel
    {
        public int Id { get; set; }
        public string DirectionName { get; set; }
        public int Order { get; set; }
    }
}