using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Models
{
    public class GenerationResultModel
    {
        public GenerationResultModel()
        {
            Selections = new List<DirectionInfoViewModel>();
        }
        public StudentScoreViewModel StudentScore { get; set; }
        public List<DirectionInfoViewModel> Selections { get; set; }
        public string DirectionName { get; set; }
        public int DirectionOrder { get; set; }
    }
}