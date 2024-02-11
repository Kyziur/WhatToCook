namespace WhatToCook.Application.Domain;

public class User
{
    public int Id { get; private set; }
    public required string Email { get; set; }
}
