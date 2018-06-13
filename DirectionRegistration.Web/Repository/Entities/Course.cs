using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectionRegistration.Repository.Entities
{
    public class Course
    {
        public Course()
        {
            Scores = new List<Score>();
        }
        /// <summary>
        /// 课程ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 课程名
        /// </summary>
        public string CourseName { get; set; }

        public virtual List<Score> Scores { get; set; }

    }
}
