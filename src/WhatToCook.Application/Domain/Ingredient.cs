using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatToCook.Application.Domain
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Recipe Recipe { get; set; }
    }
}
