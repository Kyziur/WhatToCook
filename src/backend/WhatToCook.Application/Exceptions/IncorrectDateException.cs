namespace WhatToCook.Application.Exceptions;

public class IncorrectDateException : Exception
{
    public IncorrectDateException() : base()
    {
    }
    public IncorrectDateException(string message) : base(message)
    {
    }
    public IncorrectDateException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
