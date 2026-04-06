using HybridSample.Core.Services;

namespace HybridSample.WinForms.Services;

/// <summary>
/// WinForms implementation of the files service.
/// </summary>
public class FilesService : IFilesService
{
    /// <summary>
    /// Gets the path of the installation directory.
    /// </summary>
    public string InstallationPath => Application.StartupPath;

    /// <summary>
    /// Gets a readonly Stream for a file at a specified path.
    /// </summary>
    /// <param name="path">The path of the file to retrieve.</param>
    /// <returns>The Stream for the specified file.</returns>
    public Task<Stream?> OpenForReadAsync(string path)
    {
        try
        {
            // Try to resolve the path relative to the application directory
            string fullPath = Path.Combine(Application.StartupPath, path);
            
            if (File.Exists(fullPath))
            {
                var stream = File.OpenRead(fullPath);
                return Task.FromResult<Stream?>(stream);
            }
            
            // Try the path as-is if it's already absolute
            if (File.Exists(path))
            {
                var stream = File.OpenRead(path);
                return Task.FromResult<Stream?>(stream);
            }

            // File not found
            return Task.FromResult<Stream?>(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening file '{path}': {ex.Message}");
            return Task.FromResult<Stream?>(null);
        }
    }

    /// <summary>
    /// Opens a file dialog to select a file.
    /// </summary>
    /// <returns>The selected file path, or null if no file was selected.</returns>
    public async Task<string?> OpenFileAsync()
    {
        return await Task.Run(() =>
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : null;
        });
    }

    /// <summary>
    /// Saves a file dialog to select a file location.
    /// </summary>
    /// <returns>The selected file path, or null if no location was selected.</returns>
    public async Task<string?> SaveFileAsync()
    {
        return await Task.Run(() =>
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            return saveFileDialog.ShowDialog() == DialogResult.OK ? saveFileDialog.FileName : null;
        });
    }
}