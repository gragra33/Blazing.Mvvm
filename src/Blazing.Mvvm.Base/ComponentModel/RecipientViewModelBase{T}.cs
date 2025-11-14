using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// A base class for a <c>ViewModel</c> that implements <see cref="RecipientViewModelBase"/> which provides support to receive messages for a specific type and access to the <see cref="IMessenger"/> type.
/// </summary>
/// <typeparam name="TMessage">The type of message to receive.</typeparam>
public abstract class RecipientViewModelBase<TMessage> : RecipientViewModelBase, IRecipient<TMessage>
    where TMessage : class
{
    /// <inheritdoc/>
    public abstract void Receive(TMessage message);
}
