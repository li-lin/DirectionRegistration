using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirectionRegistration.Repository.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string FirstSelection { get; set; }
        public string SecondSelection { get; set; }
        public string ThirdSelection { get; set; }
    }
}
