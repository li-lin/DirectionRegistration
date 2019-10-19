using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DirectionRegistration.Models
{
    public class DirectionViewModel
    {
        public DirectionViewModel()
        {
            Courses = new List<DirectionCoursesViewModel>();
        }
        public int Id { get; set; }
        [Required(ErrorMessage="必填")]
        public string Title { get; set; }        
        [Required(ErrorMessage = "必填")]
        public int Max { get; set; }

        public List<DirectionCoursesViewModel> Courses { get; set; }   
    }

    public class DirectionCoursesViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public double Proportion { get; set; }
    }
}