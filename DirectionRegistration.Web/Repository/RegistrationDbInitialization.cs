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

            var config = new ServerConfig()
            {
                Deadline = new DateTime(2018, 7, 30)
            };
            context.ServerConfigurations.Add(config);

            context.SaveChanges();
        }
    }
}
