using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DirectionRegistration.Models
{
    public class DirectionViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage="必填")]
        public string Title { get; set; }

        //public int TeacherId { get; set; }
        //[Required(ErrorMessage = "必填")]
        //public string TeacherName { get; set; }

        [Required(ErrorMessage = "必填")]
        public int Max { get; set; }
    }
}