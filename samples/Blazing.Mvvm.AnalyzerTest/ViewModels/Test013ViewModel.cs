using Blazing.Mvvm.AnalyzerTest.Data;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0013: Should use MvvmOwningComponentBase for scoped services (DbContext)
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test013ViewModel : ViewModelBase
{
    private readonly TestDbContext _context;

    public Test013ViewModel(TestDbContext context)
    {
        _context = context;
    }

    [ObservableProperty]
    private string _data = string.Empty;

    public override async Task OnInitializedAsync()
    {
        // Use DbContext - component should inherit from MvvmOwningComponentBase
        Data = "DbContext data";
        await Task.CompletedTask;
    }
}
