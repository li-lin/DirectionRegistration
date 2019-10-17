using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectionRegistration.Repository.Entities
{
    // 由于此版本只考虑对应关系，不考虑课程在方向中所占比重，因此作废此类。
    /// <summary>
    /// 方向与考核课程对应关系
    /// </summary>
    public class DirectionCourse
    {
        public int Id { get; set; }
        public virtual Course Course { get; set; }
        public virtual Direction Direction { get; set; }
        /// <summary>
        /// 课程在该方向所在比重
        /// </summary>
        public double Proportion { get; set; }
    }
}
