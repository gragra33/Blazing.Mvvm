using HybridSample.Core.Services;

namespace HybridSample.Blazor.Core.Services;

/// <summary>
/// Provides file access services for Blazor using HTTP requests.
/// </summary>
public class FilesService : IFilesService
{
    /// <summary>
    /// The HTTP client used to access files.
    /// </summary>
    private readonly HttpClient? _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilesService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for file access.</param>
    public FilesService(HttpClient? httpClient)
    {
        _httpClient = httpClient;
        InstallationPath = _httpClient?.BaseAddress?.ToString() ?? "";
    }

    /// <inheritdoc/>
    public string InstallationPath { get; }

    /// <summary>
    /// Gets a readonly <see cref="Stream"/> for a file at a specified path using HTTP.
    /// </summary>
    /// <param name="path">The path of the file to retrieve.</param>
    /// <returns>The <see cref="Stream"/> for the specified file.</returns>
    public async Task<Stream> OpenForReadAsync(string path)
    {
        return  await _httpClient!.GetStreamAsync(path);
    }
}
