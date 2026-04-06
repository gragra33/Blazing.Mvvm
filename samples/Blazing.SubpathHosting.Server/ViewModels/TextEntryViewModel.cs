using System.Globalization;
using System.Text;
using Blazing.Mvvm.ComponentModel;
using Blazing.SubpathHosting.Server.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.SubpathHosting.Server.ViewModels;

[ViewModelDefinition]
public sealed partial class TextEntryViewModel : RecipientViewModelBase<ConvertHexToAsciiMessage>, IRecipient<ResetHexAsciiInputsMessage>
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendToHexConverterCommand))]
    private string? _asciiText;

    public override void Receive(ConvertHexToAsciiMessage message)
    {
        var asciiBuilder = new StringBuilder();

        if (message.HexToConvert.Length % 2 != 0)
        {
            AsciiText = string.Empty;
            return;
        }

        for (int i = 0; i < message.HexToConvert.Length; i += 2)
        {
            if (!byte.TryParse(message.HexToConvert.AsSpan(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte value))
            {
                AsciiText = string.Empty;
                return;
            }

            asciiBuilder.Append((char)value);
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
