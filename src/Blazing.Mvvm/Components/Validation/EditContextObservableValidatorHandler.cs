using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazing.Mvvm.Components.Validation;

internal sealed class EditContextObservableValidatorHandler : IDisposable
{
    private const string ValidateAllPropertiesMethodName = "ValidateAllProperties";

    /// <summary>
    /// We use reflection to call the protected method <c>ObservableValidator.ValidateAllProperties</c>.
    /// This will be replaced in a future release by source generators where we expose the method publicly on types that implement <c>ObservableValidator</c>.
    /// </summary>
    private static readonly MethodInfo? ValidateAllPropertiesMethodInfo = typeof(ObservableValidator).GetMethod(ValidateAllPropertiesMethodName, BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly EditContext _editContext;
    private readonly ObservableValidator _model;
    private readonly ValidationMessageStore _validationMessageStore;

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
    /// This method is used to determine if the <see cref="EditContextObservableValidatorHandler"/> can handle the model that is being validated.
    /// </summary>
    public static bool CanHandleType(Type type)
        => type.IsAssignableTo(typeof(ObservableValidator)) && ValidateAllPropertiesMethodInfo is not null;

    public void Dispose()
    {
        _validationMessageStore.Clear();
        _editContext.OnValidationRequested -= OnValidationRequested;
        _model.ErrorsChanged -= OnErrorsChanged;
        _editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// We subscribe to the <see cref="ObservableValidator.ErrorsChanged"/> instead of the <see cref="EditContext.OnFieldChanged"/>
    /// because the validation is done in <see cref="_model">model</see> and we only want to update the UI when the errors change.
    /// </summary>
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
    /// We invoke the protected method <c>ObservableValidator.ValidateAllProperties</c> to validate all properties of the model,
    /// this triggers the <see cref="ObservableValidator.ErrorsChanged"/> for each property that has errors which is then handled by <see cref="OnErrorsChanged"/>.
    /// </summary>
    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
        => ValidateAllPropertiesMethodInfo!.Invoke(_model, null);
}
