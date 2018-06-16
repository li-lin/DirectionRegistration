using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectionRegistration.Repository.Entities
{
    public class Student
    {
        public Student()
        {
            Scores = new List<Score>();
            DirectionStudents = new List<DirectionStudent>();
        }
        public int Id { get; set; }
        public string Number { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public string Name { get; set; }
        public string Major { get; set; }

        public virtual List<Score> Scores { get; set; }
        public virtual List<DirectionStudent> DirectionStudents { get; set; }

        //public int FirstSelection { get; set; }
        //public int SecondSelection { get; set; }
        //public int ThirdSelection { get; set; }

        //[InverseProperty("FirstStudents")]
        //public virtual Direction FirstSelection { get; set; }
        //[InverseProperty("SecondStudents")]
        //public virtual Direction SecondSelection { get; set; }
        //[InverseProperty("ThirdStudents")]
        //public virtual Direction ThirdSelection { get; set; }
    }
}
