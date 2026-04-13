using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// Test003ViewModel for testing BLAZMVVM0003
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test003ViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _data = "No data";
}
