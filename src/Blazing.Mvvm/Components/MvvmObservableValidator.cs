using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Adds validation support to an <see cref="EditContext"/> for models that inherit from <see cref="ObservableValidator"/>.
/// <para>
/// However, falls back to the default behaviour and uses the built-in <see cref="DataAnnotationsValidator"/> for models that do not inherit from <see cref="ObservableValidator"/>.
/// </para>
/// </summary>
public sealed class MvvmObservableValidator : ComponentBase, IDisposable
{
    private IDisposable? _handler;
    private EditContext? _originalEditContext;

    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <inheritdoc />
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

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (CurrentEditContext == _originalEditContext)
        {
            return;
        }

        throw new InvalidOperationException($"{GetType()} does not support changing the {nameof(EditContext)} dynamically.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _handler?.Dispose();
        _handler = null;
    }
}
