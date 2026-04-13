using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// ViewModel for demonstrating Messenger send pattern in the sample app.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class MessengerSendPageViewModel : MessengerPageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessengerSendPageViewModel"/> class.
    /// </summary>
    /// <param name="filesService">The service for file operations.</param>
    public MessengerSendPageViewModel(IFilesService filesService) : base(filesService)
    {
        /* skip */
    }
}
