using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// ViewModel for demonstrating dependency injection (IoC) in the sample app.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class IocPageViewModel : SamplePageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IocPageViewModel"/> class.
    /// </summary>
    /// <param name="filesService">The service for file operations.</param>
    public IocPageViewModel(IFilesService filesService) : base(filesService)
    {
    }
}
