using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HybridSample.Core.ViewModels;

public interface ISamplePageViewModel : IViewModelBase
{
    /// <summary>
    /// Gets the <see cref="IAsyncRelayCommand{T}"/> responsible for loading the source markdown docs.
    /// </summary>
    IAsyncRelayCommand<string> LoadDocsCommand { get; }

    IReadOnlyDictionary<string, string>? Texts { get; set; }

    IAsyncRelayCommand LoadedCommand { get; }

    /// <summary>
    /// Gets the markdown for a specified paragraph from the docs page.
    /// </summary>
    /// <param name="key">The key of the paragraph to retrieve.</param>
    /// <returns>The text of the specified paragraph, or <see langword="null"/>.</returns>
    string GetParagraph(string key);
}
