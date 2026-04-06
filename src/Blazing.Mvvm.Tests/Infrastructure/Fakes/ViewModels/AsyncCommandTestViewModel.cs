using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes.ViewModels;

/// <summary>
/// Test ViewModel for AsyncRelayCommand component tests.
/// Demonstrates a typical use case for async commands with UI state updates.
/// </summary>
public class AsyncCommandTestViewModel : ViewModelBase
{
    private string _result = "Not started";
    private bool _isProcessing;
    private int _executionCount;

    public string Result
    {
        get => _result;
        set => SetProperty(ref _result, value);
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetProperty(ref _isProcessing, value);
    }

    public int ExecutionCount
    {
        get => _executionCount;
        set => SetProperty(ref _executionCount, value);
    }

    public IAsyncRelayCommand LoadDataCommand { get; }
    public IAsyncRelayCommand<string> ProcessCommand { get; }
    public IAsyncRelayCommand FastCommand { get; }
    public IAsyncRelayCommand SlowCommand { get; }

    public AsyncCommandTestViewModel()
    {
        LoadDataCommand = new AsyncRelayCommand(async () =>
        {
            IsProcessing = true;
            await Task.Delay(100);
            Result = "Data loaded";
            ExecutionCount++;
            IsProcessing = false;
        });

        ProcessCommand = new AsyncRelayCommand<string>(async (input) =>
        {
            IsProcessing = true;
            await Task.Delay(100);
            Result = $"Processed: {input ?? "null"}";
            ExecutionCount++;
            IsProcessing = false;
        });

        FastCommand = new AsyncRelayCommand(async () =>
        {
            await Task.Delay(30);
            Result = "Fast command completed";
            ExecutionCount++;
        });

        SlowCommand = new AsyncRelayCommand(async () =>
        {
            await Task.Delay(150);
            Result = "Slow command completed";
            ExecutionCount++;
        });
    }
}
