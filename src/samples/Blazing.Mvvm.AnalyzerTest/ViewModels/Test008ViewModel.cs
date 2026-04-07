using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0008: Property without change notification
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test008ViewModel : ViewModelBase
{
    private string _name = "Test";

    // BLAZMVVM0008: Custom setter without SetProperty or OnPropertyChanged
    // Should trigger analyzer to recommend using [ObservableProperty] or SetProperty
    public string Name
    {
        get => _name;
        set => _name = value;  // No notification!
    }
}
