using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DirectionRegistration.Repository;
using DirectionRegistration.Repository.Entities;

namespace DirectionRegistration.Web.Helper
{
    public class CheckSuperHelper
    {
        public static bool IsSuper(string token)
        {
            bool b = false;
            using (var db = new RegistrationDbContext())
            {
                var teacher = db.Teachers.SingleOrDefault(t => t.LoginName == token);
                if (teacher != null)
                {
                    b = teacher.IsSuper;
                }
            }
            return b;
        }
    }
}