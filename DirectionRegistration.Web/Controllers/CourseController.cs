using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using DirectionRegistration.Web.Filters;
using System.Data.OleDb;
using System.Data;

namespace DirectionRegistration.Web.Controllers
{
    [SuperCheck]
    public class CourseController : Controller
    {
        private readonly RegistrationDbContext db = new RegistrationDbContext();
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
            if (!IsExistedCourse(course.CourseName))
            {
                Course _course = new Course
                {
                    CourseName = course.CourseName
                };
                db.Courses.Add(_course);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return PartialView("PartialCourseList", GetCoursesViewModel());
                }
            }
            return Json(new { code = 1, data = "课程已存在" });
        }

        public ActionResult Modify(int Id)
        {
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
            Course _course = db.Courses.SingleOrDefault(c => c.Id == course.Id);
            if (_course != null)
            {
                _course.CourseName = course.CourseName;
                int i = db.SaveChanges();
                return PartialView("PartialCourseList", GetCoursesViewModel());
            }
            return Json(new { code = 1, data = "修改失败" });
        }

        [HttpPost]
        public ActionResult Delete(int courseId)
        {
            Course _course = db.Courses.SingleOrDefault(c => c.Id == courseId);
            if (_course != null)
            {
                db.Scores.RemoveRange(_course.Scores);
                db.DirectionCourses.RemoveRange(_course.DirectionCourse);
                db.Courses.Remove(_course);
                int i = db.SaveChanges();
                if (i == 0)
                {
                    return Json(new { code = 1, data = "删除失败" });
                }
            }
            return PartialView("PartialCourseList", GetCoursesViewModel());
        }

        [HttpGet]
        public ActionResult GetCourses()
        {
            return PartialView("PartialCourseList", GetCoursesViewModel());
        }

        public ActionResult UploadScore()
        {
            return PartialView("PartialUploadScore");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadScore(IEnumerable<HttpPostedFileBase> files, int id)
        {
            if (files != null)
            {
                foreach (var file in files)
                {
                    string fileExtentian = file.FileName.Substring(file.FileName.LastIndexOf(".")).ToLower();
                    if (fileExtentian == ".xls" || fileExtentian == ".xlsx")
                    {
                        string newFileName = DateTime.Now.ToString("yyyyMMddhhmmss") + fileExtentian;
                        string path = Server.MapPath("~/Content/UploadFiles/Score_" + newFileName);
                        file.SaveAs(path);


                        var course = db.Courses.SingleOrDefault(c => c.Id == id);
                        if (course != null)
                        {                            
                            //从Excel中导入学生考核课程的成绩，同一学生同一课程的成绩可以重复导入，后导入的覆盖先导入的。
                            ImportReturnModel b = ImportScoresFromExcel(path, course);

                            if (b.Code == 1)
                            {
                                return Json(new { code = "101", msg = "成绩数据导入成功。" });
                            }
                            else
                            {
                                string others = "未找到对应信息，无法导入其成绩。\n";
                                foreach (string s in b.Others)
                                {
                                    others += $"[{s}]";
                                }
                                return Json(new { code = "102", msg = "导入遇到一些问题。", data = others });
                            }
                        }
                        else
                        {
                            return Json(new { code = "100", msg = $"未找到相应课程" });
                        }
                    }
                }
            }
            return Json(new { code = "100", msg = "成绩数据上传失败" });
        }

        /// <summary>
        /// 从上传的学生成绩Excel中导入学生的指定课程成绩信息
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cid">课程编号</param>
        /// <returns></returns>
        private ImportReturnModel ImportScoresFromExcel(string path, Course course)
        {
            //int result = 0;//0：失败
            var result = new ImportReturnModel
            {
                Code = 0,
                Others = new List<string>()
            };

            string connectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=" + path + "; Extended Properties=Excel 8.0;";
            using (OleDbConnection Connection = new OleDbConnection(connectionString))
            {
                DataTable dt = new DataTable();
                Connection.Open();
                using (OleDbCommand command = new OleDbCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "SELECT * FROM [成绩表$]";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                    adapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        string number = dr["学号"].ToString().Trim();
                        string studentName = dr["姓名"].ToString().Trim();
                        //string courseName = dr["课程"].ToString().Trim();
                        float.TryParse(dr["成绩"].ToString().Trim(), out float scoreValue);


                        //判断导入的学生成绩是否有对应学生信息。
                        var student = db.Students.SingleOrDefault(stu => stu.Number == number);
                        if (student == null)
                        {
                            result.Others.Add(number + studentName); //如果没有，则收集学生学号和姓名。
                            continue;
                        }

                        //查找该学生该门课程成绩是否存在，如果存在则更新数据，不存在则新增数据。
                        var score = db.Scores.SingleOrDefault(ss => ss.Student.Number == number && ss.Course.CourseName == course.CourseName);
                        if (score == null)
                        {
                            Score s = new Score
                            {
                                Student = student,
                                Course = course,
                                Value = scoreValue
                            };
                            db.Scores.Add(s);
                        }
                        else
                        {
                            score.Value = scoreValue;
                            db.Entry<Score>(score).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    adapter.Dispose();
                    int i = db.SaveChanges();
                    if (i > 0) result.Code = 1; //1：成功
                }
            }
            return result;
        }

        private List<CourseViewModel> GetCoursesViewModel()
        {
            List<CourseViewModel> courses = new List<CourseViewModel>();
            db.Courses.ToList().ForEach(item =>
            {
                courses.Add(new CourseViewModel
                {
                    Id = item.Id,
                    CourseName = item.CourseName,
                    StudentCount = item.Scores.Count()
                });
            });
            return courses;
        }

        private bool IsExistedCourse(string courseName)
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
    
    class ImportReturnModel
    {
        public int Code { get; set; }
        public List<string> Others { get; set; }
    }
}
