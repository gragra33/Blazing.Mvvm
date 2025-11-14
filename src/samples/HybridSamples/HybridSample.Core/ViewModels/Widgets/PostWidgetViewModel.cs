// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
    private Post? post;

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
