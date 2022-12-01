using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatToCook.Application.Domain
{
    public class PlanOfmeals
    {
        public int Id { get; private set; }
        public DateTime FromDateToDate { get; set; }
        public Recipe Recipe { get; set; }
        public User User { get; set; }
    }
}
