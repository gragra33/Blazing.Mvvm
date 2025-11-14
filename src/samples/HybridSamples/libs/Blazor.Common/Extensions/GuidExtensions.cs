namespace System;

/// <summary>
/// Provides extension methods for <see cref="Guid"/> to convert to and from short and ID string representations.
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    /// Converts a <see cref="Guid"/> to a short string representation using base64 encoding.
    /// </summary>
    /// <param name="guid">The <see cref="Guid"/> to convert.</param>
    /// <returns>A short string representation of the <see cref="Guid"/>.</returns>
    public static string ToShortString(this Guid guid)
        => Convert.ToBase64String(guid.ToByteArray()).Replace('+', '-').Replace('/', '_')[..22];

    /// <summary>
    /// Converts a short string representation back to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="str">The short string to convert.</param>
    /// <returns>The <see cref="Guid"/> represented by the string.</returns>
    public static Guid FromShortString(this string str)
        => new(Convert.FromBase64String(str.Replace('_', '/').Replace('-', '+') + "=="));

    /// <summary>
    /// Converts a <see cref="Guid"/> to an ID string representation prefixed with "id_".
    /// </summary>
    /// <param name="guid">The <see cref="Guid"/> to convert.</param>
    /// <returns>An ID string representation of the <see cref="Guid"/>.</returns>
    public static string ToIdString(this Guid guid)
        => "id_" + Convert.ToBase64String(guid.ToByteArray()).Replace('+', '-').Replace('/', '_')[..22];
}
