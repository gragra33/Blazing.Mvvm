using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// ViewModel for the collections page, providing logic and data for collections-related UI.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class CollectionsPageViewModel : SamplePageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionsPageViewModel"/> class.
    /// </summary>
    /// <param name="filesService">The service for file operations.</param>
    public CollectionsPageViewModel(IFilesService filesService) : base(filesService)
    {
    }
}
