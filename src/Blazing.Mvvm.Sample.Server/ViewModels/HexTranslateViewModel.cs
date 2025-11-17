using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.Server.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

[ViewModelDefinition(Key = nameof(HexTranslateViewModel))]
public sealed class HexTranslateViewModel : ViewModelBase
{
    private readonly IMessenger _messenger;

    public HexTranslateViewModel(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public void ResetChildInputs()
        => _messenger.Send(new ResetHexAsciiInputsMessage());
}
