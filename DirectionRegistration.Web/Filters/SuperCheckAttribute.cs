using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;

namespace DirectionRegistration.Web.Filters
{
    public class SuperCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool b = false;
            string currentAdmin = filterContext.HttpContext.Session["admin"] as string;
            using (var db = new RegistrationDbContext())
            {
                var teacher = db.Teachers.SingleOrDefault(t => t.LoginName == currentAdmin);
                if (teacher != null)
                {
                    b = teacher.IsSuper;
                }
            }

            if (!b)
            {
                filterContext.Result = new RedirectResult("~/Home/Quit");
            }
        }
    }
}