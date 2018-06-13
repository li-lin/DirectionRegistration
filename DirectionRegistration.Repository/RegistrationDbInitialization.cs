using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using DirectionRegistration.Repository.Entities;

namespace DirectionRegistration.Repository
{
    public class RegistrationDbInitialization : DropCreateDatabaseIfModelChanges<RegistrationDbContext>
    {
        protected override void Seed(RegistrationDbContext context)
        {
            context.Students.AddRange(new List<Student>{
                new Student
            {
                Number = "140210106",
                Name = "谭晓雪",
                Password = "123456"
            },
             new Student
            {
                Number = "140210113",
                Name = "李双海",
                Password = "123456"
            },
             new Student
            {
                Number = "140210114",
                Name = "詹俞堃",
                Password = "123456"
            },
             new Student
            {
                Number = "140210116",
                Name = "杨帆",
                Password = "123456"
            },
             new Student
            {
                Number = "140210136",
                Name = "万颖",
                Password = "123456"
            }
            });
            context.Directions.AddRange(new List<Direction>{
                new Direction{
                    Title=".NET"
                },
                new Direction{
                    Title="J2EE"
                },
                new Direction{
                    Title="UI"
                },
                new Direction{
                    Title="软件测试"
                },
                new Direction{
                    Title="C++"
                },
                new Direction{
                    Title="IT商务"
                }
            });
            context.SaveChanges();
        }
    }
}
