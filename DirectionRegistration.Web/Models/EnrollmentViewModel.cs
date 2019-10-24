using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Models
{
    public class EnrollmentViewModel
    {
        public EnrollmentViewModel()
        {
            Scores = new List<ScoreInfoViewModel>();
        }
        public int EnrollmentId { get; set; }
        public StudentInfoViewModel StudentInfo { get; set; }
        public string DirectionName { get; set; }
        /// <summary>
        /// 录取志愿是第几志愿
        /// </summary>
        public int DirectionOrder { get; set; }
        /// <summary>
        /// 录取批次
        /// </summary>
        public int EnrollTime { get; set; }
        public int ScoreOrder { get; set; }
        public double ScoreTotal { get; set; }
        public List<ScoreInfoViewModel> Scores { get; set; }
    }
}