namespace Blazing.Mvvm.Sample.Wasm.Models;

public record User
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
}
