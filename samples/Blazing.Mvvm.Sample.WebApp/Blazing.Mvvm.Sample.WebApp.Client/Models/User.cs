namespace Blazing.Mvvm.Sample.WebApp.Client.Models;

public record User
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
}
