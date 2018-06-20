using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Configuration;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using DirectionRegistration.Web.Filters;

namespace DirectionRegistration.Controllers
{
    public class HomeController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();

        [LoginCheck]
        public ActionResult Index()
        {
            string currentStu = Session["currStu"] as string;
            Student stu = db.Students.SingleOrDefault(s => s.Number == currentStu);

            IndexViewModel model = new IndexViewModel();
            model.Id = stu.Id;
            model.Name = stu.Name;
            model.Number = stu.Number;

            if (stu.DirectionStudents.Count == 0)
            {
                int order = 0;
                db.Directions.ToList().ForEach(d =>
                {
                    model.Directions.Add(new DirectionInfoViewModel
                    {
                        Id = d.Id,
                        DirectionName = d.Title,
                        Order = ++order
                    });
                });
            }
            else
            {
                var orderedList = db.DirectionStudents
                    .Where(d => d.Student.Id == stu.Id)
                    .OrderBy(d => d.Order)
                    .ToList();
                foreach(var ds in orderedList)
                {
                    model.Directions.Add(new DirectionInfoViewModel
                    {
                        Id = ds.Direction.Id,
                        DirectionName = ds.Direction.Title,
                        Order = ds.Order
                    });
                }
            }
                      
            ViewBag.IsOverTime = isTimeOver();
            return View(model);
        }

        [HttpPost]
        [LoginCheck]
        public ActionResult Save(DirectionSaveViewModel model)
        {
            if (isTimeOver()) return Json(new { code = 1, data = "填报已截止" });

            Student stu = db.Students.SingleOrDefault(s => s.Id == model.Sid);
            if (stu != null)
            {
                int tag = 0;
                if (stu.DirectionStudents.Count == 0)
                {                    
                    foreach(var item in model.Orders)
                    {
                        var direction = db.Directions.SingleOrDefault(d => d.Id == item.Did);
                        if (direction != null)
                        {
                            var dd = new DirectionStudent();
                            dd.Order = item.Order;
                            dd.Direction = direction;

                            stu.DirectionStudents.Add(dd);
                            tag++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    //修改填报信息                    
                    foreach (var item in model.Orders)
                    {
                        var selectedDirection = db.DirectionStudents.SingleOrDefault(d => d.Student.Id == stu.Id && d.Direction.Id == item.Did);
                        if (selectedDirection != null)
                        {
                            selectedDirection.Order = item.Order;
                            tag++;
                        }
                    }
                }

                if (tag == model.Orders.Count)
                {
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return Json(new { code = 0, data = "填报成功" });
                    }
                }
            }
            return Json(new { code = 1, data = "保存失败" });
        }

        //判断选填志愿是否结束。
        private bool isTimeOver()
        {
            bool b = false;
            var config = db.ServerConfigurations.FirstOrDefault();
            if (config != null)
            {
                DateTime deadline = config.Deadline;
                if (DateTime.Now >= deadline)
                {
                    b = true;
                }
            }
            return b;
        }
       
        public ActionResult Login()
        {
            var model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (model.LoginName.ToUpper().StartsWith("JC"))
            {
                //管理员登录
                Teacher admin = db.Teachers.SingleOrDefault(a => a.LoginName == model.LoginName.ToUpper() && a.Password == model.Password);
                if (admin == null)
                {
                    ModelState.AddModelError("LoginName", "用户名或密码错误");
                    return View();
                }
                else
                {
                    Session["admin"] = admin.LoginName;
                    return RedirectToAction("Index", "Admin");
                }
            }
            else
            {
                //学生登录
                Student stu = db.Students.SingleOrDefault(s => s.Number == model.LoginName && s.Password == model.Password);
                if (stu == null)
                {
                    ModelState.AddModelError("LoginName", "用户名或密码错误");
                    return View();
                }
                else
                {
                    Session["currStu"] = stu.Number;
                    return RedirectToAction("Index");
                }
            }
        }

        public ActionResult Quit()
        {
            Session["currStu"] = null;
            Session["admin"] = null;
            return RedirectToAction("Login");
        }

        [LoginCheck]
        public ActionResult ChangePassword()
        {
            string currentStu = Session["currStu"] as string;
            string currentAdmin = Session["admin"] as string;
            var model = new ChangePasswordViewModel();
            if (string.IsNullOrEmpty(currentStu) == false)
            {
                var stu = db.Students.SingleOrDefault(s => s.Number == currentStu);
                if (stu == null)
                {
                    return RedirectToAction("Login");
                }
                model.Id = stu.Id;
            }
            else
            {
                var admin = db.Teachers.SingleOrDefault(a => a.LoginName == currentAdmin);
                if (admin == null)
                {
                    return RedirectToAction("Login");
                }
                model.Id = admin.Id;
            }

            return View(model);
        }

        [HttpPost]
        [LoginCheck]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string currentStu = Session["currStu"] as string;
                string currentAdmin = Session["admin"] as string;
                if (string.IsNullOrEmpty(currentStu) == false)
                {
                    var stu = db.Students.SingleOrDefault(s => s.Id == model.Id);
                    
                    if (stu != null)
                    {
                        if (stu.Password != model.OldPassword)
                        {
                            return Json(new { code = 1, data = "原密码不正确" });
                        }

                        stu.Password = model.Password;
                        db.Entry(stu).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { code = 0, data = "密码修改成功，请重新登录。" });
                    }
                }
                if (string.IsNullOrEmpty(currentAdmin) == false)
                {
                    var admin = db.Teachers.SingleOrDefault(a => a.Id == model.Id);
                    if (admin != null)
                    {
                        if (admin.Password != model.OldPassword)
                        {
                            return Json(new { code = 1, data = "原密码不正确" });
                        }

                        admin.Password = model.Password;
                        db.Entry(admin).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { code = 0, data = "密码修改成功，请重新登录。" });
                    }
                }
            }
            return Json(new { code = 1, data = "密码修改失败" });
        }        
    }
}
