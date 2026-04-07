using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0001: Missing ViewModelBase inheritance - ViewModel suffix but no base class
public class Test001ViewModel
{
    public string Name { get; set; } = "Test";
}
