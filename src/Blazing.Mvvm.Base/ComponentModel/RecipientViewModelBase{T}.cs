using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Provides a base class for ViewModels that receive messages of type <typeparamref name="TMessage"/> using <see cref="IMessenger"/>.
/// Inherits from <see cref="RecipientViewModelBase"/> and implements <see cref="IRecipient{TMessage}"/> for message handling.
/// </summary>
/// <typeparam name="TMessage">The type of message to receive. Must be a reference type.</typeparam>
public abstract class RecipientViewModelBase<TMessage> : RecipientViewModelBase, IRecipient<TMessage>
    where TMessage : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecipientViewModelBase{TMessage}"/> class.
    /// </summary>
    protected RecipientViewModelBase()
    { /* skip */ }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipientViewModelBase{TMessage}"/> class with the specified <see cref="IMessenger"/>.
    /// </summary>
    /// <param name="messenger">The messenger instance for message delivery.</param>
    protected RecipientViewModelBase(IMessenger messenger)
        : base(messenger)
    { /* skip */ }

    /// <summary>
    /// Receives a message of type <typeparamref name="TMessage"/>. Must be implemented by derived classes to handle incoming messages.
    /// </summary>
    /// <param name="message">The message instance to receive.</param>
    public abstract void Receive(TMessage message);
}
