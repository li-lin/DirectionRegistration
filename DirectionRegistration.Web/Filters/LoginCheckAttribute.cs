using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DirectionRegistration.Web.Filters
{
    public class LoginCheckAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            string currentStu = filterContext.HttpContext.Session["currStu"] as string;
            string currentAdmin = filterContext.HttpContext.Session["admin"] as string;
            if (string.IsNullOrEmpty(currentStu)||string.IsNullOrEmpty(currentAdmin))
            {
                filterContext.HttpContext.Response.Redirect("~/Home/Login", true);
            }
        }
    }
}