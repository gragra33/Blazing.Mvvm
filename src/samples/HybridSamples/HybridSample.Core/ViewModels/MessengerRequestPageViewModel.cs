using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class MessengerRequestPageViewModel : MessengerPageViewModel
{
    public MessengerRequestPageViewModel(IFilesService filesService) : base(filesService)
    {
        /* skip */
    }
}
