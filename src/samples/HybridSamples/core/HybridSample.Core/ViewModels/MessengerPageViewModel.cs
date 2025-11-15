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
    /// Gets the command to request the current username.
    /// </summary>
    public ICommand RequestCurrentUsernameCommand { get; }

    /// <summary>
    /// Gets the command to reset the current username.
    /// </summary>
    public ICommand ResetCurrentUsernameCommand { get; }

    /// <summary>
    /// Gets the sender view model for username messages.
    /// </summary>
    public UserSenderViewModel SenderViewModel { get; } = new();

    /// <summary>
    /// Gets the receiver view model for username messages.
    /// </summary>
    public UserReceiverViewModel ReceiverViewModel { get; } = new();

    /// <summary>
    /// Simple viewmodel for a module sending a username message.
    /// </summary>
    public class UserSenderViewModel : ObservableRecipient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserSenderViewModel"/> class.
        /// </summary>
        public UserSenderViewModel()
        {
            SendUserMessageCommand = new RelayCommand(SendUserMessage);
        }

        /// <summary>
        /// Gets the command to send a username message.
        /// </summary>
        public ICommand SendUserMessageCommand { get; }

        private string username = "Bob";

        /// <summary>
        /// Gets the current username.
        /// </summary>
        public string Username
        {
            get => username;
            private set => SetProperty(ref username, value);
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            Messenger.Register<UserSenderViewModel, CurrentUsernameRequestMessage>(this, (r, m) => m.Reply(r.Username));
        }

        /// <summary>
        /// Sends a username changed message and toggles the username value.
        /// </summary>
        public void SendUserMessage()
        {
            Username = Username == "Bob" ? "Alice" : "Bob";

            Messenger.Send(new UsernameChangedMessage(Username));
        }
    }

    /// <summary>
    /// Simple viewmodel for a module receiving a username message.
    /// </summary>
    public class UserReceiverViewModel : ObservableRecipient
    {
        private string username = "";

        /// <summary>
        /// Gets the received username.
        /// </summary>
        public string Username
        {
            get => username;
            private set => SetProperty(ref username, value);
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            Messenger.Register<UserReceiverViewModel, UsernameChangedMessage>(this, (r, m) => r.Username = m.Value);
        }
    }

    private string? username;

    /// <summary>
    /// Gets the current username value.
    /// </summary>
    public string? Username
    {
        get => username;
        private set => SetProperty(ref username, value);
    }

    /// <summary>
    /// Requests the current username using the messenger.
    /// </summary>
    public void RequestCurrentUsername()
    {
        Username = WeakReferenceMessenger.Default.Send<CurrentUsernameRequestMessage>();
    }

    /// <summary>
    /// Resets the current username value.
    /// </summary>
    public void ResetCurrentUsername()
    {
        Username = null;
    }

    /// <summary>
    /// A sample message with a username value.
    /// </summary>
    /// <param name="value">The username value.</param>
    public sealed class UsernameChangedMessage(string value) : ValueChangedMessage<string>(value);

    /// <summary>
    /// A sample request message to get the current username.
    /// </summary>
    public sealed class CurrentUsernameRequestMessage : RequestMessage<string>
    {
    }
}
