﻿using System;
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
            DirectionCourse = new List<DirectionCourse>();
            Enrollments = new List<Enrollment>();
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

        public virtual List<DirectionCourse> DirectionCourse { get; set; }

        public virtual List<Enrollment> Enrollments { get; set; }

    }
}
