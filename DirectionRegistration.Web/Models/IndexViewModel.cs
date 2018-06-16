using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DirectionRegistration.Repository.Entities;

namespace DirectionRegistration.Models
{
    public class IndexViewModel
    {
        public IndexViewModel()
        {
            Directions = new List<DirectionInfoViewModel>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public List<DirectionInfoViewModel> Directions { get; set; }
    }
}