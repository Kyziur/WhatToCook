﻿namespace WhatToCook.Application.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception exception) : base(message, exception)
    {
    }

    public DomainException() : base()
    {
    }
}