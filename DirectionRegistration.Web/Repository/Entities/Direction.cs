using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace DirectionRegistration.Repository.Entities
{
    public class Direction
    {
        public Direction()
        {
            DirectionStudents = new List<DirectionStudent>();
            DirectionCourses = new List<DirectionCourse>();
            Enrollments = new List<Enrollment>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Max { get; set; }
        public int? TeacherId { get; set; }
        /// <summary>
        /// 该方向学生填报情况
        /// </summary>
        public virtual List<DirectionStudent> DirectionStudents { get; set; }
        /// <summary>
        /// 该方向考核课程
        /// </summary>
        public virtual List<DirectionCourse> DirectionCourses { get; set; }
        /// <summary>
        /// 该方向录取情况
        /// </summary>
        public virtual List<Enrollment> Enrollments { get; set; }
    }
}
