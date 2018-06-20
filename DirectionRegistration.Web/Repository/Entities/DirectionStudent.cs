using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Repository.Entities
{
    public class DirectionStudent
    {
        public int Id { get; set; }
        public virtual Student Student { get; set; }
        public virtual Direction Direction { get; set; }
        public int Order { get; set; }
    }
}