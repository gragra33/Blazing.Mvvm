using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using HybridSample.Core.Services;

namespace HybridSample.Core.ViewModels;

/// <summary>
/// ViewModel for demonstrating usage of <see cref="AsyncRelayCommand"/> in the sample app.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public class AsyncRelayCommandPageViewModel : SamplePageViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRelayCommandPageViewModel"/> class.
    /// </summary>
    /// <param name="filesService">The service for file operations.</param>
    public AsyncRelayCommandPageViewModel(IFilesService filesService) : base(filesService)
    {
    }

    /// <summary>
    /// Gets the command that downloads text asynchronously.
    /// </summary>
    public IAsyncRelayCommand DownloadTextCommand { get; } = new AsyncRelayCommand(DownloadTextAsync);

    /// <summary>
    /// Simulates downloading text asynchronously.
    /// </summary>
    /// <returns>A task that returns the downloaded text.</returns>
    private static async Task<string> DownloadTextAsync()
    {
        await Task.Delay(3000); // Simulate a web request

        return "Hello world!";
    }
}
