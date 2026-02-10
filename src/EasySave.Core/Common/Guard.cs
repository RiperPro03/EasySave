namespace EasySave.Core.Common;

/// <summary>
/// Provides guard clauses for argument validation.
/// It serves as a safety barrier to prevent the creation of objects with invalid data.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Ensures a string is not null, empty, or whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The parameter name used in exception messages.</param>
    /// <returns>The original non-null string.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is null, empty, or whitespace.
    /// </exception>
    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null, empty or whitespace.", paramName);

        return value;
    }
}