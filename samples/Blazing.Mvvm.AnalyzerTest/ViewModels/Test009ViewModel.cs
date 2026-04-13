using Blazing.Mvvm.AnalyzerTest.Data;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0009: Using [Inject] instead of constructor injection in ViewModel
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test009ViewModel : ViewModelBase
{
    [Inject]  // Wrong! Should use constructor injection
    public ITestService TestService { get; set; } = null!;

    [Inject]  // Wrong! Should use constructor injection
    public HttpClient HttpClient { get; set; } = null!;
}
