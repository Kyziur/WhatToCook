namespace WhatToCook.Application.Domain;

public class Tag
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public Tag(string name)
    {
        Name = name;
    }
}
