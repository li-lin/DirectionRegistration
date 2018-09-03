using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Models
{
    public class StudentScoreViewModel
    {
        public StudentScoreViewModel()
        {
            Scores = new List<ScoreInfoViewModel>();
        }
        public int StudentId { get; set; }
        public string StudentNumber { get; set; }
        public string StudentMajor { get; set; }
        public string StudentGender { get; set; }
        public string StudentName { get; set; }
        public List<ScoreInfoViewModel> Scores { get; set; } 
        public float Total { get; set; }
    }
}