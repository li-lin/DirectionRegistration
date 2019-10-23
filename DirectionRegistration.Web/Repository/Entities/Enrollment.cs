using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Repository.Entities
{
    /// <summary>
    /// 方向录取信息
    /// </summary>
    public class Enrollment
    {
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
    }
}