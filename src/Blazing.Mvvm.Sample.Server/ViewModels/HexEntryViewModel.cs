using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.Server.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

public sealed partial class HexEntryViewModel : RecipientViewModelBase<ConvertAsciiToHexMessage>, IRecipient<ResetHexAsciiInputsMessage>
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendToAsciiConverterCommand))]
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

    public void Receive(ResetHexAsciiInputsMessage _)
        => HexText = string.Empty;

    [RelayCommand(CanExecute = nameof(CanSendToAsciiConverter))]
    private void SendToAsciiConverter()
        => Messenger.Send(new ConvertHexToAsciiMessage(HexText!));

    private bool CanSendToAsciiConverter()
        => !string.IsNullOrWhiteSpace(HexText);
}
