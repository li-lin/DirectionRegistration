using DirectionRegistration.Repository.Entities;
using DirectionRegistration.Repository;
using DirectionRegistration.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.IO;
using System.Web.Mvc;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace DirectionRegistration.Web.Controllers
{
    public class ScoreController : Controller
    {
        private readonly RegistrationDbContext db = new RegistrationDbContext();

        private string CreateExcelFile()
        {
            string path = Server.MapPath("~/Content/DownloadFiles/" + (DateTime.Now.Year - 2) + "录取结果-" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".xls");
            List<string> directionNames = db.Directions.Select(d => d.Title).ToList();

            HSSFWorkbook book = new HSSFWorkbook();
            foreach(var dn in directionNames)
            {
                Sheet sheet = book.CreateSheet(dn);
            }
            FileStream file = new FileStream(path, FileMode.Create);
            book.Write(file);
            file.Close();
            return path;
        }

        /// <summary>
        /// 生成录取结果的Excel文件
        /// </summary>
        /// <returns></returns>
        public ActionResult GenerateResult()
        {
            string path = CreateExcelFile();

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
                List<EnrollmentExportModel> enrollments = GetEnrollmentViewModels();

                foreach (var r in enrollments)
                {
                    if (r.DirectionName == null) continue;
                    StringBuilder sb1 = new StringBuilder("INSERT INTO[" + r.DirectionName + 
                        "$](学号, 姓名, 性别, 专业, 录取方向, 志愿顺序, 录取批次, 成绩排名, 总成绩");
                    StringBuilder sb2 = new StringBuilder(" VALUES(@number,@name,@gender,@major,@direction,@order,@time,@scoreOrder,@total");

                    //var theDirection = directions.SingleOrDefault(d => d.Title == r.DirectionName);
                    //int c_count = theDirection.DirectionCourses.Count;
                    var scores = r.Scores;
                    for (int i = 0; i < scores.Count; i++)
                    {

                        sb1.Append($", 课程{i + 1}");
                        sb1.Append($", 成绩{i + 1}");
                        sb2.Append($", @course{i + 1}");
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

                        for (int i = 0; i < scores.Count; i++)
                        {
                            var scoreValue = scores[i].ScoreValue;
                            var courseName = scores[i].ScoreName;
                            cmdInsert.Parameters.Add(new OleDbParameter($"@course{i + 1}", courseName));
                            cmdInsert.Parameters.Add(new OleDbParameter($"@score{i + 1}", scoreValue));
                        }
                        cmdInsert.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            return Content($"/Content/DownloadFiles/{Path.GetFileName(path)}");
        }       

        /// <summary>
        /// 获取录取信息列表
        /// </summary>
        /// <returns></returns>
        private List<EnrollmentExportModel> GetEnrollmentViewModels()
        {
            List<EnrollmentExportModel> enrollments = db.Enrollments.Select(en => new EnrollmentExportModel
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
                DirectionOrder = en.Student.DirectionStudents.FirstOrDefault(d=>d.Direction.Title==en.Direction.Title).Order,
                EnrollTime = en.WhichTime,
                ScoreOrder = en.ScoreOrder,
                Scores = en.EnrollCourses.Select(ec => new ScoreInfoViewModel
                {
                    ScoreName = ec.CourseName,
                    ScoreValue = ec.Scores.FirstOrDefault(s => s.Student.Id == en.Student.Id).Value ?? 0.0
                }).ToList()
            }).ToList();

            foreach (var enroll in enrollments)
            {
                enroll.ScoreTotal = enroll.Scores.Sum(s => s.ScoreValue);
            }
            return enrollments;
        }                      
    }
}
