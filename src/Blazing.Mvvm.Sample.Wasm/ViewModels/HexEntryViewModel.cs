using System.Text;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.Wasm.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.Sample.Wasm.ViewModels;

public sealed partial class HexEntryViewModel : RecipientViewModelBase<ConvertAsciiToHexMessage>, IRecipient<ResetHexAsciiInputsMessage>
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendToAsciiConverterCommand))]
    private string? _hexText;

    public override void Receive(ConvertAsciiToHexMessage message)
    {
        char[] charArray = message.AsciiToConvert.ToCharArray();
        StringBuilder hexOutput = new();
        foreach (char @char in charArray)
        {
            hexOutput.AppendFormat("{0:X}", Convert.ToInt32(@char));
        }

        HexText = hexOutput.ToString();
    }

    public void Receive(ResetHexAsciiInputsMessage _)
        => HexText = string.Empty;

    [RelayCommand(CanExecute = nameof(CanSendToAsciiConverter))]
    private void SendToAsciiConverter()
        => Messenger.Send(new ConvertHexToAsciiMessage(HexText!));

    private bool CanSendToAsciiConverter()
        => !string.IsNullOrWhiteSpace(HexText);
}
