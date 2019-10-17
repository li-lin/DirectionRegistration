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
            //Database.SetInitializer(new ProductionLevelInitialization());  
            Database.SetInitializer(new RegistrationDbInitialization());            
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Direction> Directions { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<DirectionStudent> DirectionStudents { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<DirectionCourse> DirectionCourses { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<ServerConfig> ServerConfigurations { get; set; }
    }
}
