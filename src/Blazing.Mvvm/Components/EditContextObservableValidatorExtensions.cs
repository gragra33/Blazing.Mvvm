using Blazing.Mvvm.Components.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Provides extension methods for <see cref="EditContext"/> to enable MVVM validation support for models that inherit from <see cref="ObservableValidator"/>.
/// </summary>
public static class EditContextObservableValidatorExtensions
{
    /// <summary>
    /// Enables MVVM validation support for the specified <see cref="EditContext"/> when its model inherits from <see cref="ObservableValidator"/>.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> to enable validation for.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> whose disposal will remove MVVM validation support from the <see cref="EditContext"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="editContext"/> is <c>null</c>.</exception>
    public static IDisposable EnableMvvmObservableValidation(this EditContext editContext)
    {
        ArgumentNullException.ThrowIfNull(editContext);
        return new EditContextObservableValidatorHandler(editContext);
    }

    /// <summary>
    /// Determines whether the MVVM validation handler can handle the model of the specified <see cref="EditContext"/>.
    /// </summary>
    /// <param name="editContext">The <see cref="EditContext"/> whose model is to be checked.</param>
    /// <returns>
    /// <c>true</c> if the MVVM validation handler can handle the model of the <see cref="EditContext"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="editContext"/> is <c>null</c>.</exception>
    public static bool CanHandleEditContextModel(this EditContext editContext)
    {
        ArgumentNullException.ThrowIfNull(editContext);
        return EditContextObservableValidatorHandler.CanHandleType(editContext.Model.GetType());
    }
}
