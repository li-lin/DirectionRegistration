﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DirectionRegistration.Web.Filters
{
    public class LoginCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string currentStu = filterContext.HttpContext.Session["currStu"] as string;
            string currentAdmin = filterContext.HttpContext.Session["admin"] as string;
            if (string.IsNullOrEmpty(currentStu) && string.IsNullOrEmpty(currentAdmin))
            {
                //HttpContext.Current.Response.Write("<script>window.parent.location.href='/Home/Login'</script>");
                //HttpContext.Current.Response.End();
                //return;
                filterContext.Result = new RedirectResult("~/Home/Quit");
            }
        }
    }
}