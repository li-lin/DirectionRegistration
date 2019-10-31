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

        /// <summary>
        /// 生成录取结果的Excel文件
        /// </summary>
        /// <returns></returns>
        public ActionResult GenerateResult()
        {            
            string tempPath = Server.MapPath("~/Content/DownloadFiles/tempGeneration1.xls");
            string path = Server.MapPath("~/Content/DownloadFiles/" + (DateTime.Now.Year - 2) + "级方向录取结果-" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".xls");
            System.IO.File.Copy(tempPath, path);

            string connectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=" + path + "; Extended Properties=Excel 8.0;";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                var directions = db.Directions.ToList();

                for (int i = 0; i < directions.Count; i++)
                {
                    StringBuilder stringBuilder = new StringBuilder("CREATE TABLE [" + directions[i].Title +
                        "$](学号 Char(100), 姓名 char(100),性别 char(20), 专业 char(160)," +
                        "录取方向 char(160),志愿顺序 char(40),录取批次 char(40),成绩排名 char(40),总成绩 char(40)");
                    
                    int c_count = directions[i].DirectionCourses.Count;
                    for (int j = 0; j < c_count + 1; j++)//3门考核课程以及加分项的名称和成绩
                    {
                        stringBuilder.Append($", 课程{j + 1} char(160)");
                        stringBuilder.Append($", 成绩{j + 1} char(40)");
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
                //List<GenerationResultModel> result = Generate();
                List<EnrollmentViewModel> enrollments = GetEnrollmentViewModels();

                foreach (var r in enrollments)
                {
                    if (r.DirectionName == null) continue;
                    StringBuilder sb1 = new StringBuilder("INSERT INTO[" + r.DirectionName + 
                        "$](学号, 姓名, 性别, 专业, 录取方向, 志愿顺序, 录取批次, 成绩排名, 总成绩");
                    StringBuilder sb2 = new StringBuilder(" VALUES(@number,@name,@gender,@major,@direction,@order,@time,@scoreOrder,@total");

                    var theDirection = directions.SingleOrDefault(d => d.Title == r.DirectionName);
                    int c_count = theDirection.DirectionCourses.Count;
                    var scores = r.Scores;
                    for (int i = 0; i < c_count + 1; i++)
                    {

                        sb1.Append($", {scores[i].ScoreName}");
                        sb2.Append($", @score{i + 1}");
                    }
                    sb1.Append(")");
                    sb2.Append(")");

                    string insertCommand = sb1.ToString() + sb2.ToString();
                    using (OleDbCommand cmdInsert = new OleDbCommand())
                    {
                        cmdInsert.Connection = connection;
                        cmdInsert.CommandText = insertCommand;
                        cmdInsert.Parameters.Add(new OleDbParameter("@number", r.StudentInfo.Number));
                        cmdInsert.Parameters.Add(new OleDbParameter("@name", r.StudentInfo.Name));
                        cmdInsert.Parameters.Add(new OleDbParameter("@gender", r.StudentInfo.Gender));
                        cmdInsert.Parameters.Add(new OleDbParameter("@major", r.StudentInfo.Major));
                        cmdInsert.Parameters.Add(new OleDbParameter("@direction", r.DirectionName));
                        cmdInsert.Parameters.Add(new OleDbParameter("@order", r.DirectionOrder));
                        cmdInsert.Parameters.Add(new OleDbParameter("@time", r.EnrollTime));
                        cmdInsert.Parameters.Add(new OleDbParameter("@scoreOrder", r.ScoreOrder));
                        cmdInsert.Parameters.Add(new OleDbParameter("@total", r.ScoreTotal));

                        for (int i = 0; i < scores.Count + 1; i++)
                        {
                            var scoreValue = scores[i].ScoreValue;
                            cmdInsert.Parameters.Add(new OleDbParameter($"@score{i + 1}", scoreValue));
                        }
                        cmdInsert.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            //return File(path, "application/vnd.ms-excel", System.IO.Path.GetFileName(path));
            return Content($"/Content/DownloadFiles/{System.IO.Path.GetFileName(path)}");
        }       

        /**
         * 通过两轮志愿进行录取
         * 第一轮：通过方向指定的签到课程成绩，仅录取第一志愿。
         * 第二轮：剩下的学生通过指定的专业课成绩进行本轮录取。
         * 两轮录取都采用志愿优先、再成绩优先、录取名额限制的规则进行。
         */
        private List<GenerationResultModel> Generate()
        {
            //var enrollments = db.Enrollments;

            List<GenerationResultModel> result = db.Students.Select(stu => new GenerationResultModel
            {
                StudentScore = new StudentScoreViewModel
                {
                    StudentId = stu.Id,
                    StudentNumber = stu.Number,
                    StudentName = stu.Name,
                    StudentGender = stu.Gender,
                    StudentMajor = stu.Major,
                    Scores = stu.Scores.Select(sc => new ScoreInfoViewModel //该学生所有成绩
                    {
                        ScoreName = sc.Course.CourseName,
                        ScoreValue = sc.Value ?? 0
                    }).ToList()
                },
                Selections = stu.DirectionStudents
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
                Count = d.Enrollments.Count, //本轮录取的初始数量为提前直接录取后的人数。
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
                //找出所有选择该方向为第一志愿的学生信息，并计算其考核课程以及加分项的总成绩。
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
                                //筛选出该学生第一志愿方向对应课程及成绩。
                                List<ScoreInfoViewModel> thisStuDirectionScores = new List<ScoreInfoViewModel>();
                                foreach (var dc in direction.Courses)
                                {
                                    var scores = s.StudentScore.Scores;
                                    var score = scores.SingleOrDefault(sc => sc.ScoreName == dc.CourseName);
                                    if (score != null)
                                    {
                                        thisStuDirectionScores.Add(score);
                                    }
                                }

                                //如果为第一志愿则计算出考核课程的加权总分。
                                double total = 0.0;

                                //针对当前方向有超过3门指定考核课程的，仅取成绩较高的4门课的成绩。
                                if (thisStuDirectionScores.Count > 3)
                                {
                                    thisStuDirectionScores = thisStuDirectionScores.OrderByDescending(sds => sds.ScoreValue).Take(4).ToList();
                                }
                                //计算方向考核课程总成绩，并记录参与该方向考核的学生课程名称。
                                foreach(var item in thisStuDirectionScores)
                                {
                                    total += item.ScoreValue;
                                    s.EnrollmentCourses.Add(item.ScoreName);
                                }

                                //有加分的学生成绩，记入总分。
                                var addtinalScore = s.StudentScore.Scores.SingleOrDefault(ss => ss.ScoreName == "加分项");
                                if (addtinalScore != null)
                                {
                                    total += addtinalScore.ScoreValue;
                                    s.EnrollmentCourses.Add(addtinalScore.ScoreName);
                                }
                                s.StudentScore.Total = total;
                            }
                        }
                    }
                    return r;
                });

                /***第一轮录取逻辑***/
                int so = 0;//学生在当前方向的考核课程的排名
                double theLastOne = 0; //当前方向录取到限定人数时的最小总成绩。
                foreach (var tds in thisDirectionStudents.OrderByDescending(ds => ds.StudentScore.Total))
                {
                    double thisTotal = tds.StudentScore.Total;
                    bool canEnroll = false;
                    if (thisTotal > 0)
                    {
                        if (direction.Count < direction.Max)
                        {
                            theLastOne = thisTotal;
                            canEnroll = true;

                        }
                        else if (thisTotal == theLastOne) //当达到方向上线人数时，如果存在与最后一个成绩相同的学生，一并录取。
                        {
                            canEnroll = true;
                        }
                        else
                        {
                            break;
                        }

                        if (canEnroll)
                        {
                            tds.DirectionName = direction.DirectionName;
                            tds.DirectionOrder = 1;
                            direction.Count++;
                            so++;

                            //将录取信息记入数据库
                            var enrollment = new Enrollment
                            {
                                Direction = db.Directions.SingleOrDefault(d => d.Id == direction.DirectionId),
                                Student = db.Students.SingleOrDefault(s => s.Id == tds.StudentScore.StudentId),
                                ScoreOrder = so,
                                WhichTime = 1,
                                EnrollCourses = new List<Course>()
                            };
                            foreach(var cname in tds.EnrollmentCourses)
                            {
                                var course = db.Courses.SingleOrDefault(c => c.CourseName == cname);
                                enrollment.EnrollCourses.Add(course);
                            }
                            db.Enrollments.Add(enrollment);
                        }
                    }
                }
            }

            /***第二轮录取，除去第一轮已经录取的学生，剩下学生按照指定专业基础课成绩进行拉通录取，包括第一轮中为录取满员的方向。***/
            //获取还未被录取的学生
            var restStudents = result.Where(r => String.IsNullOrEmpty(r.DirectionName));
            //获取专业基础课信息
            var basicCoureses = db.Courses.Where(c => c.DirectionCourse.Count() == 0);

            foreach (var r in restStudents)
            {
                List<ScoreInfoViewModel> thisStuBasicScores = new List<ScoreInfoViewModel>();
                foreach (var course in basicCoureses.ToArray())
                {
                    var score = r.StudentScore.Scores.SingleOrDefault(sc => sc.ScoreName == course.CourseName);
                    if (score != null)
                    {
                        thisStuBasicScores.Add(score);
                    }
                }
                foreach(var item in thisStuBasicScores) //*包括了具有加分的学生成绩
                {
                    r.StudentScore.Total += item.ScoreValue;
                    r.EnrollmentCourses.Add(item.ScoreName);
                }
            }

            int bso = 0;//学生专业基础课成绩排名
            foreach (var stu in restStudents.OrderByDescending(r => r.StudentScore.Total))
            {
                bso++;
                //按照学生志愿选择顺序依次录取。
                foreach (var selection in stu.Selections)
                {
                    var dir = directions.SingleOrDefault(d => d.DirectionId == selection.Id);
                    if (dir.Count < dir.Max)
                    {
                        stu.DirectionName = dir.DirectionName;
                        stu.DirectionOrder = selection.Order;
                        dir.Count++;
                        
                        //将录取信息记入数据库
                        var enrollment = new Enrollment
                        {
                            Direction = db.Directions.SingleOrDefault(d => d.Id == dir.DirectionId),
                            Student = db.Students.SingleOrDefault(s => s.Id == stu.StudentScore.StudentId),
                            ScoreOrder = bso,
                            WhichTime = 2,
                            EnrollCourses = new List<Course>()
                        };
                        foreach(var cname in stu.EnrollmentCourses)
                        {
                            var course = db.Courses.SingleOrDefault(c => c.CourseName == cname);
                            if (course != null)
                            {
                                enrollment.EnrollCourses.Add(course);
                            }
                        }

                        db.Enrollments.Add(enrollment);
                        break;
                    }
                }
            }
            int ok = db.SaveChanges();           
            return result;
        }

        /// <summary>
        /// 获取录取信息列表
        /// </summary>
        /// <returns></returns>
        private List<EnrollmentViewModel> GetEnrollmentViewModels()
        {
            List<EnrollmentViewModel> enrollments = db.Enrollments.Select(en => new EnrollmentViewModel
            {
                EnrollmentId = en.EnrollmentId,
                StudentInfo = new StudentInfoViewModel
                {
                    Id = en.Student.Id,
                    Name = en.Student.Name,
                    Gender = en.Student.Gender,
                    Major = en.Student.Major,
                    Number = en.Student.Number
                },
                DirectionName = en.Direction.Title,
                DirectionOrder = en.Direction.DirectionStudents.SingleOrDefault(ds => ds.Student.Id == en.Student.Id).Order,
                EnrollTime = en.WhichTime,
                ScoreOrder = en.ScoreOrder,
                Scores = en.EnrollCourses.Select(ec => new ScoreInfoViewModel
                {
                    ScoreName = ec.CourseName,
                    ScoreValue = ec.Scores.SingleOrDefault(s => s.Student.Id == en.Student.Id).Value ?? 0.0
                }).ToList()
            }).ToList();

            foreach (var enroll in enrollments)
            {
                enroll.ScoreTotal = enroll.Scores.Sum(s => s.ScoreValue);
            }
            return enrollments;
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
