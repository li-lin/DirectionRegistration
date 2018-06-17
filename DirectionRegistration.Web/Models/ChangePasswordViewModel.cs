using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DirectionRegistration.Models
{
    public class ChangePasswordViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage="请输入原密码")]
        //[Remote("ValidateOldPassword","Home")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage="请输入密码")]
        [StringLength(16, MinimumLength = 6, ErrorMessage = "密码至少6位")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请确认密码")]
        [Compare("Password", ErrorMessage = "两次密码不一致")]
        public string RePassword { get; set; }
    }
}