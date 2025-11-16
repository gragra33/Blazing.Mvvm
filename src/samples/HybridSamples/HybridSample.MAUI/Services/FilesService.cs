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
    public async Task<Stream>? OpenForReadAsync(string path)
    {
        Debug.WriteLine($"[FilesService] Attempting to open file: {path}");

        try
        {
            // For MAUI, try to open from the app package
            Debug.WriteLine($"[FilesService] Trying FileSystem.OpenAppPackageFileAsync...");
            var stream = await FileSystem.OpenAppPackageFileAsync(path);
            Debug.WriteLine($"[FilesService] Successfully opened: {path}");
            return stream;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FilesService] FileSystem.OpenAppPackageFileAsync failed: {ex.Message}");

            // If file not found in app package, try absolute path
            try
            {
                Debug.WriteLine($"[FilesService] Trying File.OpenRead with absolute path...");
                var absolutePath = Path.Combine(InstallationPath, path);
                Debug.WriteLine($"[FilesService] Absolute path: {absolutePath}");
                return File.OpenRead(absolutePath);
            }
            catch (Exception ex2)
            {
                Debug.WriteLine($"[FilesService] File.OpenRead failed: {ex2.Message}");
                Debug.WriteLine($"[FilesService] All attempts failed for: {path}");
                throw new FileNotFoundException($"Could not find file '{path}' in app package or file system.", path, ex2);
            }
        }
    }
}