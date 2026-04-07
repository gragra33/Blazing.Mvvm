using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

/// <summary>
/// ViewModel that is intentionally NOT registered with [ViewModelDefinition].
/// Used by BLAZMVVM0011 analyzer test to detect unregistered ViewModels in MvvmNavLink.
/// </summary>
public class UnregisteredTargetViewModel : ViewModelBase
{
}
