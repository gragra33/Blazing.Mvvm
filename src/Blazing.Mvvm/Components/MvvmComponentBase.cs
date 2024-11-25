using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components;

public abstract class MvvmComponentBase<TViewModel> : ComponentBase, IView<TViewModel>, IDisposable  where TViewModel : IViewModelBase
{
    [Inject]
    protected TViewModel? ViewModel { get; set; }

    protected override void OnInitialized()
    {
        // Cause changes to the ViewModel to make Blazor re-render
        ViewModel!.PropertyChanged += (_, _) => InvokeAsync(StateHasChanged);
        base.OnInitialized();
    }

     public virtual void Dispose()
     {
        GC.SuppressFinalize(this);
        ObservableRecipient? observableRecipient  = (base.ViewModel as ObservableRecipient);
        if(observableRecipient != null)
        {
            observableRecipient.IsActive = false;
        }
    }

    protected override Task OnInitializedAsync()
        => ViewModel!.OnInitializedAsync();
}
