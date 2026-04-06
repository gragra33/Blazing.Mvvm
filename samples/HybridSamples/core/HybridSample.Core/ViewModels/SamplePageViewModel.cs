using System.Text.RegularExpressions;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Helpers;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// A base class for viewmodels for sample pages in the app.
/// </summary>
[ViewModelDefinition<IIntroductionPageViewModel>(Lifetime = ServiceLifetime.Transient)]
[ViewModelDefinition<ISettingUpTheViewModelsPageViewModel>(Lifetime = ServiceLifetime.Transient)]
[ViewModelDefinition<ISettingsServicePageViewModel>(Lifetime = ServiceLifetime.Transient)]
[ViewModelDefinition<IRedditServicePageViewModel>(Lifetime = ServiceLifetime.Transient)]
[ViewModelDefinition<IBuildingTheUIPageViewModel>(Lifetime = ServiceLifetime.Transient)]
[ViewModelDefinition<IRedditBrowserPageViewModel>(Lifetime = ServiceLifetime.Transient)]
public class SamplePageViewModel : ViewModelBase, 
    IIntroductionPageViewModel, 
    ISettingUpTheViewModelsPageViewModel, 
    ISettingsServicePageViewModel, 
    IRedditServicePageViewModel,
    IBuildingTheUIPageViewModel,
    IRedditBrowserPageViewModel
{
    /// <summary>
    /// The <see cref="IFilesService"/> instance currently in use.
    /// </summary>
    private readonly IFilesService FilesServices;

    /// <summary>
    /// Initializes a new instance of the <see cref="SamplePageViewModel"/> class.
    /// </summary>
    /// <param name="filesService">The service for file operations.</param>
    public SamplePageViewModel(IFilesService filesService)
    {
        FilesServices = filesService;

        LoadDocsCommand = new AsyncRelayCommand<string>(LoadDocsAsync);
    }

    /// <summary>
    /// Gets the <see cref="IAsyncRelayCommand{T}"/> responsible for loading the source markdown docs.
    /// </summary>
    public IAsyncRelayCommand<string> LoadDocsCommand { get; }

    private IReadOnlyDictionary<string, string>? texts;

    /// <summary>
    /// Gets or sets the loaded markdown paragraphs as a dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Texts 
    { 
        get => texts; 
        set => SetProperty(ref texts, value); 
    }

    /// <summary>
    /// Gets the command that is executed when the view is loaded.
    /// </summary>
    public IAsyncRelayCommand LoadedCommand { get; } = null!;

    /// <summary>
    /// Gets the markdown for a specified paragraph from the docs page.
    /// </summary>
    /// <param name="key">The key of the paragraph to retrieve.</param>
    /// <returns>The text of the specified paragraph, or <see langword="null"/>.</returns>
    public string GetParagraph(string key)
    {
        return Texts is not null && Texts.TryGetValue(key, out string? value) ? value : string.Empty;
    }

    /// <summary>
    /// Implements the logic for <see cref="LoadDocsCommand"/>.
    /// </summary>
    /// <param name="name">The name of the docs file to load.</param>
    private async Task LoadDocsAsync(string? name)
    {
        if (name is null) return;

        // Skip if the loading has already started
        if (LoadDocsCommand.ExecutionTask is not null) return;

        string? directory = Path.GetDirectoryName(name);
        string filename = Path.GetFileName(name);
        
        // Build path using forward slashes for cross-platform compatibility (MAUI assets)
        string path = string.IsNullOrEmpty(directory)
            ? $"Assets/docs/{filename}.md"
            : $"Assets/docs/{directory}/{filename}.md";
        
        await using Stream? stream = await FilesServices.OpenForReadAsync(path);
        using StreamReader reader = new(stream!);
        string text = await reader.ReadToEndAsync();

        // Fixup image links
        string fixedText = Regex.Replace(text, @"!\[[^\]]+\]\(([^ \)]+)(?:[^\)]+)?\)", m => $"![]({m.Groups[1].Value})");

        Texts = MarkdownHelper.GetParagraphs(fixedText);

        OnPropertyChanged(nameof(GetParagraph));
    }
}
