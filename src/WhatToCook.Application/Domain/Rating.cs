using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatToCook.Application.Domain
{
    public class Rating
    {
        public int Id { get; private set; }
        public int Score { get; set; }
 
        public User User { get; set; }
        public Recipe Recipe { get; set; }

    }
}
