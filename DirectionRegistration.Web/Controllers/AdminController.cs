using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using PagedList;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using DirectionRegistration.Web.Filters;

namespace DirectionRegistration.Web.Controllers
{
    public class AdminController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();

        [LoginCheck]
        public ActionResult Index(int? page)
        {
            string currentAdmin = Session["admin"] as string;           

            Teacher teacher = db.Teachers.SingleOrDefault(t => t.LoginName == currentAdmin);
            if (teacher == null)
            {
                return RedirectToAction("Quit", "Home");
            }
            
            List<RegistrationViewModel> registrations = new List<RegistrationViewModel>();
            var directionsByStudent = db.DirectionStudents.GroupBy(ds => ds.Student).ToList();

            foreach (var group in directionsByStudent)
            {
                var reg = new RegistrationViewModel();
                reg.Id = group.Key.Id;
                reg.Number = group.Key.Number;
                reg.Name = group.Key.Name;
                reg.Gender = group.Key.Gender;
                reg.Major = group.Key.Major;

                reg.Selections = group.OrderBy(g => g.Order).Select(ds => new DirectionInfoViewModel
                {
                    Id = ds.Direction.Id,
                    DirectionName = ds.Direction.Title,
                    Order = ds.Order
                }).Take(5).ToList();

                if (!teacher.IsSuper)
                {
                    int dirId = teacher.Direction.Id;
                    if (reg.Selections[0].Id == dirId || reg.Selections[1].Id == dirId)
                        registrations.Add(reg);
                }
                else
                {
                    registrations.Add(reg);
                }
            }

            ViewBag.All = db.Students.Count();
            ViewBag.Now = registrations.Count;
            if (!teacher.IsSuper)
            {
                ViewBag.DirName = teacher.Direction.Title;
            }
            else
            {
                ViewBag.DirName = "全部";
            }
            ViewBag.TeacherInfo = teacher.LoginName + " | " + teacher.Name;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(registrations.ToPagedList(pageNumber, pageSize));
        }

        [LoginCheck]
        public ActionResult Details(int id)
        {
            RegistrationViewModel selection = new RegistrationViewModel();
            var student = db.Students.SingleOrDefault(s => s.Id == id);
            if (student != null)
            {
                var directionsByStudent = db.DirectionStudents.Where(ds => ds.Student.Id == id).OrderBy(ds => ds.Order).ToList();
                selection.Id = student.Id;
                selection.Number = student.Number;
                selection.Name = student.Name;
                selection.Gender = student.Gender;
                selection.Major = student.Major;
                foreach(var dir in directionsByStudent)
                {
                    selection.Selections.Add(new DirectionInfoViewModel
                    {
                        Id = dir.Direction.Id,
                        DirectionName = dir.Direction.Title,
                        Order = dir.Order
                    });
                }

                return PartialView("PartialSelectionDetails", selection);
            }
            //return Json(new { code = 1, data = "系统错误" });     
            return Content("系统错误");
        }

        //下载学生志愿填报情况（Excel）
        [AdminCheck]
        public ActionResult DownloadData()
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            string tempPath = Server.MapPath("~/Content/DownloadFiles/temp.xls");
            string path = Server.MapPath("~/Content/DownloadFiles/方向志愿-" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".xls");
            System.IO.File.Copy(tempPath, path);

            string connectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=" + path + "; Extended Properties=Excel 8.0;";
            StringBuilder stringBuilder1 = new StringBuilder("CREATE TABLE [学生名单$](学号 Char(100), 姓名 char(100),性别 char(20), 专业 char(160)");
            StringBuilder stringBuilder2 = new StringBuilder("INSERT INTO[学生名单$](学号, 姓名, 性别, 专业");
            StringBuilder stringBuilder3 = new StringBuilder(" VALUES(@number,@name,@gender,@major");
            int directionCount = db.Directions.Count();
            for (int i = 0; i < directionCount; i++)
            {
                stringBuilder1.Append($", 志愿{i + 1} char(160)");
                stringBuilder2.Append($", 志愿{i + 1}");
                stringBuilder3.Append($", @want{i + 1}");
            }
            stringBuilder1.Append(")");
            stringBuilder2.Append(")");
            stringBuilder3.Append(")");

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                using (OleDbCommand command = new OleDbCommand())
                {
                    command.Connection = connection;
                    command.CommandText = stringBuilder1.ToString();
                    command.ExecuteNonQuery();
                }

                List<RegistrationViewModel> registrations = new List<RegistrationViewModel>();
                var directionsByStudent = db.DirectionStudents.GroupBy(ds => ds.Student).ToList();

                foreach (var group in directionsByStudent)
                {
                    var reg = new RegistrationViewModel();
                    reg.Id = group.Key.Id;
                    reg.Number = group.Key.Number;
                    reg.Name = group.Key.Name;
                    reg.Gender = group.Key.Gender;
                    reg.Major = group.Key.Major;

                    reg.Selections = group.OrderBy(g => g.Order).Select(ds => new DirectionInfoViewModel
                    {
                        Id = ds.Direction.Id,
                        DirectionName = ds.Direction.Title,
                        Order = ds.Order
                    }).ToList();

                    registrations.Add(reg);
                }

                string commandText = stringBuilder2.ToString() + stringBuilder3.ToString();
                foreach (var s in registrations)
                {
                    OleDbCommand cmdInsert = new OleDbCommand();
                    cmdInsert.Connection = connection;
                    cmdInsert.CommandText = commandText;
                    cmdInsert.Parameters.Add(new OleDbParameter("@number", s.Number));
                    cmdInsert.Parameters.Add(new OleDbParameter("@name", s.Name));
                    cmdInsert.Parameters.Add(new OleDbParameter("@gender", s.Gender));
                    cmdInsert.Parameters.Add(new OleDbParameter("@major", s.Major));
                    foreach (var d in s.Selections)
                    {
                        cmdInsert.Parameters.Add(new OleDbParameter("@want" + d.Order.ToString(), d.DirectionName));
                    }
                    cmdInsert.ExecuteNonQuery();
                }
                connection.Close();
            }
            return File(path, "application/vnd.ms-excel", path.Substring(path.LastIndexOf("\\")));
        }

        //设置截止日期
        [AdminCheck]
        public ActionResult Setting()
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            var config = db.ServerConfigurations.FirstOrDefault();
            string deadlineStr = null;
            if (config != null)
            {
                DateTime deadline = config.Deadline;
                deadlineStr = $"{deadline.Year}-{deadline.Month}-{deadline.Day}";
            }
            else
            {
                deadlineStr = "0000-00-00";
            }
            return PartialView("Setting", deadlineStr);
        }

        [HttpPost]
        [AdminCheck]
        public ActionResult Setting(string deadline)
        {
            DateTime deadlineDt = DateTime.Now;
            bool b = DateTime.TryParse(deadline, out deadlineDt);
            var dl = db.ServerConfigurations.FirstOrDefault();
            if (b && dl != null)
            {
                dl.Deadline = new DateTime(deadlineDt.Year, deadlineDt.Month, deadlineDt.Day, 23, 59, 59);
                db.SaveChanges();

                return Json(new { code = 0, data = "设置成功" });
            }
            return Json(new { code = 1, data = "设置失败" });
        }
        
    }
}