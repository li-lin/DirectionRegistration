using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.OleDb;
using X.PagedList;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using DirectionRegistration.Web.Filters;

namespace DirectionRegistration.Web.Controllers
{
    [SuperCheck]
    public class StudentController : Controller
    {
        private readonly RegistrationDbContext db = new RegistrationDbContext();
        //
        // GET: /Stuent/

        public ActionResult Index()
        {            
            ViewBag.Majors = GetMajorSelectionItems();
            return View();
        }

        private List<SelectListItem> GetMajorSelectionItems()
        {
            List<SelectListItem> majorList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Selected = true,
                    Text = "未选择",
                    Value = ""
                }
            };
            foreach (string s in GetMajors())
            {
                majorList.Add(new SelectListItem
                {
                    Selected = false,
                    Text = s,
                    Value = s
                });
            }
            return majorList;
        }

        [HttpPost]
        public ActionResult Add(StudentInfoViewModel model)
        {
            if (!CheckStudentExist(model.Number))
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
                    return GetAllStudentsView();
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
       
        public ActionResult GetStudents(int? page, string gender, string major, string number, string name)
        {
            int pageSize = 18;
            var students = db.Students.Select(s => new StudentInfoViewModel
            {
                Id = s.Id,
                Number = s.Number,
                Name = s.Name,
                Gender = s.Gender,
                Major = s.Major
            });

            if (String.IsNullOrEmpty(gender) == false)
            {
                students = students.Where(s => s.Gender == gender);
                ViewBag.Gender = gender;
            }
            if (String.IsNullOrEmpty(major) == false)
            {
                students = students.Where(s => s.Major == major);
                ViewBag.Major = major;
            }
            if (String.IsNullOrEmpty(number) == false)
            {
                students = students.Where(s => s.Number == number);
            }
            if (String.IsNullOrEmpty(name) == false)
            {
                students = students.Where(s => s.Name == name);
            }

            return PartialView("PartialStudentList", 
                students.OrderBy(s=>s.Number).ToPagedList(page ?? 1, pageSize));
        }

        private ActionResult GetAllStudentsView()
        {
            return GetStudents(1, null, null, null, null);
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
            return GetAllStudentsView();
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
            foreach (string s in GetMajors())
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
                return GetAllStudentsView();
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

                        int b = ImportStudentsFromExcel(path);
                        if (b == 1)
                        {
                            return Json(new { code = "101", msg = "学生数据导入成功。" });
                        }
                    }
                }
            }
            return Json(new { code = "100", msg = "学生数据上传或导入失败。" });
        }

        /// <summary>
        /// 从上传的Excel中导入学生信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private int ImportStudentsFromExcel(string path)
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
                        if (CheckStudentExist(s.Number))
                        {
                            //return 2;//2：数据重复
                            continue;
                        }
                        db.Students.Add(s);
                    }
                    int i = db.SaveChanges();
                    if (i > 0) result = 1; //1：成功
                }
            }
            return result;
        }

        private bool CheckStudentExist(string number)
        {
            bool r = false;
            int ss = db.Students.Count(s => s.Number == number);
            if (ss != 0) r = true;
            return r;
        }

        private List<String> GetMajors()
        {
            return db.Students.Select(s => s.Major).Distinct().ToList();
        }      
    }
}
