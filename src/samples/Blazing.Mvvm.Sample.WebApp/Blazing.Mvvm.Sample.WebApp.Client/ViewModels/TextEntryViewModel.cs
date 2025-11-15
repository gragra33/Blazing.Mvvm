using System.Text;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.WebApp.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.Sample.WebApp.Client.ViewModels;

public sealed partial class TextEntryViewModel : RecipientViewModelBase<ConvertHexToAsciiMessage>, IRecipient<ResetHexAsciiInputsMessage>
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendToHexConverterCommand))]
    private string? _asciiText;

    public override void Receive(ConvertHexToAsciiMessage message)
    {
        StringBuilder asciiBuilder = new();

        for (int i = 0; i < message.HexToConvert.Length; i += 2)
        {
            string hs = message.HexToConvert.Substring(i, 2);
            uint decimalVal = Convert.ToUInt32(hs, 16);
            char character = Convert.ToChar(decimalVal);
            asciiBuilder.Append(character);
        }

        AsciiText = asciiBuilder.ToString();
    }

    public void Receive(ResetHexAsciiInputsMessage _)
        => AsciiText = string.Empty;

    [RelayCommand(CanExecute = nameof(CanSendToHexConverter))]
    private void SendToHexConverter()
        => Messenger.Send(new ConvertAsciiToHexMessage(AsciiText!));

    private bool CanSendToHexConverter()
        => !string.IsNullOrWhiteSpace(AsciiText);
}
