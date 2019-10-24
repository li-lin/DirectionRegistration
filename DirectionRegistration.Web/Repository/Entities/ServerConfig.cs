using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectionRegistration.Repository.Entities
{
    /// <summary>
    /// 服务器配置数据，只允许有一行数据。
    /// </summary>
    public class ServerConfig
    {
        public int Id { get; set; }
        public DateTime Deadline { get; set; }
        /// <summary>
        /// 录取工作状态，0为未进行，1为已完成。
        /// </summary>
        public int EnrollmentState { get; set; }

    }
}
