using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DirectionRegistration.Models
{
    public class CourseViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="不能为空")]
        public string CourseName { get; set; }
    }
}