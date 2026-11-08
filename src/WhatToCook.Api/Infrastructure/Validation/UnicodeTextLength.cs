using System.Text;

namespace WhatToCook.Api.Infrastructure.Validation;

/// <summary>Measures text length in Unicode scalar values, matching PostgreSQL varchar(n) character limits.</summary>
internal static class UnicodeTextLength
{
    public static bool Exceeds(string? value, int maxLength, bool trim = true)
    {
        if (value is null)
        {
            return false;
        }

        var span = value.AsSpan();
        if (trim)
        {
            span = span.Trim();
        }

        var scalarCount = 0;
        foreach (var _ in span.EnumerateRunes())
        {
            if (++scalarCount > maxLength)
            {
                return true;
            }
        }

        return false;
    }
}
