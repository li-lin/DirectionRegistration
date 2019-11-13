using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Models
{
    public class RegEnrollDetailViewModel
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Major { get; set; }
        /// <summary>
        /// 录取方向
        /// </summary>
        public string EnrollDirection { get; set; }
        /// <summary>
        /// 录取批次
        /// </summary>
        public int WhichTime { get; set; }
        /// <summary>
        /// 被第几志愿录取
        /// </summary>
        public int EnrolledOrder { get; set; }
        /// <summary>
        /// 成绩在被录取方向中的排名
        /// </summary>
        public int OrderInDirection { get; set; }
        /// <summary>
        /// 考核课程的总成绩
        /// </summary>
        public double TotalScore { get; set; }
        /// <summary>
        /// 志愿填报情况
        /// </summary>
        public List<DirectionInfoViewModel> Selections { get; set; }
        /// <summary>
        /// 录取考查成绩
        /// </summary>
        public List<ScoreInfoViewModel> Scores { get; set; }
    }
}