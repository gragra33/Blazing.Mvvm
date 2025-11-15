using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazing.Mvvm.Components.Validation;

/// <summary>
/// Handles MVVM validation for an <see cref="EditContext"/> whose model inherits from <see cref="ObservableValidator"/>.
/// Subscribes to validation events and updates the Blazor UI with validation messages.
/// </summary>
internal sealed class EditContextObservableValidatorHandler : IDisposable
{
    /// <summary>
    /// The name of the protected method <c>ValidateAllProperties</c> on <see cref="ObservableValidator"/>.
    /// </summary>
    private const string ValidateAllPropertiesMethodName = "ValidateAllProperties";

    /// <summary>
    /// Uses reflection to call the protected method <c>ObservableValidator.ValidateAllProperties</c>.
    /// This will be replaced in a future release by source generators where the method is exposed publicly on types that implement <c>ObservableValidator</c>.
    /// </summary>
    private static readonly MethodInfo? ValidateAllPropertiesMethodInfo = typeof(ObservableValidator).GetMethod(ValidateAllPropertiesMethodName, BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly EditContext _editContext;
    private readonly ObservableValidator _model;
    private readonly ValidationMessageStore _validationMessageStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditContextObservableValidatorHandler"/> class and subscribes to validation events.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> to handle validation for. Its model must inherit from <see cref="ObservableValidator"/>.</param>
    /// <exception cref="InvalidOperationException">Thrown if the model does not inherit from <see cref="ObservableValidator"/> or lacks the required method.</exception>
    public EditContextObservableValidatorHandler(EditContext editContext)
    {
        if (!CanHandleType(editContext.Model.GetType()))
        {
            throw new InvalidOperationException($"{nameof(EditContextObservableValidatorHandler)} requires a model that inherits from {nameof(ObservableValidator)} and has a protected method named '{ValidateAllPropertiesMethodName}'.");
        }

        _editContext = editContext;
        _model = (ObservableValidator)editContext.Model;
        _validationMessageStore = new ValidationMessageStore(editContext);

        editContext.OnValidationRequested += OnValidationRequested;
        _model.ErrorsChanged += OnErrorsChanged;
    }

    /// <summary>
    /// Determines whether the <see cref="EditContextObservableValidatorHandler"/> can handle the specified model type for validation.
    /// </summary>
    /// <param name="type">The model type to check.</param>
    /// <returns><c>true</c> if the type inherits from <see cref="ObservableValidator"/> and has the required method; otherwise, <c>false</c>.</returns>
    public static bool CanHandleType(Type type)
        => type.IsAssignableTo(typeof(ObservableValidator)) && ValidateAllPropertiesMethodInfo is not null;

    /// <summary>
    /// Disposes the handler, unsubscribes from events, and clears validation messages.
    /// </summary>
    public void Dispose()
    {
        _validationMessageStore.Clear();
        _editContext.OnValidationRequested -= OnValidationRequested;
        _model.ErrorsChanged -= OnErrorsChanged;
        _editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Handles the <see cref="ObservableValidator.ErrorsChanged"/> event and updates validation messages for the affected property.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event data containing the property name.</param>
    private void OnErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
    {
        var fieldIdentifier = string.IsNullOrEmpty(e.PropertyName)
            ? new FieldIdentifier(_model, string.Empty)
            : _editContext.Field(e.PropertyName);

        _validationMessageStore.Clear(fieldIdentifier);

        foreach (var result in _model.GetErrors(e.PropertyName))
        {
            _validationMessageStore.Add(fieldIdentifier, result.ErrorMessage!);
        }

        _editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Handles the <see cref="EditContext.OnValidationRequested"/> event by invoking validation for all properties on the model.
    /// This triggers <see cref="ObservableValidator.ErrorsChanged"/> for each property with errors, which is then handled by <see cref="OnErrorsChanged"/>.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event data.</param>
    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
        => ValidateAllPropertiesMethodInfo!.Invoke(_model, null);
}
