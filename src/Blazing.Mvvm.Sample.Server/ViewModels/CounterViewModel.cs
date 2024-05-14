using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class CounterViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _currentCount;

    public void IncrementCount()
        => CurrentCount++;

    public void ResetCount()
        => CurrentCount = 0;
}
