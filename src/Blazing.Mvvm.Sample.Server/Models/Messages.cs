namespace Blazing.Mvvm.Sample.Server.Models;

public record ConvertHexToAsciiMessage(string HexToConvert);
public record ConvertAsciiToHexMessage(string AsciiToConvert);
public record ResetHexAsciiInputsMessage;
