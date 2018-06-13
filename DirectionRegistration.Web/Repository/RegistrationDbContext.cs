using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using DirectionRegistration.Repository.Entities;

namespace DirectionRegistration.Repository
{
    public class RegistrationDbContext : DbContext
    {
        public RegistrationDbContext()
        {
            Database.SetInitializer(new RegistrationDbInitialization());
            //if (this.Admins.SingleOrDefault(a => a.LoginName == "jc_super") == null)
            //{
            //    var admins = new List<Teacher>{
            //        new Teacher{
            //            LoginName="jc_super",
            //            Password="998998"
            //        }
            //    };
            //    admins.ForEach(a => this.Admins.Add(a));
            //    this.SaveChanges();
            //}
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Direction> Directions { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<DirectionStudent> DirectionStudents { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<DirectionCourse> DirectionCourses { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<ServerConfig> ServerConfigurations { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Student>().Property(s => s.Number).IsRequired();
        //    modelBuilder.Entity<Student>().Property(s => s.Password).IsRequired();
        //    modelBuilder.Entity<Student>().Property(s => s.Name).IsRequired();
        //    modelBuilder.Entity<Student>().HasOptional(s => s.FirstSelection).WithMany(d => d.FirstStudents);
        //    modelBuilder.Entity<Student>().HasOptional(s => s.SecondSelection).WithMany(d => d.SecondStudents);
        //    modelBuilder.Entity<Student>().HasOptional(s => s.ThirdSelection).WithMany(d => d.ThirdStudents);
        //    modelBuilder.Entity<Direction>().Property(d => d.Title).IsRequired();
        //}
    }
}
