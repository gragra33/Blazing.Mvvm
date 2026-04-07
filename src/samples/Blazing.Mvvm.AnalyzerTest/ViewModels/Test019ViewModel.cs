using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test019ViewModel : ViewModelBase
{
}
