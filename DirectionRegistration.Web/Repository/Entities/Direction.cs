using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace DirectionRegistration.Repository.Entities
{
    public class Direction
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public Teacher Teacher { get; set; }

        //public virtual List<Student> FirstStudents { get; set; }
        //public virtual List<Student> SecondStudents { get; set; }
        //public virtual List<Student> ThirdStudents { get; set; }
    }
}
