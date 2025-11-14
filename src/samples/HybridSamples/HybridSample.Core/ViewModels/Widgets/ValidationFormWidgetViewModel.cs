using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HybridSample.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace HybridSample.Core.ViewModels.Widgets;

/// <summary>
/// A viewmodel for the validation widget.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class ValidationFormWidgetViewModel : ValidatorViewModelBase
{
    private readonly IDialogService dialogService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFormWidgetViewModel"/> class.
    /// </summary>
    /// <param name="dialogService">The dialog service for showing messages.</param>
    public ValidationFormWidgetViewModel(IDialogService dialogService)
    {
        this.dialogService = dialogService;
    }

    /// <summary>
    /// Occurs when the form submission is completed successfully.
    /// </summary>
    public event EventHandler? FormSubmissionCompleted;

    /// <summary>
    /// Occurs when the form submission fails validation.
    /// </summary>
    public event EventHandler? FormSubmissionFailed;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    [ObservableProperty]
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    private string? firstName;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    [ObservableProperty]
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    private string? lastName;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [ObservableProperty]
    [Required]
    [EmailAddress]
    private string? email;

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    [ObservableProperty]
    [Required]
    [Phone]
    private string? phoneNumber;

    [RelayCommand]
    private void Submit()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            FormSubmissionFailed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void ShowErrors()
    {
        string message = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));

        _ = dialogService.ShowMessageDialogAsync("Validation errors", message);
    }
}
