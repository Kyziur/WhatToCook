﻿namespace WhatToCook.Application.DataTransferObjects.Requests
{
    public class RecipeRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Ingredients { get; set; }
        public string PreparationDescription { get; set; }
        public string TimeToPrepare { get; set; }
        public string Image { get; set; }
    }
}