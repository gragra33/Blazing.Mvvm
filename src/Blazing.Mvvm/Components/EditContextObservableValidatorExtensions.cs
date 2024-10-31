using Blazing.Mvvm.Components.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Extensions for <see cref="EditContext"/> to enable MVVM validation support for models that inherit from <see cref="ObservableValidator"/>.
/// </summary>
public static class EditContextObservableValidatorExtensions
{
    /// <summary>
    /// Enables MVVM validation support for an <see cref="EditContext"/> for models that inherit from <see cref="ObservableValidator"/>.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/>.</param>
    /// <returns>A disposable object whose disposal will remove validation support from the <see cref="EditContext"/>.</returns>
    public static IDisposable EnableMvvmObservableValidation(this EditContext editContext)
    {
        ArgumentNullException.ThrowIfNull(editContext);
        return new EditContextObservableValidatorHandler(editContext);
    }

    /// <summary>
    /// Determines if the MVVM validation handler can handle the model of the <see cref="EditContext"/>.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/>.</param>
    /// <returns><c>true</c> if the MVVM validation handler can handle the model of the <see cref="EditContext"/>; otherwise, <c>false</c>.</returns>
    public static bool CanHandleEditContextModel(this EditContext editContext)
    {
        ArgumentNullException.ThrowIfNull(editContext);
        return EditContextObservableValidatorHandler.CanHandleType(editContext.Model.GetType());
    }
}
