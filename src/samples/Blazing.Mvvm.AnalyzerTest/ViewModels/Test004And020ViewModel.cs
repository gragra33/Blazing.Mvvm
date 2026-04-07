using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0004 & BLAZMVVM0020: ViewParameter and Route Parameter Binding
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test004And020ViewModel : ViewModelBase
{
    // BLAZMVVM0020: Route parameter without [ViewParameter] attribute
    // Page has @page "/test004/{id:int}" but this property lacks [ViewParameter]
    public int Id { get; set; }

    // BLAZMVVM0004: ViewParameter without matching [Parameter] in View
    // This property has [ViewParameter] but the View doesn't have matching [Parameter]
    [ViewParameter]
    public string Title { get; set; } = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;
}
