using HybridSample.Core.Services;

namespace HybridSample.Avalonia.Services;

/// <summary>
/// Provides file access services for Avalonia using local file system operations.
/// </summary>
internal class FilesService : IFilesService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesService"/> class.
    /// </summary>
    public FilesService() { }

    /// <summary>
    /// Gets the path of the installation directory.
    /// </summary>
    public string InstallationPath { get; } = Environment.CurrentDirectory;

    /// <summary>
    /// Gets a readonly <see cref="Stream"/> for a file at a specified path.
    /// </summary>
    /// <param name="path">The path of the file to retrieve.</param>
    /// <returns>The <see cref="Stream"/> for the specified file.</returns>
    public Task<Stream> OpenForReadAsync(string path)
    {
        string filePath = Path.Combine(InstallationPath, path);
        FileStream stream = File.OpenRead(filePath);
        return Task.FromResult<Stream>(stream);
    }
}
