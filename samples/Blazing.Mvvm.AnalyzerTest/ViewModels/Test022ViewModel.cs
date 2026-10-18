using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0022: The View for this ViewModel implements IAsyncDisposable itself, bypassing MvvmComponentBase disposal.
// See Test022.razor for the actual test case.
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test022ViewModel : ViewModelBase
{
}
