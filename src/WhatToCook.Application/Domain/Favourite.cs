namespace WhatToCook.Application.Domain
{
    public class Favourite
    {
        protected Favourite() { }
        public Favourite(Recipe recipe, User user)
        {
            Recipe = recipe;
            RecipeId = recipe.Id;
            User = user;
            UserId = user.Id;
        }

        public int Id { get; private set; }
        public int RecipeId { get; private set; }
        public Recipe Recipe { get; private set; }
        public int UserId { get; private set; }
        public User User { get; private set; }
    }
}
