﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectionRegistration.Models
{
    public class RegistrationViewModel
    {
        public RegistrationViewModel()
        {
            Selections = new List<DirectionInfoViewModel>();
        }
        public int Id { get; set; }
        public string Number { get; set; }
        public string Gender { get; set; }
        public string Name { get; set; }
        public string Major { get; set; }
        public List<DirectionInfoViewModel> Selections { get; set; }
    }
}