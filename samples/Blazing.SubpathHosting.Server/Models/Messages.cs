namespace Blazing.SubpathHosting.Server.Models;

public record ConvertHexToAsciiMessage(string HexToConvert);
public record ConvertAsciiToHexMessage(string AsciiToConvert);
public record ResetHexAsciiInputsMessage;
