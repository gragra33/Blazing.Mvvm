using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// ViewModel for demonstrating usage of ObservableValidator in the sample app.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class ObservableValidatorPageViewModel : SamplePageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableValidatorPageViewModel"/> class.
    /// </summary>
    /// <param name="filesService">The service for file operations.</param>
    public ObservableValidatorPageViewModel(IFilesService filesService) : base(filesService)
    {
    }
}
