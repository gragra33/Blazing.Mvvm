using HybridSample.Core.Services;

namespace HybridSample.WinForms.Services;

/// <summary>
/// WinForms implementation of the settings service using in-memory storage.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
    
    /// <summary>
    /// Gets a setting value by key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <returns>The setting value or default if not found.</returns>
    public T? GetValue<T>(string key)
        => _properties.TryGetValue(key, out var value) ? (T)value : default;

    /// <summary>
    /// Sets a setting value by key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The setting key.</param>
    /// <param name="value">The setting value.</param>
    public void SetValue<T>(string key, T? value)
    {
        if (!_properties.TryAdd(key, value!))
            _properties[key] = value!;
    }
}