namespace WhatToCook.Application.Domain;

public class Rating
{
    public int Id { get; private set; }
    public int Score { get; set; }
    public User User { get; set; }
    public Recipe Recipe { get; set; }

    public Rating(int score, User user, Recipe recipe)
    {
        Score = score;
        User = user;
        Recipe = recipe;
    }
    private Rating()
    {      
    }
}
