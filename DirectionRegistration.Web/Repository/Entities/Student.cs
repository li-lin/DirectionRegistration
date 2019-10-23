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
        /// <summary>
        /// 该学生方向填报情况
        /// </summary>
        public virtual List<DirectionStudent> DirectionStudents { get; set; }
        /// <summary>
        /// 该学生被录取情况
        /// </summary>
        public virtual Enrollment Enrollment { get; set; }        
    }
}
