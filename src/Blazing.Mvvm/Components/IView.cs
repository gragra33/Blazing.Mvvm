namespace Blazing.Mvvm.Components;

/// <summary>
/// Represents a View in the MVVM pattern.
/// This interface is used as a marker for Blazor components or pages that act as Views and participate in MVVM navigation and lifecycle management.
/// </summary>
/// <remarks>
/// Implementing this interface allows a component to be recognized as a View for navigation and disposal purposes.
/// </remarks>
public interface IView : IDisposable
{
    // This interface is used for type safety and navigation detection. No members are defined.
}