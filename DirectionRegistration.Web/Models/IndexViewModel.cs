using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DirectionRegistration.Repository.Entities;

namespace DirectionRegistration.Models
{
    public class IndexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string FirstId { get; set; }
        public string SecondId { get; set; }
        public string ThirdId { get; set; }
    }
}