using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class MessengerSendPageViewModel : MessengerPageViewModel
{
    public MessengerSendPageViewModel(IFilesService filesService) : base(filesService)
    {
        /* skip */
    }
}
