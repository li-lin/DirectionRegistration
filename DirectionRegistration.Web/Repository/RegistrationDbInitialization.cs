﻿using System;
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
            base.Seed(context);
            var admins = new List<Teacher>{
                new Teacher{
                    LoginName="JC001",
                    Name="李小林",
                    IsSuper=true,
                    Password="123456"
                },
                new Teacher
                {
                    LoginName="JC002",
                    Name="林小李",
                    IsSuper=false,
                    Password="123456"
                }
            };
            admins.ForEach(a => context.Teachers.Add(a));

            var students = new List<Student>
            {
                new Student
                {
                    Number="2016001",
                    Name="王小二",
                    Gender="男",
                     Major="软件工程",
                     Password="123456"
                },
                new Student
                {
                    Number="2016002",
                    Name="王小四",
                    Gender="男",
                     Major="计算机类",
                     Password="123456"
                },
                new Student
                {
                    Number="2016003",
                    Name="王小六",
                    Gender="男",
                     Major="电子商务",
                     Password="123456"
                },new Student
                {
                    Number="2016004",
                    Name="王小八",
                    Gender="男",
                     Major="信息管理",
                     Password="123456"
                }
            };
            students.ForEach(s =>
            {
                context.Students.Add(s);
            });

            var config = new ServerConfig()
            {
                Deadline = DateTime.Now.AddDays(30),
                EnrollmentState = 0
            };
            context.ServerConfigurations.Add(config);

            context.SaveChanges();
        }
    }

    //发布版本初始化器
    public class ProductionLevelInitialization : IDatabaseInitializer<RegistrationDbContext>
    {
        public void InitializeDatabase(RegistrationDbContext context)
        {
            var super = context.Teachers.SingleOrDefault(t => t.LoginName == "jcsuper");
            if (super == null)
            {
                var admins = new List<Teacher>{
                    new Teacher{
                        LoginName="jcsuper",
                        Password="13141516",
                        Name="管理员",
                        IsSuper=true
                    }
                };
                admins.ForEach(a => context.Teachers.Add(a));

                var config = new ServerConfig()
                {
                    Deadline = DateTime.Now.AddDays(30),
                    EnrollmentState = 0
                };
                context.ServerConfigurations.Add(config);

                context.SaveChanges();
            }
        }
    }
}
