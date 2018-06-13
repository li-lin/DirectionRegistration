using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Models
{
    public class RegistrationViewModel
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Major { get; set; }
        public string FirstSelection { get; set; }
        public string SecondSelection { get; set; }
        public string ThirdSelection { get; set; }
    }
}