namespace Blazing.Mvvm.Sample.Wasm.Models;

public record ConvertHexToAsciiMessage(string HexToConvert);
public record ConvertAsciiToHexMessage(string AsciiToConvert);
public record ResetHexAsciiInputsMessage();
