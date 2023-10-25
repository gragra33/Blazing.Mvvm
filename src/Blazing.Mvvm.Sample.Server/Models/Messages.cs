namespace Blazing.Mvvm.Sample.Server.Models;

public record class ConvertHexToAsciiMessage(string HexToConvert);
public record class ConvertAsciiToHexMessage(string AsciiToConvert);