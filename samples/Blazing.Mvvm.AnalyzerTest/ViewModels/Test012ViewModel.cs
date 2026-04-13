using Blazing.Mvvm.AnalyzerTest.Data;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0012: Public method called from UI should be RelayCommand
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test012ViewModel : ViewModelBase
{
    private readonly ITestService _testService;

    public Test012ViewModel(ITestService testService)
    {
        _testService = testService;
    }

    [ObservableProperty]
    private string _data = string.Empty;

    // Should be [RelayCommand] instead of public method
    public async Task LoadDataAsync()
    {
        Data = await _testService.GetDataAsync();
    }

    // Should be [RelayCommand] instead of public method
    public void ResetData()
    {
        Data = string.Empty;
    }
}
