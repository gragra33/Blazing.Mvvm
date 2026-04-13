using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.ParentChildSample.Extensions;
using Blazing.Mvvm.ParentChildSample.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.ParentChildSample.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class HomeViewModel : RecipientViewModelBase<ChildMessage>
{
    private int _count;

    public readonly Dictionary<string, ChildMetadata> Components = new();

    [ObservableProperty]
    private string? _text = "Welcome to your new Blazing.Mvvm app.";

    [RelayCommand]
    public virtual void AddChild()
        => Components.AddChildComponent($"Child #{++_count}");

    // return a numerically sorted set of keys
    public IEnumerable<string> GetSortedKeys()
        => Components.Keys
            .Select(x => x.Split('#')[1])
            .OrderBy(int.Parse)
            .Select(index => $"Child #{index}");

    // obsolete
    // Messaging...
    //public override Task Loaded()
    //{
    //    IsActive = true;
    //    return base.Loaded();
    //}

    // inbound message
    public override void Receive(ChildMessage child)
    {
        Components.Remove(child.message);
        NotifyStateChanged();
    }
}