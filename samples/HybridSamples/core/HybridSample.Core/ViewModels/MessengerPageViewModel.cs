using System.Windows.Input;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// ViewModel for demonstrating usage of the Messenger pattern in the sample app.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class MessengerPageViewModel : SamplePageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessengerPageViewModel"/> class.
    /// </summary>
    /// <param name="filesService">The service for file operations.</param>
    public MessengerPageViewModel(IFilesService filesService) 
        : base(filesService)
    {
        RequestCurrentUsernameCommand = new RelayCommand(RequestCurrentUsername);
        ResetCurrentUsernameCommand = new RelayCommand(ResetCurrentUsername);
    }

    /// <summary>
    /// Gets the command to request the current _username.
    /// </summary>
    public ICommand RequestCurrentUsernameCommand { get; }

    /// <summary>
    /// Gets the command to reset the current _username.
    /// </summary>
    public ICommand ResetCurrentUsernameCommand { get; }

    /// <summary>
    /// Gets the sender view model for _username messages.
    /// </summary>
    public UserSenderViewModel SenderViewModel { get; } = new();

    /// <summary>
    /// Gets the receiver view model for _username messages.
    /// </summary>
    public UserReceiverViewModel ReceiverViewModel { get; } = new();

    /// <summary>
    /// Simple viewmodel for a module sending a _username message.
    /// </summary>
    [ViewModelDefinition]
    public class UserSenderViewModel : RecipientViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserSenderViewModel"/> class.
        /// </summary>
        public UserSenderViewModel()
        {
            SendUserMessageCommand = new RelayCommand(SendUserMessage);
        }

        /// <summary>
        /// Gets the command to send a _username message.
        /// </summary>
        public ICommand SendUserMessageCommand { get; }

        private string _username = "Bob";

        /// <summary>
        /// Gets the current _username.
        /// </summary>
        public string Username
        {
            get => _username;
            private set => SetProperty(ref _username, value);
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            Messenger.Register<UserSenderViewModel, CurrentUsernameRequestMessage>(this, (r, m) => m.Reply(r.Username));
        }

        /// <summary>
        /// Sends a _username changed message and toggles the _username value.
        /// </summary>
        public void SendUserMessage()
        {
            Username = Username == "Bob" ? "Alice" : "Bob";

            Messenger.Send(new UsernameChangedMessage(Username));
        }
    }

    /// <summary>
    /// Simple viewmodel for a module receiving a _username message.
    /// </summary>
    public class UserReceiverViewModel : RecipientViewModelBase
    {
        private string _username = "";

        /// <summary>
        /// Gets the received _username.
        /// </summary>
        public string Username
        {
            get => _username;
            private set => SetProperty(ref _username, value);
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            Messenger.Register<UserReceiverViewModel, UsernameChangedMessage>(this, (r, m) => r.Username = m.Value);
        }
    }

    private string? _username;

    /// <summary>
    /// Gets the current _username value.
    /// </summary>
    public string? Username
    {
        get => _username;
        private set => SetProperty(ref _username, value);
    }

    /// <summary>
    /// Requests the current _username using the messenger.
    /// </summary>
    public void RequestCurrentUsername()
    {
        Username = WeakReferenceMessenger.Default.Send<CurrentUsernameRequestMessage>();
    }

    /// <summary>
    /// Resets the current _username value.
    /// </summary>
    public void ResetCurrentUsername()
    {
        Username = null;
    }

    /// <summary>
    /// A sample message with a _username value.
    /// </summary>
    /// <param name="value">The _username value.</param>
    public sealed class UsernameChangedMessage(string value) : ValueChangedMessage<string>(value);

    /// <summary>
    /// A sample request message to get the current _username.
    /// </summary>
    public sealed class CurrentUsernameRequestMessage : RequestMessage<string>
    {
    }
}
