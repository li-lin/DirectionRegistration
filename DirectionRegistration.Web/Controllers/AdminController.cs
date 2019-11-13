using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using X.PagedList;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Models;
using DirectionRegistration.Web.Filters;

namespace DirectionRegistration.Web.Controllers
{
    [CustHandleError]
    public class AdminController : Controller
    {
        private readonly RegistrationDbContext db = new RegistrationDbContext();

        [LoginCheck]
        public ActionResult Index(int? page)
        {
            string currentAdmin = Session["admin"] as string;
            var teacher = db.Teachers.SingleOrDefault(t => t.LoginName == currentAdmin);
            if (teacher != null)
            {
                ViewBag.TeacherInfo = teacher.LoginName + " | " + teacher.Name;
            }
            ViewBag.All = db.Students.Count();
            ViewBag.Regs = db.DirectionStudents.GroupBy(ds => ds.Student).Count();
            ViewBag.Enrolls = db.Enrollments.Count();

            List<SelectListItem> directionItems = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Selected=true,
                    Text="全部",
                    Value=""
                }
            };
            foreach(var d in db.Directions)
            {
                directionItems.Add(new SelectListItem
                {
                    Selected = false,
                    Text = d.Title,
                    Value = d.Title
                });
            }
            directionItems.Add(new SelectListItem
            {
                Selected = false,
                Text = "其他",
                Value = "-1"
            });
            ViewBag.Directions = directionItems;

