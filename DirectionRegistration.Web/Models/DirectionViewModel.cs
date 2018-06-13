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
        [Required(ErrorMessage="请输入方向名称")]
        public string Title { get; set; }
    }
}