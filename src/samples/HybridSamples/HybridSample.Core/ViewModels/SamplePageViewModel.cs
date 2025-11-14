// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

    public IReadOnlyDictionary<string, string>? Texts 
    { 
        get => texts; 
        set => SetProperty(ref texts, value); 
    }

    public IAsyncRelayCommand LoadedCommand { get; }

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

        string directory = Path.GetDirectoryName(name);
        string filename = Path.GetFileName(name);
        string path = Path.Combine("Assets", "docs", directory, $"{filename}.md");
        using Stream stream = await FilesServices.OpenForReadAsync(path);
        using StreamReader reader = new(stream);
        string text = await reader.ReadToEndAsync();

        // Fixup image links
        string fixedText = Regex.Replace(text, @"!\[[^\]]+\]\(([^ \)]+)(?:[^\)]+)?\)", m => $"![]({m.Groups[1].Value})");

        Texts = MarkdownHelper.GetParagraphs(fixedText);

        OnPropertyChanged(nameof(GetParagraph));
    }
}