            List<SelectListItem> whichtimeItems = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Selected=true,
                    Text="全部",
                    Value="-1"
                }
            };
            for(int i = 0; i < 3; i++)
            {
                whichtimeItems.Add(new SelectListItem
                {
                    Selected = false,
                    Text = i == 0 ? "提前批" : $"第{i}批",
                    Value = i.ToString()
                });
            }
            ViewBag.WhichTimes = whichtimeItems;

            return View();
        }

        [LoginCheck]
        public ActionResult GetEnrollments(int? page, string number, string name,
            string direction, string firstwish, int whichtime = -1)
        {
            int pageSize = 18;
            var enrollments = db.Students.Select(stu => new RegistrationViewModel
            {
                Id = stu.Id,
                Number = stu.Number,
                Name = stu.Name,
                Gender = stu.Gender,
                Major = stu.Major,
                EnrollDirection = stu.Enrollment == null ? "未录取" : stu.Enrollment.Direction.Title,
                WhichTime = stu.Enrollment == null ? -1 : stu.Enrollment.WhichTime,
                OrderInDirection = stu.Enrollment == null ? 0 : stu.Enrollment.ScoreOrder,
                EnrolledOrder = stu.Enrollment == null ? 0 : stu.DirectionStudents.FirstOrDefault(ds => ds.Direction.Title == stu.Enrollment.Direction.Title).Order,
                TotalScore = stu.Enrollment == null ? 0 : stu.Enrollment.EnrollCourses.Select(ec => new ScoreInfoViewModel
                {
                    ScoreName = ec.CourseName,
                    ScoreValue = ec.Scores.FirstOrDefault(s => s.Student.Id == stu.Id).Value ?? 0.0
                }).Sum(ss => ss.ScoreValue),
                FirstSelection = stu.DirectionStudents.FirstOrDefault(ds => ds.Order == 1).Direction.Title
            }).ToList();

            ViewBag.Regs = enrollments.Count(en => String.IsNullOrEmpty(en.FirstSelection) == false);
            ViewBag.Enrolls = enrollments.Count(en => en.EnrollDirection != "未录取");
            if (string.IsNullOrEmpty(direction) && string.IsNullOrEmpty(firstwish)&& whichtime < 0)
            {
                ViewBag.Regs = enrollments.Count(en => String.IsNullOrEmpty(en.FirstSelection) == false);
                ViewBag.Enrolls = enrollments.Count(en => en.EnrollDirection != "未录取");
            }
            else
            {
                if (!String.IsNullOrEmpty(direction) && direction != "-1")
                {
                    enrollments = enrollments.Where(en => en.EnrollDirection == direction).ToList();
                    ViewBag.EnrollDirection = direction;
                    ViewBag.Enrolls = enrollments.Count();
                }
                else if (direction == "-1")
                {
                    enrollments = enrollments.Where(en => en.EnrollDirection == "未录取").ToList();
                    ViewBag.EnrollDirection = direction;
                    ViewBag.Enrolls = enrollments.Count();
                }
                //else
                //{
                //    enrollments = enrollments.Where(en => en.EnrollDirection != "未录取").ToList();
                //}

                if (whichtime >= 0)
                {
                    enrollments = enrollments.Where(en => en.WhichTime == whichtime).ToList();
                    ViewBag.WhichTime = whichtime;
                    ViewBag.Enrolls = enrollments.Count();
                }

                if (!String.IsNullOrEmpty(firstwish) && firstwish != "-1")
                {
                    enrollments = enrollments.Where(en => en.FirstSelection == firstwish).ToList();
                    ViewBag.FirstSelection = firstwish;
                    ViewBag.Regs = enrollments.Count();
                }
                else if (firstwish == "-1")
                {
                    enrollments = enrollments.Where(en => String.IsNullOrEmpty(en.FirstSelection) == true).ToList();
                    ViewBag.FirstSelection = firstwish;
                    ViewBag.Regs = enrollments.Count();
                }
                //else
                //{
                //    enrollments = enrollments.Where(en => String.IsNullOrEmpty(en.FirstSelection) == false).ToList();
                //}
            }

            if (!String.IsNullOrEmpty(number))
            {
                enrollments = enrollments.Where(en => en.Number == number).ToList();
                ViewBag.Regs = db.DirectionStudents.GroupBy(ds => ds.Student).Count();
                ViewBag.Enrolls = db.Enrollments.Count();
            }

            if (!String.IsNullOrEmpty(name))
            {
                enrollments = enrollments.Where(en => en.Name == name).ToList();
                ViewBag.Regs = db.DirectionStudents.GroupBy(ds => ds.Student).Count();
                ViewBag.Enrolls = db.Enrollments.Count();
            }

            return PartialView("PartialEnrollmentList",
                enrollments.OrderByDescending(en => en.TotalScore).ToPagedList(page ?? 1, pageSize));
        }

        /// <summary>
        /// 提前批录取，通过学生学号手动录取。
        /// </summary>
        /// <param name="number">学号</param>
        /// <returns></returns>
        [LoginCheck]
        public ActionResult EnrollBest(string number)
        {
            var stu = db.Students.SingleOrDefault(s => s.Number == number);
            if (stu != null)
            {
                var direction = stu.DirectionStudents.FirstOrDefault(ds => ds.Order == 1).Direction;
                var enrollment = new Enrollment
                {
                    Direction = direction,
                    WhichTime = 0,
                    ScoreOrder = 0,
                    Student = stu,
                    EnrollCourses = new List<Course>()
                };

                var scoreCount = direction.DirectionCourses.Count;
                if (scoreCount > 3)
                {
                    var scores = stu.Scores.Where(s => s.Course.DirectionCourse.Count != 0).OrderByDescending(s => s.Value).Take(4).ToList();
                    foreach (var sc in scores)
                    {
                        enrollment.EnrollCourses.Add(sc.Course);
                    }
                }
                else
                {
                    foreach (var c in direction.DirectionCourses)
                    {
                        enrollment.EnrollCourses.Add(c.Course);
                    }
                }

                if (stu.Enrollment == null)
                {                   
                    db.Enrollments.Add(enrollment);
                }
                else
                {                    
                    stu.Enrollment.Direction = enrollment.Direction;
                    stu.Enrollment.WhichTime = enrollment.WhichTime;
                    stu.Enrollment.ScoreOrder = enrollment.ScoreOrder;
                    stu.Enrollment.EnrollCourses.RemoveAll(c => true);
                    stu.Enrollment.EnrollCourses.AddRange(enrollment.EnrollCourses);
                }
                db.SaveChanges();
            }
            return GetEnrollments(1, number, null, null, null);
        }

        /// <summary>
        /// 第一、二批次录取
        /// </summary>
        /// <returns></returns>
        [LoginCheck]
        public ActionResult EnrollFirst()
        {
            var result = Generate().Count();
            var enrolled = db.Enrollments.Where(en => en.WhichTime > 0).Count();
            var notEnroll = db.Students.Where(s => s.Enrollment == null).Count();
            return GetEnrollments(1, null, null, null, null);
        }

        [LoginCheck]
        public ActionResult Details(int id)
        {
            var stu = db.Students.SingleOrDefault(s => s.Id == id);
            if (stu != null)
            {
                return PartialView("PartialSelectionDetails", GetStudentRegEnroll(stu));
            }
            return Json(new { code = 1, data = "系统错误" });
        }

        private RegEnrollDetailViewModel GetStudentRegEnroll(Student stu)
        {
            var reg_enroll_detail = new RegEnrollDetailViewModel
            {
                Number = stu.Number,
                Name = stu.Name,
                Gender = stu.Gender,
                Major = stu.Major,
                EnrollDirection = stu.Enrollment == null ? "未录取" : stu.Enrollment.Direction.Title,
                WhichTime = stu.Enrollment == null ? -1 : stu.Enrollment.WhichTime,
                OrderInDirection = stu.Enrollment == null ? 0 : stu.Enrollment.ScoreOrder,
                EnrolledOrder = stu.Enrollment == null ? 0 : stu.DirectionStudents.FirstOrDefault(ds => ds.Direction.Title == stu.Enrollment.Direction.Title).Order,
                Scores = stu.Enrollment == null ? null : stu.Enrollment.EnrollCourses.Select(ec => new ScoreInfoViewModel
                {
                    ScoreName = ec.CourseName,
                    ScoreValue = ec.Scores.FirstOrDefault(s => s.Student.Id == stu.Id).Value ?? 0.0
                }).ToList(),
                Selections = stu.DirectionStudents.OrderBy(ds => ds.Order).Select(ds => new DirectionInfoViewModel
                {
                    Order = ds.Order,
                    DirectionName = ds.Direction.Title
                }).ToList()
            };
            reg_enroll_detail.TotalScore = reg_enroll_detail.Scores == null ? 0 : reg_enroll_detail.Scores.Sum(s => s.ScoreValue);
            return reg_enroll_detail;
        }

        //下载学生志愿填报情况（Excel）        
        [SuperCheck]
        public ActionResult DownloadData()
        {
            string tempPath = Server.MapPath("~/Content/DownloadFiles/temp.xls");
            string path = Server.MapPath("~/Content/DownloadFiles/" + (DateTime.Now.Year - 2) + "方向志愿-" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".xls");
            System.IO.File.Copy(tempPath, path);

            string connectionString = "Provider=Microsoft.Jet.OleDb.4.0; Data Source=" + path + "; Extended Properties=Excel 8.0;";
            StringBuilder stringBuilder1 = new StringBuilder("CREATE TABLE [学生名单$](学号 Char(100), 姓名 char(100),性别 char(20), 专业 char(160)");
            StringBuilder stringBuilder2 = new StringBuilder("INSERT INTO [学生名单$](学号, 姓名, 性别, 专业");
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

                List<RegEnrollDetailViewModel> registrations = new List<RegEnrollDetailViewModel>();
                int dirCount = db.Directions.Count();

                var students = db.Students.OrderBy(s => s.DirectionStudents.Count).ToList();
                foreach(var student in students)
                {
                    var reg = GetStudentRegEnroll(student);
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
                    if (s.Selections.Count != 0)
                    {
                        foreach (var d in s.Selections)
                        {
                            cmdInsert.Parameters.Add(new OleDbParameter("@want" + d.Order.ToString(), d.DirectionName));
                        }
                    }
                    else
                    {
                        for(int i = 0; i < db.Directions.Count(); i++)
                        {
                            cmdInsert.Parameters.Add(new OleDbParameter("@want" + i.ToString(), "未填报"));
                        }
                    }
                    cmdInsert.ExecuteNonQuery();
                }
                connection.Close();
            }
            return File(path, "application/vnd.ms-excel", Path.GetFileName(path));
        }

        //设置截止日期
        [SuperCheck]
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
                deadlineStr = $"{deadline.Year}-{deadline.Month}-{deadline.Day} {deadline.Hour} : {deadline.Minute}";
            }
            else
            {
                deadlineStr = "0000-00-00";
            }
            return PartialView("Setting", deadlineStr);
        }

        [HttpPost]
        [SuperCheck]
        public ActionResult Setting(string deadline)
        {
            DateTime deadlineDt = DateTime.Now;
            bool b = DateTime.TryParse(deadline, out deadlineDt);
            var dl = db.ServerConfigurations.FirstOrDefault();
            if (b && dl != null)
            {
                dl.Deadline = new DateTime(deadlineDt.Year, deadlineDt.Month, deadlineDt.Day, deadlineDt.Hour, deadlineDt.Minute, 0);
                db.SaveChanges();

                return Json(new { code = 0, data = "设置成功" });
            }
            return Json(new { code = 1, data = "设置失败" });
        }

        /// <summary>
        /// 第一、二志愿录取前初始化，清空第一、二志愿录取信息。
        /// 指定whichTime参数可以清空特定批次的录取信息。
        /// </summary>
        private void IntialEnroll(int? whichTime)
        {
            List<Enrollment> enrolls = null;
            if (whichTime == null)
            {
                enrolls = db.Enrollments.Where(en => en.WhichTime != 0).ToList();
            }
            else
            {
                enrolls = db.Enrollments.Where(en => en.WhichTime == whichTime).ToList();
            }
             
            foreach(var enroll in enrolls)
            {
                var courses = enroll.EnrollCourses;
                foreach(var c in courses)
                {
                    c.Enrollments.Remove(enroll);
                }
                db.Enrollments.Remove(enroll);
            }
            db.SaveChanges();
        }

        /**
         * 通过两轮志愿进行录取
         * 第一轮：通过方向指定的签到课程成绩，仅录取第一志愿。
         * 第二轮：剩下的学生通过指定的专业课成绩进行本轮录取。
         * 两轮录取都采用志愿优先、再成绩优先、录取名额限制的规则进行。
         */
        private List<GenerationResultModel> Generate()
        {
            IntialEnroll(null);

            List<GenerationResultModel> result = db.Students.Where(stu=>stu.Enrollment.WhichTime!=0).Select(stu => new GenerationResultModel
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
                Count = d.Enrollments.Count, //本轮录取的初始数量为提前批录取后的人数。
                Courses = d.DirectionCourses.Select(dc => new DirectionCourseInfoModel
                {
                    CourseId = dc.Course.Id,
                    CourseName = dc.Course.CourseName,
                    Proportion = dc.Proportion
                }).ToList()
            }).ToList();

            //按方向依次进行录取
            foreach (var direction in directions)
            {
                //找出所有选择该方向为第一志愿的学生信息，并计算其考核课程以及加分项的总成绩。
                var thisDirectionStudents = result.Where(s => {
                    bool r = false;
                    if (s.Selections.Count != 0)
                    {
                        var dir = s.Selections.SingleOrDefault(d => d.Order == 1);
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
                                    else
                                    {
                                        thisStuDirectionScores.Add(new ScoreInfoViewModel
                                        {
                                            ScoreName = dc.CourseName,
                                            ScoreValue = 0
                                        });
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
                                foreach (var item in thisStuDirectionScores)
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
                            var m_student = db.Students.SingleOrDefault(s => s.Id == tds.StudentScore.StudentId);
                            var m_direction = db.Directions.SingleOrDefault(d => d.Id == direction.DirectionId);
                            var enrollment = new Enrollment
                            {
                                Direction = m_direction,
                                Student = m_student,
                                ScoreOrder = so,
                                WhichTime = 1,
                                EnrollCourses = new List<Course>()
                            };
                            foreach (var cname in tds.EnrollmentCourses)
                            {
                                var course = db.Courses.SingleOrDefault(c => c.CourseName == cname);
                                enrollment.EnrollCourses.Add(course);
                            }

                            if (m_student.Enrollment == null)
                            {
                                db.Enrollments.Add(enrollment);
                            }
                            else
                            {
                                m_student.Enrollment.Direction = m_direction;
                                m_student.Enrollment.ScoreOrder = so;
                                m_student.Enrollment.WhichTime = 1;
                            }
                            db.SaveChanges(); //保存第一批次录取结果。
                        }
                    }
                }
            }

            /***第二轮录取，除去第一轮已经录取的学生，剩下学生按照指定专业基础课成绩进行拉通录取，包括第一轮中为录取满员的方向。***/
            //获取还未被录取的学生
            var db_restStudents = db.Students.Where(s => s.Enrollment.WhichTime != 0 && s.Enrollment.WhichTime != 1).ToList();
            var restStudents = new List<GenerationResultModel>();
            foreach(var drs in db_restStudents)
            {
                var item = result.SingleOrDefault(r => r.StudentScore.StudentId == drs.Id);
                if (item != null)
                {
                    restStudents.Add(item);
                }
            }
             
            //获取专业基础课信息
            var basicCoureses = db.Courses.Where(c => c.DirectionCourse.Count() == 0).ToList();

            foreach (var r in restStudents)
            {
                r.EnrollmentCourses.Clear();
                List<ScoreInfoViewModel> thisStuBasicScores = new List<ScoreInfoViewModel>();
                foreach (var course in basicCoureses.ToArray())
                {
                    var score = r.StudentScore.Scores.SingleOrDefault(sc => sc.ScoreName == course.CourseName);
                    if (score != null)
                    {
                        thisStuBasicScores.Add(score);
                    }
                }
                foreach (var item in thisStuBasicScores) //*包括了具有加分的学生成绩
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
                        var m_direction = db.Directions.SingleOrDefault(d => d.Id == dir.DirectionId);
                        var m_student = db.Students.SingleOrDefault(s => s.Id == stu.StudentScore.StudentId);
                        var enrollment = new Enrollment
                        {
                            Direction = m_direction,
                            Student = m_student,
                            ScoreOrder = bso,
                            WhichTime = 2,
                            EnrollCourses = new List<Course>()
                        };
                        foreach (var cname in stu.EnrollmentCourses)
                        {
                            var course = db.Courses.SingleOrDefault(c => c.CourseName == cname);
                            if (course != null)
                            {
                                enrollment.EnrollCourses.Add(course);
                            }
                        }

                        if (m_student.Enrollment == null)
                        {
                            db.Enrollments.Add(enrollment);
                        }
                        else
                        {
                            m_student.Enrollment.Direction = m_direction;
                            m_student.Enrollment.ScoreOrder = bso;
                            m_student.Enrollment.WhichTime = 2;
                        }
                        int ok = db.SaveChanges();
                        break;
                    }
                }
            }
            return result;
        }
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