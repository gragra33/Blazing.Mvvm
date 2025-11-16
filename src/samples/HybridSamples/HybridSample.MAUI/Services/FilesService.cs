using HybridSample.Core.Services;
using System.Diagnostics;

namespace HybridSample.MAUI.Services;

/// <summary>
/// MAUI implementation of the files service.
/// </summary>
public class FilesService : IFilesService
{
    /// <summary>
    /// Opens a file dialog to select a file.
    /// </summary>
    /// <returns>The selected file path, or null if no file was selected.</returns>
    public async Task<string?> OpenFileAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            return result?.FullPath;
        }
        catch (Exception)
        {
            // The user canceled or something went wrong
            return null;
        }
    }

    /// <summary>
    /// Saves a file dialog to select a file location.
    /// </summary>
    /// <returns>The selected file path, or null if no location was selected.</returns>
    public async Task<string?> SaveFileAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync();
            return result?.FullPath;
        }
        catch (Exception)
        {
            // The user canceled or something went wrong
            return null;
        }
    }

    /// <summary>
    /// Gets the path of the installation directory.
    /// </summary>
    public string InstallationPath => AppContext.BaseDirectory;

    /// <summary>
    /// Gets a readonly <see cref="Stream"/> for a file at a specified path.
    /// </summary>
    /// <param name="path">The path of the file to retrieve.</param>
    /// <returns>The <see cref="Stream"/> for the specified file.</returns>
    public async Task<Stream?> OpenForReadAsync(string path)
    {
        Debug.WriteLine($"[FilesService] Attempting to open file: {path}");
        Debug.WriteLine($"[FilesService] Path uses backslashes: {path.Contains('\\')}");
        Debug.WriteLine($"[FilesService] Path uses forward slashes: {path.Contains('/')}");

        // Normalize path separators - MAUI expects forward slashes
        string normalizedPath = path.Replace('\\', '/');
        Debug.WriteLine($"[FilesService] Normalized path: {normalizedPath}");

        try
        {
            // For MAUI, try to open from the app package
            // Files in Resources\Raw with MauiAsset build action can be opened directly
            Debug.WriteLine($"[FilesService] Trying FileSystem.OpenAppPackageFileAsync with normalized path...");
            var stream = await FileSystem.OpenAppPackageFileAsync(normalizedPath);
            Debug.WriteLine($"[FilesService] Successfully opened: {normalizedPath}");
            return stream;
        }
        catch (FileNotFoundException ex)
        {
            Debug.WriteLine($"[FilesService] FileNotFoundException: {ex.Message}");
            Debug.WriteLine($"[FilesService] File not found in app package: {normalizedPath}");
            Debug.WriteLine($"[FilesService] Original path: {path}");
            throw new FileNotFoundException($"Could not find file '{normalizedPath}' in app package. Original path: '{path}'. Ensure the file is in Resources\\Raw folder with MauiAsset build action.", normalizedPath, ex);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FilesService] Unexpected error: {ex.Message}");
            Debug.WriteLine($"[FilesService] Exception type: {ex.GetType().Name}");
            Debug.WriteLine($"[FilesService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}