using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Repository.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public bool IsSuper { get; set; }
        public virtual Direction Direction { get; set; }
    }
}