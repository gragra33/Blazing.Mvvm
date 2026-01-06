namespace Blazing.Mvvm.Sample.HybridMaui.Models;

public record Post
{
    public required string Id { get; init; }
    public required string UserId { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required string Preview { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
