using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Models;

namespace HybridSample.Core.ViewModels.Widgets;

/// <summary>
/// A viewmodel for a post widget.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public sealed class PostWidgetViewModel : RecipientViewModelBase<PropertyChangedMessage<Post>>
{
    /// <summary>
    /// Backing field for <see cref="Post"/>.
    /// </summary>
    private Post? post;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostWidgetViewModel"/> class.
    /// </summary>
    public PostWidgetViewModel() { /* skip */ }

    /// <summary>
    /// Gets the currently selected post, if any.
    /// </summary>
    public Post? Post
    {
        get => post;
        private set => SetProperty(ref post, value);
    }

    /// <inheritdoc/>
    public override void Receive(PropertyChangedMessage<Post> message)
    {
        if (message is
            {
                Sender: SubredditWidgetViewModel,
                PropertyName: nameof(SubredditWidgetViewModel.SelectedPost)
            })
        {
            Post = message.NewValue;
        }
    }
}
