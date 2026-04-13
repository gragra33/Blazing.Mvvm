using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0002: Missing [ViewModelDefinition] attribute
public class Test002ViewModel : ViewModelBase
{
    public string Name { get; set; } = "Test";
}
