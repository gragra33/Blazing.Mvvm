using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.WebApp.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.Sample.WebApp.Client.ViewModels;

public partial class HexEntryViewModel : RecipientViewModelBase<ConvertAsciiToHexMessage>
{
    [ObservableProperty]
    private string? _hexText;

    public override void Receive(ConvertAsciiToHexMessage message)
    {
        char[] charArray = message.AsciiToConvert.ToCharArray();
        string hexOutput = string.Empty;
        foreach (char @char in charArray)
        {
            hexOutput += $"{Convert.ToInt32(@char):X}";
        }

        HexText = hexOutput;
    }

    public override Task Loaded()
    {
        IsActive = true;
        return base.Loaded();
    }

    [RelayCommand]
    public virtual void SendToAsciiConverter()
    {
        Messenger.Send(new ConvertHexToAsciiMessage(HexText ?? string.Empty));
    }
}