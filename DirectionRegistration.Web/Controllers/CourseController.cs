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
    public class CourseController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();
        //
        // GET: /Course/

        public ActionResult Index()
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            CourseViewModel course = new CourseViewModel();
            return View(course);
        }

        [HttpPost]
        public ActionResult Add(CourseViewModel course)
        {
            Course _course = new Course
            {
                CourseName = course.CourseName
            };

            if (isExistedCourse(_course.CourseName))
            {
                db.Courses.Add(_course);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return PartialView("PartialCourseList", getCoursesViewModel());
                }
            }
            return Json(new { code = 1, data = "添加失败" });
        }

        [HttpPost]
        public ActionResult Modify(CourseViewModel course)
        {
            Course _course = db.Courses.SingleOrDefault(c => c.Id == course.Id);
            if (_course != null)
            {
                _course.CourseName = course.CourseName;
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return PartialView("PartialCourseList", getCoursesViewModel());
                }
            }
            return Json(new { code = 1, data = "修改失败" });
        }

        [HttpPost]
        public ActionResult Delete(int courseId)
        {
            Course _course = db.Courses.SingleOrDefault(c => c.Id == courseId);
            if (_course != null)
            {
                db.Courses.Remove(_course);
                int i = db.SaveChanges();
                if (i == 0)
                {
                    return Json(new { code = 1, data = "删除失败" });
                }
            }
            return PartialView("PartialCourseList", getCoursesViewModel());
        }

        [HttpGet]
        public ActionResult GetCourses()
        {
            return PartialView("PartialCourseList", getCoursesViewModel());
        }

        private List<CourseViewModel> getCoursesViewModel()
        {
            List<CourseViewModel> courses = new List<CourseViewModel>();
            db.Courses.ToList().ForEach(item =>
            {
                courses.Add(new CourseViewModel
                {
                    Id = item.Id,
                    CourseName = item.CourseName
                });
            });
            return courses;
        }

        private bool isExistedCourse(string courseName)
        {
            bool b = false;
            int count = db.Courses.Count(c => c.CourseName == courseName);
            if (count != 0)
            {
                b = true;
            }
            return b;
        }

    }
}
