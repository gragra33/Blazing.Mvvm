using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0018: Missing NotifyPropertyChangedFor for computed properties
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test018ViewModel : ViewModelBase
{
    [ObservableProperty]  // Missing [NotifyPropertyChangedFor(nameof(FullName))]
    private string _firstName = string.Empty;

    [ObservableProperty]  // Missing [NotifyPropertyChangedFor(nameof(FullName))]
    private string _lastName = string.Empty;

    // Computed property - won't update automatically when FirstName/LastName change
    public string FullName => $"{FirstName} {LastName}";
}
