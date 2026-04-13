using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0006: ViewModelKey mismatch with component
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient, Key = "WrongKey")]
public class Test006ViewModel : ViewModelBase
{
    public string Title { get; set; } = "Test006";
}
