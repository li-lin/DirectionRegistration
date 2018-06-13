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
        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Direction> Directions { get; set; }
    }
}
