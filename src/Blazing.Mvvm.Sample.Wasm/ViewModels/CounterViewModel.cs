using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.Wasm.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Singleton)]
public sealed partial class CounterViewModel : ViewModelBase
{
    private readonly ILogger<CounterViewModel> _logger;

    [ObservableProperty]
    private int _currentCount;

    public CounterViewModel(ILogger<CounterViewModel> logger)
    {
        _logger = logger;
    }

    public void IncrementCount()
        => CurrentCount++;

    public void ResetCount()
        => CurrentCount = 0;

    public override void OnInitialized()
        => LogLifeCycleEvent(nameof(OnInitialized));

    public override Task OnInitializedAsync()
    {
        LogLifeCycleEvent(nameof(OnInitializedAsync));
        return base.OnInitializedAsync();
    }

    public override void OnParametersSet()
        => LogLifeCycleEvent(nameof(OnParametersSet));

    public override Task OnParametersSetAsync()
    {
        LogLifeCycleEvent(nameof(OnParametersSetAsync));
        return Task.CompletedTask;
    }

    public override void OnAfterRender(bool firstRender)
        => LogLifeCycleEvent(nameof(OnAfterRender));

    public override Task OnAfterRenderAsync(bool firstRender)
    {
        LogLifeCycleEvent(nameof(OnAfterRenderAsync));
        return Task.CompletedTask;
    }

    public override bool ShouldRender()
    {
        LogLifeCycleEvent(nameof(ShouldRender));
        return true;
    }

    [LoggerMessage(Message = "{ViewModel} => Life-cycle event: {LifeCycleMethod}.")]
    private partial void LogLifeCycleEvent(string lifeCycleMethod, string viewModel = nameof(CounterViewModel), LogLevel level = LogLevel.Information);
}
