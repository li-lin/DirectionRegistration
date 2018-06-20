using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Models
{
    public class PagedStudentsViewModel
    {
        public int PageCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchName { get; set; }
        public string SearchMajor { get; set; }
        public string SearchGender { get; set; }
        public string SearchNumber { get; set; }
    }
}