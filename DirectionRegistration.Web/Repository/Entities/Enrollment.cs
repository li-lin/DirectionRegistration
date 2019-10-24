using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectionRegistration.Repository.Entities
{
    /// <summary>
    /// 方向录取信息
    /// </summary>
    public class Enrollment
    {
        [Key]
        [ForeignKey("Student")]
        public int EnrollmentId { get; set; }
        public virtual Direction Direction { get; set; }
        public virtual Student Student { get; set; }
        /// <summary>
        /// 学生在该方向中录取成绩的排名
        /// </summary>
        public int ScoreOrder { get; set; }
        /// <summary>
        /// 学生是第几批次被录取
        /// </summary>
        public int WhichTime { get; set; }

        public virtual List<Course> EnrollCourses { get; set; }
    }
}