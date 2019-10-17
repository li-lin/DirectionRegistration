using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.OleDb;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using DirectionRegistration.Web.Filters;

namespace DirectionRegistration.Web.Controllers
{
    [SuperCheck]
    public class StudentController : Controller
    {
        private RegistrationDbContext db = new RegistrationDbContext();
        //
        // GET: /Stuent/

        public ActionResult Index()
        {
            StudentInfoViewModel model = new StudentInfoViewModel();
            List<SelectListItem> majorList = new List<SelectListItem>();
            foreach(string s in getMajors())
            {
                majorList.Add(new SelectListItem
                {
                    Selected = true,
                    Text = s,
                    Value = s
                });
            }
            ViewBag.Majors = majorList;

            var pageInfo = getPageInfo(0);
            ViewBag.PageNumber = pageInfo.PageNumber;
            ViewBag.PageCount = pageInfo.PageCount;

            return View(model);
        }

        [HttpPost]
        public ActionResult Add(StudentInfoViewModel model)
        {
            if (!checkStudentExist(model.Number))
            {
                Student student = new Student
                {
                    Number = model.Number,
                    Name = model.Name,
                    Gender = model.Gender,
                    Major = model.Major,
                    Password = "123456"
                };
                db.Students.Add(student);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return PartialView("PartialStudentList", bindStudentsViewModel(getPageInfo(0)));
                }
                else
                {
                    return Json(new { code = 1, data = "添加失败" }); 
                }
            }
            else
            {
                return Json(new { code = 1, data = "该学生已存在" });
            }
        }

        [HttpPost]
        public ActionResult Search(string number)
        {
            List<StudentInfoViewModel> data = new List<StudentInfoViewModel>();
            if (!string.IsNullOrEmpty(number))
            {
                var students = db.Students.Where(item => item.Number == number).ToList();
                foreach (var s in students)
                {
                    data.Add(new StudentInfoViewModel
                    {
                        Id = s.Id,
                        Number = s.Number,
                        Name = s.Name,
                        Gender = s.Gender,
                        Major = s.Major
                    });
                }
            }
            return PartialView("PartialStudentList", data); 
        }

        public ActionResult GetStudents(PagedStudentsViewModel model)
        {
            List<StudentInfoViewModel> data = null;
            data = bindStudentsViewModel(getPageInfo(model.PageNumber));
            return PartialView("PartialStudentList", data);
        }

        [HttpPost]
        public ActionResult Delete(int Id)
        {
            Student d = db.Students.SingleOrDefault(dd => dd.Id == Id);
            if (d != null)
            {
                var selections = db.DirectionStudents.Where(ds => ds.Student.Id == Id);
                foreach(var selection in selections)
                {
                    db.DirectionStudents.Remove(selection);
                }

                db.Students.Remove(d);
                int i = this.db.SaveChanges();
                if (i == 0)
                {
                    return Json(new { code = 1, data = "删除失败" });
                }
            }
            return PartialView("PartialStudentList", bindStudentsViewModel(getPageInfo(0)));
        }

        [HttpGet]
        public ActionResult Modify(int Id)
        {
            Student _student = db.Students.SingleOrDefault(d => d.Id == Id);
            StudentInfoViewModel model = new StudentInfoViewModel();
            if (_student != null)
            { 
                model.Id = _student.Id;
                model.Number = _student.Number;
                model.Name = _student.Name;
                model.Gender = _student.Gender;
                model.Major = _student.Major;
            }
            List<SelectListItem> majorList = new List<SelectListItem>();
            foreach (string s in getMajors())
            {
                majorList.Add(new SelectListItem
                {
                    Selected = true,
                    Text = s,
                    Value = s
                });
            }
            ViewBag.Majors = majorList;

            return PartialView("PartialModifyStudent", model);
        }

        [HttpPost]
        public ActionResult Modify(StudentInfoViewModel student)
        {
            Student _student = db.Students.SingleOrDefault(d => d.Id == student.Id);
            if (_student != null)
            {
                _student.Name = student.Name;
                _student.Number = student.Number;
                _student.Gender = student.Gender;
                _student.Major = student.Major;
                db.SaveChanges();
                return PartialView("PartialStudentList", bindStudentsViewModel(getPageInfo(0)));
            }
            return Json(new { code = 1, data = "修改失败" });
        }

        [HttpPost]
        public ActionResult RePassword(int id)
        {
            Student _student = db.Students.SingleOrDefault(d => d.Id == id);
            if (_student != null)
            {
                _student.Password = "123456";
                db.SaveChanges();
                return Json(new { code = 0, data = "密码重置成功" });
            }
            return Json(new { code = 1, data = "密码重置失败" });
        }

        public ActionResult LoadScore(int id)
        {
            Student student = db.Students.SingleOrDefault(s => s.Id == id);
            StudentScoreViewModel model = new StudentScoreViewModel();
            if (student != null)
            {
                model.StudentName = student.Name;
                model.Scores = student.Scores.Select(sc => new ScoreInfoViewModel
                {
                    ScoreName = sc.Course.CourseName,
                    ScoreValue = sc.Value??0
                }).ToList();
                model.Total = model.Scores.Sum(sc => sc.ScoreValue);
            }
            return PartialView("PartialStudentScore", model);
        }

        public ActionResult UploadData()
        {
            return PartialView("UploadData");
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
                        //else if (b == 2)
                        //{
                        //    return Json(new { code = "102", msg = "学生数据重复，请确认后再导入。" });
                        //}
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
                        s.Password = dr["出生日期"].ToString();//利用出生日期作为登录密码
                        //判断导入学生信息是否与数据库中重复。
                        if (checkStudentExist(s.Number))
                        {
                            //return 2;//2：数据重复
                            continue;
                        }
                        this.db.Students.Add(s);
                    }
                    int i = this.db.SaveChanges();
                    if (i > 0) result = 1; //1：成功
                }
            }
            return result;
        }

        private bool checkStudentExist(string number)
        {
            bool r = false;
            int ss = this.db.Students.Count(s => s.Number == number);
            if (ss != 0) r = true;
            return r;
        }

        private List<String> getMajors()
        {
            return db.Students.Select(s => s.Major).Distinct().ToList();
        }

        private PagedStudentsViewModel getPageInfo(int pageNumber)
        {
            int all = db.Students.Count();
            int pageSize = 18;
            return new PagedStudentsViewModel
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                PageCount = (int)Math.Ceiling(Convert.ToDecimal(all / pageSize))
            };
        }

        private List<StudentInfoViewModel> bindStudentsViewModel(int? pageNumber,int pageSize=20)
        {
            int pageNum = pageNumber ?? 0;
            var students = db.Students.OrderBy(s => s.Number).Skip(pageNum * pageSize).Take(pageSize).ToList();

            List<StudentInfoViewModel> model = new List<StudentInfoViewModel>();
            foreach (var s in students)
            {
                model.Add(new StudentInfoViewModel
                {
                    Id = s.Id,
                    Number = s.Number,
                    Name = s.Name,
                    Gender = s.Gender,
                    Major = s.Major
                });
            }
            return model;
        }

        private List<StudentInfoViewModel> bindStudentsViewModel(PagedStudentsViewModel arg)
        {
            return bindStudentsViewModel(arg.PageNumber, arg.PageSize);
        }
    }
}
