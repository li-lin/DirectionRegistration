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
    [AdminCheck]
    public class TeacherController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();
        //
        // GET: /Teacher/

        public ActionResult Index()
        {
            TeacherViewModel model = new TeacherViewModel();
            return View(model);
        }

        public PartialViewResult GetTeachers()
        {
            return PartialView("PartialTeacherList", bindTeacherViewModel());
        }

        [HttpPost]
        public ActionResult Add(TeacherViewModel model)
        {
            if (!isExistedTeacher(model.LoginName))
            {
                Teacher d = new Teacher
                {
                    LoginName = model.LoginName,
                    Name = model.Name,
                    IsSuper = model.IsSuper,
                    Password = "123456"
                };
                this.db.Teachers.Add(d);
                int i = this.db.SaveChanges();
                if (i > 0)
                {
                    return PartialView("PartialTeacherList", bindTeacherViewModel());
                }
                else
                {
                    return Json(new { code = 1, data = "添加失败" });
                }
            }
            else
            {
                return Json(new { code = 1, data = "该教师已存在" });
            }
           
        }

        [HttpPost]
        public ActionResult Delete(int Id)
        {
            Teacher d = this.db.Teachers.SingleOrDefault(dd => dd.Id == Id);
            if (d != null)
            {
                this.db.Teachers.Remove(d);
                int i = this.db.SaveChanges();
                if (i == 0)
                {
                    return Json(new { code = 1, data = "删除失败" });
                }
            }
            return PartialView("PartialTeacherList", bindTeacherViewModel());
        }

        [HttpGet]
        public ActionResult Modify(int Id)
        {
            Teacher _teacher = db.Teachers.SingleOrDefault(d => d.Id == Id);
            TeacherViewModel model = new TeacherViewModel();
            if (_teacher != null)
            {
                model.Id = _teacher.Id;
                model.LoginName = _teacher.LoginName;
                model.Name = _teacher.Name;
                model.IsSuper = _teacher.IsSuper;
            }                      

            return PartialView("PartialModifyTeacher", model);
        }

        [HttpPost]
        public ActionResult Modify(TeacherViewModel teacher)
        {
            Teacher _teacher = db.Teachers.SingleOrDefault(d => d.Id == teacher.Id);
            if (_teacher != null)
            {
                _teacher.Name = teacher.Name;
                _teacher.LoginName = teacher.LoginName;
                _teacher.IsSuper = teacher.IsSuper;
                int i = db.SaveChanges();
                return PartialView("PartialTeacherList", bindTeacherViewModel());
            }
            return Json(new { code = 1, data = "修改失败" });
        }

        [HttpPost]
        public ActionResult RePassword(int id)
        {
            Teacher _teacher = db.Teachers.SingleOrDefault(d => d.Id == id);
            if (_teacher != null)
            {                
                _teacher.Password = "123456";
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return Json(new { code = 0, data = "密码重置成功" });
                }
            }
            return Json(new { code = 1, data = "密码重置失败" });
        }

        private List<TeacherViewModel> bindTeacherViewModel()
        {
            string currentAdmin = Session["admin"] as string;

            List<Teacher> ds = this.db.Teachers.ToList();
            List<TeacherViewModel> teachers = new List<TeacherViewModel>();
            foreach(var d in ds)
            {
                teachers.Add(new TeacherViewModel
                {
                    Id = d.Id,
                    LoginName = d.LoginName,
                    Name = d.Name,
                    IsSuper = d.IsSuper,
                    IsSelf = d.LoginName == currentAdmin,
                    DirectionName = d.Direction == null ? "无" : d.Direction.Title
                });
            }
            return teachers;
        }

        private bool isExistedTeacher(string loginName)
        {
            bool b = false;
            int count = db.Teachers.Count(d => d.LoginName == loginName);
            if (count != 0)
            {
                b = true;
            }
            return b;
        }
    }
}
