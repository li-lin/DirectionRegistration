using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DirectionRegistration.Models
{
    public class StudentInfoViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="必填")]
        public string Number { get; set; }
        [Required(ErrorMessage = "必填")]
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Major { get; set; }
    }
}