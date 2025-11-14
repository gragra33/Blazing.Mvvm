using HybridSample.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HybridSample.Wpf.Services;

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
