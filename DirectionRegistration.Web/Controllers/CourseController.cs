using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using DirectionRegistration.Web.Filters;

namespace DirectionRegistration.Web.Controllers
{
    [SuperCheck]
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
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }            

            if (!isExistedCourse(course.CourseName))
            {
                Course _course = new Course
                {
                    CourseName = course.CourseName
                };
                db.Courses.Add(_course);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return PartialView("PartialCourseList", getCoursesViewModel());
                }
            }
            return Json(new { code = 1, data = "添加失败" });
        }

        public ActionResult Modify(int Id)
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            Course _course = db.Courses.SingleOrDefault(c => c.Id == Id);
            CourseViewModel model = new CourseViewModel();
            if (_course != null)
            {
                model.Id = _course.Id;
                model.CourseName = _course.CourseName;
            }
            return PartialView("PartialModifyCourse", model);
        }

        [HttpPost]
        public ActionResult Modify(CourseViewModel course)
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            Course _course = db.Courses.SingleOrDefault(c => c.Id == course.Id);
            if (_course != null)
            {
                _course.CourseName = course.CourseName;
                int i = db.SaveChanges();
                return PartialView("PartialCourseList", getCoursesViewModel());
            }
            return Json(new { code = 1, data = "修改失败" });
        }

        [HttpPost]
        public ActionResult Delete(int courseId)
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

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
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

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
