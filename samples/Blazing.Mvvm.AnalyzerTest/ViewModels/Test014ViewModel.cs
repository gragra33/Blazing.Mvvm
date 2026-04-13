using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0014: Unnecessary StateHasChanged calls
// NOTE: This analyzer is designed to detect StateHasChanged() calls in Blazor components,
// not ViewModels. ViewModels don't have access to StateHasChanged() from ComponentBase.
// The actual test case would be in a Razor component's @code block, not the ViewModel.
// See Test014.razor for the actual test case.
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test014ViewModel : ViewModelBase
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    // Manually declare properties with SetProperty for proper property change notification
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public void UpdateName(string first, string last)
    {
        FirstName = first;   // SetProperty triggers PropertyChanged -> UI updates automatically
        LastName = last;     // SetProperty triggers PropertyChanged -> UI updates automatically
        StateHasChanged();   // Intentional: should trigger BLAZMVVM0014
    }

    private void StateHasChanged()
    {
        // Intentional no-op for analyzer test coverage.
    }
}
