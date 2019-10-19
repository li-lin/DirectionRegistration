using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DirectionRegistration.Repository.Entities
{
    /// <summary>
    /// 方向与考核课程对应关系
    /// </summary>
    public class DirectionCourse
    {
        public int Id { get; set; }
        public virtual Course Course { get; set; }
        public virtual Direction Direction { get; set; }
        /// <summary>
        /// 课程在该方向所在比重，默认1.0，即成绩原值。
        /// </summary>
        [DefaultValue(1.0)]
        public double Proportion { get; set; }
    }
}
