using Blazing.Mvvm.ParentChildSample.Models;

namespace Blazing.Mvvm.ParentChildSample.Extensions;

public static class DictionaryExtensions
{
    // set up metadata to be passed when the ChildView is created
    public static void AddChildComponent(this Dictionary<string, ChildMetadata> dictionary, string name)
    {
        dictionary.Add(name, new() { Parameters = new() { ["Text"] = name } });
    }
}