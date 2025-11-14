using HybridSample.Core.Services;

namespace HybridSample.Blazor.Core.Services;

public class FilesService : IFilesService
{
    private readonly HttpClient? _httpClient;

    public FilesService(HttpClient? httpClient)
    {
        _httpClient = httpClient;
        InstallationPath = _httpClient?.BaseAddress?.ToString() ?? "";
    }

    public string InstallationPath { get; }

    public async Task<Stream>? OpenForReadAsync(string path)
    {
        return  await _httpClient!.GetStreamAsync(path);
    }
}
