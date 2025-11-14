namespace System;

public static class GuidExtensions
{
    public static string ToShortString(this Guid guid)
        => Convert.ToBase64String(guid.ToByteArray()).Replace('+', '-').Replace('/', '_')[..22];

    public static Guid FromShortString(this string str)
        => new(Convert.FromBase64String(str.Replace('_', '/').Replace('-', '+') + "=="));

    public static string ToIdString(this Guid guid)
        => "id_" + Convert.ToBase64String(guid.ToByteArray()).Replace('+', '-').Replace('/', '_')[..22];
}
