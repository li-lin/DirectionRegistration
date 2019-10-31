using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

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
        /// 学生是第几批次被录取，0为提前批，1为第一批（第一志愿），2为第二批（第二志愿及以后的平行志愿）
        /// </summary>
        //[DefaultValue(-1)]
        public int WhichTime { get; set; }

        public virtual List<Course> EnrollCourses { get; set; }
    }
}