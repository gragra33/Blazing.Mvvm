namespace Blazing.Mvvm.Sample.WebApp.Client.Models;

public record ConvertHexToAsciiMessage(string HexToConvert);
public record ConvertAsciiToHexMessage(string AsciiToConvert);
public record ResetHexAsciiInputsMessage;
