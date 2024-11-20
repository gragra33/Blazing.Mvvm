using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.HybridMaui.Models;

namespace Blazing.Mvvm.Sample.HybridMaui.ViewModels;

public partial class TextEntryViewModel : RecipientViewModelBase<ConvertHexToAsciiMessage>
{
    [ObservableProperty]
    private string? _asciiText;

    public override void Receive(ConvertHexToAsciiMessage message)
    {
        string? ascii = string.Empty;

        for (int i = 0; i < message.HexToConvert.Length; i += 2)
        {
            string? hs = message.HexToConvert.Substring(i, 2);
            uint decimalVal = Convert.ToUInt32(hs, 16);
            char character = Convert.ToChar(decimalVal);
            ascii += character;
        }

        AsciiText = ascii;
    }

    public override Task Loaded()
    {
        IsActive = true;
        return base.Loaded();
    }

    [RelayCommand]
    public virtual void SendToHexConverter()
    {
        Messenger.Send(new ConvertAsciiToHexMessage(AsciiText ?? string.Empty));
    }
}