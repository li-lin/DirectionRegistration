using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DirectionRegistration.Models
{
    public class LoginViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage="登录名不能为空")]
        public string LoginName { get; set; }
        [Required(ErrorMessage="密码不能为空")]
        public string Password { get; set; }
    }
}