using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;

namespace DirectionRegistration.Web.Controllers
{
    public class StudentController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();
        //
        // GET: /Stuent/

        public ActionResult Index()
        {
            return View();
        }

    }
}
