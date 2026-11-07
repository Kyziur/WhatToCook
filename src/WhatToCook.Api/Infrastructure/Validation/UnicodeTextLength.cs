using System.Text;

namespace WhatToCook.Api.Infrastructure.Validation;

/// <summary>
/// Applies PostgreSQL varchar character limits as Unicode scalar-value limits.
/// </summary>
public static class UnicodeTextLength
{
    public static bool ExceedsMax(string? value, int maximum, bool trim = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximum);

        if (value is null)
        {
            return false;
        }

        ReadOnlySpan<char> text = value.AsSpan();
        if (trim)
        {
            text = text.Trim();
        }

        var count = 0;
        foreach (var _ in text.EnumerateRunes())
        {
            if (++count > maximum)
            {
                return true;
            }
        }

        return false;
    }
}
