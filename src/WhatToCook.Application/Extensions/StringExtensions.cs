namespace WhatToCook.Application.Extensions;
public static class StringExtensions
{
    public static bool IsEmpty(this string text)
    {
        return string.IsNullOrWhiteSpace(text);
    }

    public static bool IsNotEmpty(this string text)
    {
        return !text.IsEmpty();
    }

    public static bool EqualsIgnoreCase(this string text, string secondText)
    {
        return text.Equals(secondText, StringComparison.OrdinalIgnoreCase);
    }
}
