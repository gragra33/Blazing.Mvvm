using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazing.Mvvm.Components;

/// <summary>
/// A Blazor component that adds validation support to an <see cref="EditContext"/> for models inheriting from <see cref="ObservableValidator"/>.
/// Falls back to the default behaviour and uses the built-in <see cref="DataAnnotationsValidator"/> for models that do not inherit from <see cref="ObservableValidator"/>.
/// </summary>
public sealed class MvvmObservableValidator : ComponentBase, IDisposable
{
    private IDisposable? _handler;
    private EditContext? _originalEditContext;

    /// <summary>
    /// Gets or sets the current <see cref="EditContext"/> from the cascading parameter.
    /// </summary>
    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    /// <summary>
    /// Gets or sets the service provider for dependency injection.
    /// </summary>
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;

    /// <summary>
    /// Initializes the component and sets up validation for the <see cref="EditContext"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="CurrentEditContext"/> is <c>null</c>.</exception>
    protected override void OnInitialized()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException($"{nameof(MvvmObservableValidator)} requires a cascading " +
                $"parameter of type {nameof(EditContext)}. For example, you can use {nameof(MvvmObservableValidator)} inside an EditForm.");
        }

        _handler = CurrentEditContext.CanHandleEditContextModel()
            ? CurrentEditContext.EnableMvvmObservableValidation()
            : CurrentEditContext.EnableDataAnnotationsValidation(ServiceProvider); // Fallback to the default behaviour and use the built-in DataAnnotationsValidator

        _originalEditContext = CurrentEditContext;
    }

    /// <summary>
    /// Checks if the <see cref="EditContext"/> has changed and throws if it has, as dynamic changes are not supported.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="EditContext"/> is changed dynamically.</exception>
    protected override void OnParametersSet()
    {
        if (CurrentEditContext == _originalEditContext)
        {
            return;
        }

        throw new InvalidOperationException($"{GetType()} does not support changing the {nameof(EditContext)} dynamically.");
    }

    /// <summary>
    /// Disposes the validation handler and releases resources.
    /// </summary>
    public void Dispose()
    {
        _handler?.Dispose();
        _handler = null;
    }
}
