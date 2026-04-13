using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class IndexViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Blazing.Mvvm Analyzer Test Suite";

    [ObservableProperty]
    private string _description = "This application demonstrates all 21 Blazing.Mvvm analyzers";
}
