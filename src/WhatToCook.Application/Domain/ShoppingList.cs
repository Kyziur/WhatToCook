namespace WhatToCook.Application.Domain;

public class ShoppingList
{
    public int Id { get; private set; }
    public Recipe Recipe { get; set; }
    public User User { get; set; }
}
