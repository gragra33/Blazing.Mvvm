using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// ViewModel for demonstrating Messenger request/response pattern in the sample app.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class MessengerRequestPageViewModel : MessengerPageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessengerRequestPageViewModel"/> class.
    /// </summary>
    /// <param name="filesService">The service for file operations.</param>
    public MessengerRequestPageViewModel(IFilesService filesService) : base(filesService)
    {
        /* skip */
    }
}
