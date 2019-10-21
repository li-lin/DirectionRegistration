using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Repository;
using DirectionRegistration.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace DirectionRegistration.Web.Controllers
{
    public class ScoreController : Controller
    {
        private readonly RegistrationDbContext db = new RegistrationDbContext();
        //
        // GET: /Score/

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult UploadScore()
        {
            return PartialView("PartialUploadScore");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadScore(IEnumerable<HttpPostedFileBase> files)
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

                        RemoveAllScore();

                        ImportReturnModel b = ImportStudentsFromExcel(path);
                        if (b.Code == 1)
                        {
                            return Json(new { code = "101", msg = "成绩数据导入成功。" });
                        }
                        else
                        {
                            string others = "未找到学生：";
                            foreach(string s in b.Others)
                            {
                                others += $"[{s}]";
                            }
                            return Json(new { code = "102", msg = "导入遇到一些问题。", data = others });
                        }
                    }
                }
            }
            return Json(new { code = "100", msg = "学生数据上传或导入失败。" });
        }

        private void RemoveAllScore()
        {
            db.Scores.RemoveRange(db.Scores);
            db.SaveChanges();
        }

        private ImportReturnModel ImportStudentsFromExcel(string path)
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
                        string number = dr["学号"].ToString();
                        string studentName = dr["姓名"].ToString();
                        string courseName = dr["课程"].ToString();
                        float.TryParse(dr["成绩"].ToString(), out float scoreValue);


                        //判断导入的学生成绩是否有对应学生信息。
                        var student = db.Students.SingleOrDefault(stu => stu.Number == number);
                        if (student == null)
                        {
                            result.Others.Add(number); //如果没有，则收集学生学号。
                            continue;
                        }

                        Score s = new Score
                        {
                            Student = student,
                            Course = db.Courses.SingleOrDefault(c => c.CourseName == courseName),
                            Value = scoreValue
                        };
                        db.Scores.Add(s);
                    }
                    adapter.Dispose();
                    int i = this.db.SaveChanges();
                    if (i > 0) result.Code = 1; //1：成功
                }
            }
            return result;
        }

        /// <summary>
        /// 生成录取结果的Excel文件
        /// </summary>
        /// <returns></returns>
        public ActionResult GenerateResult()
        {
            string tempPath = Server.MapPath("~/Content/DownloadFiles/tempGeneration.xls");
            string path = Server.MapPath("~/Content/DownloadFiles/" + (DateTime.Now.Year - 2) + "级方向录取结果-" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".xls");
            System.IO.File.Copy(tempPath, path);

            string connectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=" + path + "; Extended Properties=Excel 8.0;";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                var directions = db.Directions.ToList();

                for (int i = 0; i < directions.Count; i++)
                {
                    StringBuilder stringBuilder = new StringBuilder("CREATE TABLE [" + directions[i].Title + "$](学号 Char(100), 姓名 char(100),性别 char(20), 专业 char(160),录取方向 char(160),志愿顺序 char(40),总成绩 char(40)");
                    foreach (var c in db.Courses)
                    {
                        stringBuilder.Append($", {c.CourseName} char(160)");
                    }
                    stringBuilder.Append(")");

                    using (OleDbCommand command = new OleDbCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = stringBuilder.ToString();
                        command.ExecuteNonQuery();
                    }
                }

                //利用模型生成录取数据
                List<GenerationResultModel> result = Generate();

                foreach (var r in result)
                {
                    if (r.DirectionName == null) continue;
                    StringBuilder sb1 = new StringBuilder("INSERT INTO[" + r.DirectionName + "$](学号, 姓名, 性别, 专业, 录取方向, 志愿顺序, 总成绩");
                    StringBuilder sb2 = new StringBuilder(" VALUES(@number,@name,@gender,@major,@direction,@order,@total");
                    var courses = db.Courses.ToList();
                    for (int i = 0; i < courses.Count; i++)
                    {

                        sb1.Append($", {courses[i].CourseName}");
                        sb2.Append($", @score{i + 1}");
                    }
                    sb1.Append(")");
                    sb2.Append(")");

                    string insertCommand = sb1.ToString() + sb2.ToString();
                    using (OleDbCommand cmdInsert = new OleDbCommand())
                    {
                        cmdInsert.Connection = connection;
                        cmdInsert.CommandText = insertCommand;
                        cmdInsert.Parameters.Add(new OleDbParameter("@number", r.StudentScore.StudentNumber));
                        cmdInsert.Parameters.Add(new OleDbParameter("@name", r.StudentScore.StudentName));
                        cmdInsert.Parameters.Add(new OleDbParameter("@gender", r.StudentScore.StudentGender));
                        cmdInsert.Parameters.Add(new OleDbParameter("@major", r.StudentScore.StudentMajor));
                        cmdInsert.Parameters.Add(new OleDbParameter("@direction", r.DirectionName));
                        cmdInsert.Parameters.Add(new OleDbParameter("@order", r.DirectionOrder)); ;
                        cmdInsert.Parameters.Add(new OleDbParameter("@total", r.StudentScore.Total));

                        for (int i = 0; i < courses.Count; i++)
                        {
                            var score = r.StudentScore.Scores.SingleOrDefault(sc => sc.ScoreName == courses[i].CourseName);
                            if (score != null && score.ScoreName != null)
                            {
                                cmdInsert.Parameters.Add(new OleDbParameter($"@score{i + 1}", score.ScoreValue));
                            }
                            else
                            {
                                cmdInsert.Parameters.Add(new OleDbParameter($"@score{i + 1}", 0));
                            }
                        }
                        cmdInsert.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            return File(path, "application/vnd.ms-excel", path.Substring(path.LastIndexOf("\\")));
        }

        //录取，以分数优先，由于算法发生改变，该生成算法作废。
        /*
        private List<GenerationResultViewModel> GenerateOld()
        {
            List<GenerationResultViewModel> result = db.Students.Select(stu => new GenerationResultViewModel
            {
                StudentScore = new StudentScoreViewModel
                {
                    StudentId = stu.Id,
                    StudentNumber = stu.Number,
                    StudentName = stu.Name,
                    StudentGender = stu.Gender,
                    StudentMajor = stu.Major,
                    Scores = stu.Scores.Select(sc => new ScoreInfoViewModel
                    {
                        ScoreName = sc.Course.CourseName,
                        ScoreValue = sc.Value ?? 0
                    }).ToList()
                },
                Selections = stu.DirectionStudents
                .Where(ds => ds.Order > 0) //只取出前5个志愿，其他志愿Order属性为0，表示未选择的志愿。
                .OrderBy(ds => ds.Order)
                .Select(ds => new DirectionInfoViewModel
                {
                    Id = ds.Direction.Id,
                    DirectionName = ds.Direction.Title,
                    Order = ds.Order
                }).ToList()
            }).ToList();

            int courseCount = db.Courses.Count();
            foreach(var r in result)
            {
                r.StudentScore.Total = r.StudentScore.Scores.Sum(sc => sc.ScoreValue);
                if(r.StudentScore.Scores.Count< courseCount)
                {
                    foreach (var course in db.Courses.ToArray())
                    {
                        if(r.StudentScore.Scores.SingleOrDefault(sc=>sc.ScoreName== course.CourseName) == null)
                        {
                            r.StudentScore.Scores.Add(new ScoreInfoViewModel
                            {
                                ScoreName = course.CourseName,
                                ScoreValue = 0
                            });
                        }
                    }
                }
            }
            result = result.OrderByDescending(r => r.StudentScore.Total).ToList();

            List<RegDirectionInfoModel> directions = db.Directions.Select(d => new RegDirectionInfoModel
            {
                DirectionId = d.Id,
                DirectionName = d.Title,
                Max = d.Max,
                Count = 0
            }).ToList();


            foreach(var stu in result)
            {
                if (String.IsNullOrEmpty(stu.DirectionName))
                {
                    foreach (var selection in stu.Selections)
                    {
                        var dir = directions.SingleOrDefault(d => d.DirectionId == selection.Id);
                        if (dir.Count < dir.Max)
                        {
                            stu.DirectionName = dir.DirectionName;
                            stu.DirectionOrder = selection.Order;
                            dir.Count++;
                            break;
                        }
                    }
                }
            }

            return result;
        }
        */

        /**
         * 通过两轮志愿进行录取
         * 第一轮：通过方向指定的签到课程成绩，仅录取第一志愿。
         * 第二轮：剩下的学生通过指定的专业课成绩进行本轮录取。
         * 两轮录取都采用志愿优先、再成绩优先、录取名额限制的规则进行。
         */
        private List<GenerationResultModel> Generate()
        {
            List<GenerationResultModel> result = db.Students.Select(stu => new GenerationResultModel
            {
                StudentScore = new StudentScoreViewModel
                {
                    StudentId = stu.Id,
                    StudentNumber = stu.Number,
                    StudentName = stu.Name,
                    StudentGender = stu.Gender,
                    StudentMajor = stu.Major,
                    Scores = stu.Scores.Select(sc => new ScoreInfoViewModel
                    {
                        ScoreName = sc.Course.CourseName,
                        ScoreValue = sc.Value ?? 0
                    }).ToList()
                },
                Selections = stu.DirectionStudents
                .Where(ds => ds.Order > 0) //只取出前5个志愿，其他志愿Order属性为0，表示未选择的志愿。
                .OrderBy(ds => ds.Order)
                .Select(ds => new DirectionInfoViewModel
                {
                    Id = ds.Direction.Id,
                    DirectionName = ds.Direction.Title,
                    Order = ds.Order
                }).ToList()
            }).ToList();

            /***第一轮录取（第一志愿，且具有对应签到课程成绩者）***/
            //获得所有方向的相关信息，包括该方向考核课程及权值。
            List<RegDirectionInfoModel> directions = db.Directions.Select(d => new RegDirectionInfoModel
            {
                DirectionId = d.Id,
                DirectionName = d.Title,
                Max = d.Max,
                Count = 0,
                Courses = d.DirectionCourses.Select(dc => new DirectionCourseInfoModel
                {
                    CourseId = dc.Course.Id,
                    CourseName = dc.Course.CourseName,
                    Proportion = dc.Proportion
                }).ToList()
            }).ToList();
            //按方向依次进行录取
            foreach(var direction in directions)
            {
                //找出所有选择该方向为第一志愿的学生信息。
                var thisDirectionStudents = result.Where(s => {
                    bool r = false;
                    if (s.Selections.Count != 0)
                    {
                        var dir = s.Selections.SingleOrDefault(d => d.Order==1);
                        if (dir != null)
                        {
                            r = dir.Id == direction.DirectionId;
                            if (r)
                            {
                                //如果为第一志愿则计算出考核课程的加权总分。
                                double total = 0.0;
                                foreach(var dc in direction.Courses)
                                {
                                    var score = s.StudentScore.Scores.SingleOrDefault(sc => sc.ScoreName == dc.CourseName);
                                    if (score != null)
                                    {
                                        total += score.ScoreValue * dc.Proportion;
                                    }
                                }
                                s.StudentScore.Total = total;
                            }
                        }
                    }
                    return r;
                });

                foreach(var tds in thisDirectionStudents.OrderByDescending(ds => ds.StudentScore.Total))
                {
                    if (tds.StudentScore.Total > 0)
                    {
                        if (direction.Count < direction.Max)
                        {
                            tds.DirectionName = direction.DirectionName;
                            tds.DirectionOrder = 1;
                            direction.Count++;
                        }
                    }
                }
            }

            /***第二轮录取，除去第一轮已经录取的学生，剩下学生按照指定专业基础课成绩进行拉通录取，包括第一轮中为录取满员的方向。***/
            //获取还未被录取的学生
            var restStudents = result.Where(r => String.IsNullOrEmpty(r.DirectionName));
            //获取专业基础课信息
            var basicCourese = db.Courses.Where(c => c.DirectionCourse.Count() == 0);

            int courseCount = basicCourese.Count();
            foreach (var r in restStudents)
            {
                if (r.StudentScore.Scores.Count < courseCount)
                {
                    foreach (var course in basicCourese.ToArray())
                    {
                        if (r.StudentScore.Scores.SingleOrDefault(sc => sc.ScoreName == course.CourseName) == null)
                        {
                            r.StudentScore.Scores.Add(new ScoreInfoViewModel
                            {
                                ScoreName = course.CourseName,
                                ScoreValue = 0
                            });
                        }
                    }
                }
                r.StudentScore.Total = r.StudentScore.Scores.Sum(sc => sc.ScoreValue);
            }

            foreach (var stu in restStudents.OrderByDescending(r => r.StudentScore.Total))
            {
                //按照学生志愿选择顺序依次录取。
                foreach (var selection in stu.Selections)
                {
                    var dir = directions.SingleOrDefault(d => d.DirectionId == selection.Id);
                    if (dir.Count < dir.Max)
                    {
                        stu.DirectionName = dir.DirectionName;
                        stu.DirectionOrder = selection.Order;
                        dir.Count++;
                        break;
                    }
                }
            }
            return result;
        }


        class ImportReturnModel
        {
            public int Code { get; set; }
            public List<string> Others { get; set; }
        }

        class RegDirectionInfoModel
        {
            public int DirectionId { get; set; }
            public string DirectionName { get; set; }
            /// <summary>
            /// 方向录取人数上线
            /// </summary>
            public int Max { get; set; }
            /// <summary>
            /// 方向已录取人数
            /// </summary>
            public int Count { get; set; }
            /// <summary>
            /// 方向考核签到课程
            /// </summary>
            public List<DirectionCourseInfoModel> Courses { get; set; }
        }

        class DirectionCourseInfoModel
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; }
            public double Proportion { get; set; }
        }
    }
}
