using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.ComponentModel;

public abstract class RecipientViewModelBase<TMessage> : RecipientViewModelBase, IRecipient<TMessage>
    where TMessage : class
{
    public abstract void Receive(TMessage message);
}
