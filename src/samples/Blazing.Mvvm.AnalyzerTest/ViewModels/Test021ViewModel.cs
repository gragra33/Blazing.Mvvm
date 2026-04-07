using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// ViewModel for Test021 - EventCallback two-way binding
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test021ViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private int _counter;
}
