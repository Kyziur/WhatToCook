using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatToCook.Application.Domain
{
    public class RecipeTag
    {
        public int Id { get; private set; }
        public Tag Tags { get;  set; } = new Tag ();
        public Recipe Recipe { get;  set; }
    }
}
