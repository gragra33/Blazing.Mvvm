using HybridSample.Core.Services;

namespace HybridSample.Avalonia.Services;

internal class FilesService : IFilesService
{
    public string InstallationPath { get; } = Environment.CurrentDirectory;

    public async Task<Stream>? OpenForReadAsync(string path)
    {
        string filePath = Path.Combine(InstallationPath, path);
        FileStream stream = File.OpenRead(filePath);
        return stream;
    }
}
