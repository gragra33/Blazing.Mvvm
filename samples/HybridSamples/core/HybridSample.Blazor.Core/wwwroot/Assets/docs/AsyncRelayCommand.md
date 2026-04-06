---
title: AsyncRelayCommand
author: Sergio0694
description: An asynchronous command whose sole purpose is to relay its functionality to other objects by invoking delegates
keywords: windows 10, uwp, windows community toolkit, uwp community toolkit, uwp toolkit, mvvm, componentmodel, property changed, notification, binding, command, delegate, net core, net standard
dev_langs:
  - csharp
---

# AsyncRelayCommand and AsyncRelayCommand&lt;T&gt;

The [`AsyncRelayCommand`](/dotnet/api/communitytoolkit.mvvm.input.AsyncRelayCommand) and [`AsyncRelayCommand<T>`](/dotnet/api/communitytoolkit.mvvm.input.AsyncRelayCommand-1) are `ICommand` implementations that extend the functionalities offered by [`RelayCommand`](/dotnet/api/communitytoolkit.mvvm.input.RelayCommand), with support for asynchronous operations.

> **Platform APIs:** [`AsyncRelayCommand`](/dotnet/api/communitytoolkit.mvvm.input.AsyncRelayCommand), [`AsyncRelayCommand<T>`](/dotnet/api/communitytoolkit.mvvm.input.AsyncRelayCommand-1), [`RelayCommand`](/dotnet/api/communitytoolkit.mvvm.input.RelayCommand), [`IAsyncRelayCommand`](/dotnet/api/communitytoolkit.mvvm.input.IAsyncRelayCommand), [`IAsyncRelayCommand<T>`](/dotnet/api/communitytoolkit.mvvm.input.IAsyncRelayCommand-1)

## How they work

`AsyncRelayCommand` and `AsyncRelayCommand<T>` have the following main features:

- They extend the functionalities of the synchronous commands included in the library, with support for `Task`-returning delegates.
- They can wrap asynchronous functions with an additional `CancellationToken` parameter to support cancelation, and they expose a `CanBeCanceled` and `IsCancellationRequested` properties, as well as a `Cancel` method.
- They expose an `ExecutionTask` property that can be used to monitor the progress of a pending operation, and an `IsRunning` that can be used to check when an operation completes. This is particularly useful to bind a command to UI elements such as loading indicators.
- They implement the [`IAsyncRelayCommand`](/dotnet/api/communitytoolkit.mvvm.input.IAsyncRelayCommand) and [`IAsyncRelayCommand<T>`](/dotnet/api/communitytoolkit.mvvm.input.IAsyncRelayCommand-1) interfaces, which means that viewmodel can easily expose commands using these to reduce the tight coupling between types. For instance, this makes it easier to replace a command with a custom implementation exposing the same public API surface, if needed.

## Working with asynchronous commands

Let's imagine a scenario similar to the one described in the `RelayCommand` sample, but a command executing an asynchronous operation:

```csharp
public class MyViewModel : ObservableObject
{
    public MyViewModel()
    {
        DownloadTextCommand = new AsyncRelayCommand(DownloadText);
    }

    public IAsyncRelayCommand DownloadTextCommand { get; }

    private Task<string> DownloadText()
    {
        return WebService.LoadMyTextAsync();
    }
}
```

With the related UI code:

```html
<div class="mb-3">
    <p class="sample--output">Status: @ViewModel.DownloadTextCommand.ExecutionTask?.Status</p>
    @{
        string? GetResult(object? executionTask)
        {
            if (executionTask is Task<string> task)
                return task.Status == TaskStatus.RanToCompletion ? task.Result : default;

            return default;
        }
        <p class="sample--output">Result: @GetResult(ViewModel.DownloadTextCommand.ExecutionTask)</p>
    }
    <button type="button"
            class="btn btn-primary" @onclick="async () => 
            {
                await ViewModel.DownloadTextCommand.ExecuteAsync(null);
                await InvokeAsync(StateHasChanged);
            }">
        Click me!
    </button>
</div>
```

Upon clicking the `Button`, the command is invoked, and the `ExecutionTask` updated. When the operation completes, the property raises a notification which is reflected in the UI. In this case, both the task status and the current result of the task are displayed. Note that to show the result of the task, it is necessary to use the `TaskExtensions.GetResultOrDefault` method - this provides access to the result of a task that has not yet completed without blocking the thread (and possibly causing a deadlock).

## Examples

- Check out the [sample app](https://aka.ms/mvvmtoolkit/samples) (for multiple UI frameworks) to see the MVVM Toolkit in action.
- You can also find more examples in the [unit tests](https://github.com/CommunityToolkit/dotnet/tree/main/tests/CommunityToolkit.Mvvm.UnitTests).
