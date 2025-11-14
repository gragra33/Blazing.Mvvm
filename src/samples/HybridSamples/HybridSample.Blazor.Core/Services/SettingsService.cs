using HybridSample.Core.Services;

namespace HybridSample.Blazor.Core.Services;

public class SettingsService : ISettingsService
{
    private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
    
    public T? GetValue<T>(string key)
        => _properties.TryGetValue(key, out var value) ? (T)value : default;

    public void SetValue<T>(string key, T value)
    {
            if (!_properties.TryAdd(key, value!))
                _properties[key] = value!;
    }
}
