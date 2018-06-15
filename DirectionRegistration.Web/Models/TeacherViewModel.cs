using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DirectionRegistration.Models
{
    public class TeacherViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="必填")]
        public string LoginName { get; set; }
        [Required(ErrorMessage = "必填")]
        public string Name { get; set; }
        public bool IsSuper { get; set; }
        public string DirectionName { get; set; }
        public bool IsSelf { get; set; }
    }
}