using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;

namespace DirectionRegistration.Web.Filters
{
    public class CustHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            Exception ex = filterContext.Exception;
            //写入异常日志  
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToString());
            sb.AppendLine(ex.ToString());
            File.AppendAllText(filterContext.HttpContext.Server.MapPath("~/logs/exps.txt"), sb.ToString());
            filterContext.HttpContext.Session.Clear();
            //跳转到错误页面  
            filterContext.HttpContext.Response.Redirect("/Error.html");
        }
    }
}