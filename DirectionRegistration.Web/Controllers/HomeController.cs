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

namespace DirectionRegistration.Controllers
{
    public class HomeController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();

        public ActionResult Index()
        {
            string currentStu = Session["currStu"] as string;
            if (string.IsNullOrEmpty(currentStu))
            {
                return RedirectToAction("Login");
            }

            Student stu = db.Students.SingleOrDefault(s => s.Number == currentStu);
            IndexViewModel model = new IndexViewModel();
            model.Id = stu.Id;
            model.Name = stu.Name;
            model.Number = stu.Number;
            model.FirstId = stu.FirstSelection.ToString();
            model.SecondId = stu.SecondSelection.ToString();
            model.ThirdId = stu.ThirdSelection.ToString();

            ViewBag.Directions = db.Directions.ToList();
            ViewBag.IsOverTime = isTimeOver();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(IndexViewModel model)
        {
            var stu = db.Students.SingleOrDefault(s => s.Id == model.Id);
            if (stu != null)
            {
                stu.FirstSelection = int.Parse(model.FirstId);
                stu.SecondSelection = int.Parse(model.SecondId);
                stu.ThirdSelection = int.Parse(model.ThirdId);
                db.Entry(stu).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                //StringBuilder sb = new StringBuilder("你的选择是：<br/><ol>");
                //if (stu.FirstSelection != null)
                //{
                //    sb.AppendFormat("<li>第一志愿:{0}<li>", stu.FirstSelection.Title);
                //}
                //if (stu.SecondSelection != null)
                //{
                //    sb.AppendFormat("<li>第二志愿:{0}<li>", stu.SecondSelection.Title);
                //}
                //if (stu.ThirdSelection != null)
                //{
                //    sb.AppendFormat("<li>第三志愿:{0}<li>", stu.ThirdSelection.Title);
                //}
                //sb.Append("</ol>");
                //ViewBag.Info = sb.ToString();
                ViewBag.Info = "保存成功";
            }
            model.Id = stu.Id;
            model.Name = stu.Name;
            model.Number = stu.Number;
            model.FirstId = stu.FirstSelection.ToString();
            model.SecondId = stu.SecondSelection.ToString();
            model.ThirdId = stu.ThirdSelection.ToString();
            ViewBag.Directions = db.Directions.ToList();
            ViewBag.IsOverTime = isTimeOver();
            return View(model);
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

        public ActionResult ChangePassword()
        {
            string currentStu = Session["currStu"] as string;
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentStu) && string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login");
            }
            else if (string.IsNullOrEmpty(currentStu) == false)
            {
                var stu = db.Students.SingleOrDefault(s => s.Number == currentStu);
                if (stu == null)
                {
                    return RedirectToAction("Login");
                }

                var model = new ChangePasswordViewModel()
                {
                    Id = stu.Id
                };
                return View(model);
            }
            else
            {
                var admin = db.Teachers.SingleOrDefault(a => a.LoginName == currentAdmin);
                if (admin == null)
                {
                    return RedirectToAction("Login");
                }

                var model = new ChangePasswordViewModel()
                {
                    Id = admin.Id
                };
                return View(model);
            }            
        }

        [HttpPost]
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
                        admin.Password = model.Password;
                        db.Entry(admin).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                        return Json(new { code = 0, data = "密码修改成功，请重新登录。" });
                    }
                }
            }
            return Json(new { code = 1, data = "密码修改失败" });
        }

        public JsonResult ValidateOldPassword(string OldPassword)
        {
            string currentStu = Session["currStu"] as string;
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentStu) == false)
            {
                var stu = db.Students.SingleOrDefault(s => s.Number == currentStu);
                if (stu == null)
                {
                    return Json("密码修改：系统错误，请联系管理员。", JsonRequestBehavior.AllowGet);
                }

                if (stu.Password != OldPassword)
                {
                    return Json("原密码不正确", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var admin = db.Teachers.SingleOrDefault(a => a.LoginName == currentAdmin);
                if (admin == null)
                {
                    return Json("密码修改：系统错误，请联系管理员。", JsonRequestBehavior.AllowGet);
                }

                if (admin.Password != OldPassword)
                {
                    return Json("原密码不正确", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}
