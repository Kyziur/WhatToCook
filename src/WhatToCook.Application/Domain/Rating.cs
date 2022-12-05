namespace WhatToCook.Application.Domain;

public class Rating
{
    public int Id { get; private set; }
    public int Score { get; set; }

    public User User { get; set; }
    public Recipe Recipe { get; set; }

}
