using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.OleDb;
using System.IO;
using PagedList;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using System.Configuration;

namespace DirectionRegistration.Web.Controllers
{
    public class AdminController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();
        public ActionResult Index(int? page)
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            List<RegistrationViewModel> registrations = new List<RegistrationViewModel>();
            db.Students.ToList().ForEach(s =>
            {
                var reg = new RegistrationViewModel();
                reg.Id = s.Id;
                reg.Number = s.Number;
                reg.Name = s.Name;
                reg.Major = s.Major;
                string fs = getDirectionName(s.FirstSelection);
                reg.FirstSelection = fs ?? "未选择";
                string ss = getDirectionName(s.SecondSelection);
                reg.SecondSelection = ss ?? "未选择";
                string ts = getDirectionName(s.ThirdSelection);
                reg.ThirdSelection = ts ?? "未选择";
                registrations.Add(reg);
            });
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(registrations.ToPagedList(pageNumber, pageSize));
        }

        private string getDirectionName(int id)
        {
            string dName = null;
            var dir = db.Directions.SingleOrDefault(d => d.Id == id);
            if (dir != null)
            {
                dName = dir.Title;
            }
            return dName;
        }

        public ActionResult UploadData()
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadData(IEnumerable<HttpPostedFileBase> files)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    string fileExtentian = file.FileName.Substring(file.FileName.LastIndexOf(".")).ToLower();
                    if (fileExtentian == ".xls" || fileExtentian == ".xlsx")
                    {
                        string newFileName = DateTime.Now.ToString("yyyyMMddhhmmss") + fileExtentian;
                        string path = Server.MapPath("~/Content/UploadFiles/" + newFileName);
                        file.SaveAs(path);

                        int b = importStudentsFromExcel(path);
                        if (b == 1)
                        {
                            return Json(new { code = "101", msg = "学生数据导入成功。" });
                        }
                        else if (b == 2)
                        {
                            return Json(new { code = "102", msg = "学生数据重复，请确认后再导入。" });
                        }
                    }
                }
            }
            return Json(new { code = "100", msg = "学生数据上传或导入失败。" });
        }

        private int importStudentsFromExcel(string path)
        {
            int result = 0;//0：失败
            string connectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=" + path + "; Extended Properties=Excel 8.0;";
            using (OleDbConnection Connection = new OleDbConnection(connectionString))
            {
                DataTable dt = new DataTable();
                Connection.Open();
                using (OleDbCommand command = new OleDbCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "SELECT * FROM [学生名单$]";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                    adapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        Student s = new Student();
                        s.Number = dr["学号"].ToString();
                        s.Name = dr["姓名"].ToString();
                        s.Gender = dr["性别"].ToString();
                        s.Major = dr["专业名称"].ToString();
                        s.Password = "123456";
                        //判断导入学生信息是否与数据库中重复。
                        if (checkStudentExist(s))
                        {
                            return 2;//2：数据重复
                        }
                        this.db.Students.Add(s);
                    }
                    int i = this.db.SaveChanges();
                    if (i > 0) result = 1; //1：成功
                }
            }
            return result;
        }

        private bool checkStudentExist(Student student)
        {
            bool r = false;
            int ss = this.db.Students.Count(s=>s.Number == student.Number);
            if (ss != 0) r = true;
            return r;
        }
         
        //下载学生志愿填报情况（Excel）
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
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                using (OleDbCommand command = new OleDbCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "CREATE TABLE [学生名单$](学号 Char(100), 姓名 char(100), 专业 char(250), 第一志愿 char(200), 第二志愿 char(200), 第三志愿 char(200))";
                    command.ExecuteNonQuery();
                }

               List<Student> students = this.db.Students.ToList();
               foreach (Student s in students)
               {
                   OleDbCommand cmdInsert = new OleDbCommand();
                   cmdInsert.Connection = connection;
                   cmdInsert.CommandText = "INSERT INTO [学生名单$](学号,姓名,专业,第一志愿,第二志愿,第三志愿) VALUES(@number,@name,@major,@firstSel,@secondSel,@thirdSel)";
                   cmdInsert.Parameters.Add(new OleDbParameter("@number", s.Number));
                   cmdInsert.Parameters.Add(new OleDbParameter("@name", s.Name));
                   cmdInsert.Parameters.Add(new OleDbParameter("@major", s.Major));
                   string fs = getDirectionName(s.FirstSelection);
                   string ss = getDirectionName(s.SecondSelection);
                   string ts = getDirectionName(s.ThirdSelection);
                   cmdInsert.Parameters.Add(new OleDbParameter("@firstSel", (fs ?? "未选择")));
                   cmdInsert.Parameters.Add(new OleDbParameter("@secondSel", (ss ?? "未选择")));
                   cmdInsert.Parameters.Add(new OleDbParameter("@thirdSel", (ts ?? "未选择")));
                   cmdInsert.ExecuteNonQuery();
               }
                connection.Close();
            }
            return File(path, "application/vnd.ms-excel", path.Substring(path.LastIndexOf("\\")));
        }

        //设置截止日期
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
        public ActionResult Setting(string deadline)
        {
            //string currentAdmin = Session["admin"] as string;
            //if (string.IsNullOrEmpty(currentAdmin))
            //{
            //    return RedirectToAction("Login", "Home");
            //}

            DateTime deadlineDt = DateTime.Now;
            bool b = DateTime.TryParse(deadline, out deadlineDt);
            var dl = db.ServerConfigurations.FirstOrDefault();
            if (b && dl != null)
            {
                dl.Deadline = deadlineDt;
                db.SaveChanges();

                return Json(new { code = 0, data = "设置成功" });
            }
            return Json(new { code = 1, data = "设置失败" });
        }

        public ActionResult DirectionManage()
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            return PartialView();
        }

        public PartialViewResult GetDirections()
        {
            return PartialView(bindDirectionViewModel());
        }

        [HttpPost]
        public ActionResult DirectionAdd(DirectionViewModel model)
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            Direction d = new Direction
            {
                Title = model.Title
            };
            this.db.Directions.Add(d);
            int i = this.db.SaveChanges();
            if (i > 0)
            {
                return PartialView("GetDirections", bindDirectionViewModel());
            }
            else
            {
                return Json("系统错误");
            }
        }
        
        public ActionResult DirectionDel(int Id)
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            var d = this.db.Directions.SingleOrDefault(dd => dd.Id == Id);
            if (d != null)
            {
                this.db.Directions.Remove(d);
                int i = this.db.SaveChanges();
                if (i > 0)
                {
                    return PartialView("GetDirections", bindDirectionViewModel());
                }
            }
            return Json("系统错误", JsonRequestBehavior.AllowGet);
        }

        private List<DirectionViewModel> bindDirectionViewModel()
        {
            List<Direction> ds = this.db.Directions.ToList();
            List<DirectionViewModel> m = new List<DirectionViewModel>();
            ds.ForEach(dd =>
            {
                m.Add(new DirectionViewModel
                {
                    Id = dd.Id,
                    Title = dd.Title
                });
            });
            return m;
        }

        public ActionResult AdminManage()
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            return PartialView();
        }

        public PartialViewResult GetAdmins()
        {
            return PartialView(bindAdminViewModel());
        }

        [HttpPost]
        public ActionResult AdminAdd(Teacher model)
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            Teacher d = new Teacher
            {
                LoginName = model.LoginName,
                Password = "12345678"
            };
            this.db.Teachers.Add(d);
            int i = this.db.SaveChanges();
            if (i > 0)
            {
                return PartialView("GetAdmins", bindAdminViewModel());
            }
            else
            {
                return Json("系统错误");
            }
        }

        public ActionResult AdminDel(int Id)
        {
            string currentAdmin = Session["admin"] as string;
            if (string.IsNullOrEmpty(currentAdmin))
            {
                return RedirectToAction("Login", "Home");
            }

            Teacher d = this.db.Teachers.SingleOrDefault(dd => dd.Id == Id);
            if (d != null)
            {
                this.db.Teachers.Remove(d);
                int i = this.db.SaveChanges();
                if (i > 0)
                {
                    return PartialView("GetAdmins", bindAdminViewModel());
                }
            }
            return Json("系统错误", JsonRequestBehavior.AllowGet);
        }

        private List<Teacher> bindAdminViewModel()
        {
            List<Teacher> ds = this.db.Teachers.ToList();           
            return ds;
        }
    }
}