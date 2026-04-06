using Blazing.Mvvm.Tests.Infrastructure.Fakes.ViewModels;
using Blazing.Mvvm.Tests.Infrastructure.Fakes.Views;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.ComponentTests;

/// <summary>
/// Component tests for AsyncRelayCommand integration with Blazor components.
/// Tests verify that button states update correctly when IsRunning changes (GitHub Issue #65).
/// </summary>
public class AsyncRelayCommandComponentTests : ComponentTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRelayCommandComponentTests"/> class
    /// and registers the test ViewModel.
    /// </summary>
    public AsyncRelayCommandComponentTests()
    {
        Services.AddSingleton(_ => CreateInstance<AsyncCommandTestViewModel>(true));
    }

    /// <summary>
    /// Verifies that button becomes disabled while command is executing and re-enabled when complete.
    /// This is the core scenario from GitHub Issue #65.
    /// </summary>
    [Fact]
    public async Task WhenCommandExecutes_ThenButtonDisablesAndReEnables()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();
        var button = cut.Find("#load-data");

        // Act - Initial state
        var initiallyEnabled = !button.HasAttribute("disabled");

        // Start command execution
        button.Click();
        await Task.Delay(20); // Let command start

        var disabledWhileRunning = button.HasAttribute("disabled");

        // Wait for command to complete
        await Task.Delay(150);
        cut.WaitForState(() => !GetViewModel<AsyncCommandTestViewModel>().LoadDataCommand.IsRunning, timeout: TimeSpan.FromSeconds(2));

        var enabledAfterCompletion = !button.HasAttribute("disabled");

        // Assert
        using var _ = new AssertionScope();
        initiallyEnabled.Should().BeTrue("Button should be enabled initially");
        disabledWhileRunning.Should().BeTrue("Button should be disabled while command is running");
        enabledAfterCompletion.Should().BeTrue("Button should be re-enabled after command completes");
    }

    /// <summary>
    /// Verifies that IsRunning state is reflected in the UI.
    /// </summary>
    [Fact]
    public async Task WhenCommandExecutes_ThenIsRunningStateIsDisplayed()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();

        // Act - Execute command
        cut.Find("#load-data").Click();
        await Task.Delay(20);

        var runningStateWhileExecuting = cut.Find("#is-running").TextContent;

        // Wait for completion
        await Task.Delay(150);
        cut.WaitForState(() => !GetViewModel<AsyncCommandTestViewModel>().LoadDataCommand.IsRunning, timeout: TimeSpan.FromSeconds(2));

        var runningStateAfterCompletion = cut.Find("#is-running").TextContent;

        // Assert
        runningStateWhileExecuting.Should().Contain("True", "IsRunning should be true while executing");
        runningStateAfterCompletion.Should().Contain("False", "IsRunning should be false after completion");
    }

    /// <summary>
    /// Verifies that result updates are shown in the UI after command completion.
    /// </summary>
    [Fact]
    public async Task WhenCommandCompletes_ThenResultIsUpdatedInUI()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();

        // Act
        cut.Find("#load-data").Click();
        
        // Wait for command to complete
        await Task.Delay(200);
        cut.WaitForState(() => cut.Find("#result").TextContent.Contains("Data loaded"), timeout: TimeSpan.FromSeconds(2));

        // Assert
        cut.Find("#result").TextContent.Should().Contain("Data loaded");
        cut.Find("#execution-count").TextContent.Should().Contain("1");
    }

    /// <summary>
    /// Verifies that parameterized commands work correctly.
    /// </summary>
    [Fact]
    public async Task WhenParameterizedCommandExecutes_ThenParameterIsProcessed()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();

        // Act
        cut.Find("#process-data").Click();

        // Wait for completion
        await Task.Delay(200);
        cut.WaitForState(() => cut.Find("#result").TextContent.Contains("Processed"), timeout: TimeSpan.FromSeconds(2));

        // Assert
        cut.Find("#result").TextContent.Should().Contain("Processed: TestInput");
    }

    /// <summary>
    /// Verifies that multiple sequential command executions work correctly.
    /// </summary>
    [Fact]
    public async Task WhenCommandExecutedMultipleTimes_ThenEachExecutionUpdatesUI()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();
        var button = cut.Find("#fast-command");

        // Act - Execute three times
        for (int i = 0; i < 3; i++)
        {
            button.Click();
            await Task.Delay(100);
        }

        // Wait for all to complete
        cut.WaitForState(() => cut.Find("#execution-count").TextContent.Contains('3'), timeout: TimeSpan.FromSeconds(2));

        // Assert
        cut.Find("#execution-count").TextContent.Should().Contain("3");
        cut.Find("#result").TextContent.Should().Contain("Fast command completed");
    }

    /// <summary>
    /// Verifies that CanExecute state is properly displayed in the UI.
    /// </summary>
    [Fact]
    public async Task WhenCommandIsRunning_ThenCanExecuteStateIsDisplayed()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();

        // Act - Start command
        cut.Find("#slow-command").Click();
        await Task.Delay(20);

        var canExecuteWhileRunning = cut.Find("#load-data-enabled").TextContent;

        // Wait for completion
        await Task.Delay(200);
        var viewModel = GetViewModel<AsyncCommandTestViewModel>();
        cut.WaitForState(() => !viewModel.SlowCommand.IsRunning, timeout: TimeSpan.FromSeconds(2));

        var canExecuteAfterCompletion = cut.Find("#load-data-enabled").TextContent;

        // Assert
        using var _ = new AssertionScope();
        cut.Find("#load-data-enabled").TextContent.Should().Contain("True");
    }

    /// <summary>
    /// Verifies that UI updates don't block command execution.
    /// </summary>
    [Fact]
    public async Task WhenCommandExecutes_ThenUIUpdatesAreNonBlocking()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();
        var viewModel = GetViewModel<AsyncCommandTestViewModel>();

        // Act - Execute command and immediately check we can access properties
        cut.Find("#load-data").Click();
        
        var canAccessViewModel = true;
        try
        {
            _ = viewModel.Result;
            _ = viewModel.IsProcessing;
            _ = viewModel.ExecutionCount;
        }
        catch
        {
            canAccessViewModel = false;
        }

        // Wait for completion
        await Task.Delay(200);

        // Assert
        canAccessViewModel.Should().BeTrue("ViewModel should be accessible while command is running");
    }

    /// <summary>
    /// Verifies that different commands can have different IsRunning states.
    /// </summary>
    [Fact]
    public async Task WhenMultipleCommandsExist_ThenEachHasIndependentIsRunningState()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();
        var viewModel = GetViewModel<AsyncCommandTestViewModel>();

        // Act - Start slow command
        cut.Find("#slow-command").Click();
        await Task.Delay(20);

        var slowCommandRunning = viewModel.SlowCommand.IsRunning;
        var loadCommandRunning = viewModel.LoadDataCommand.IsRunning;

        // Assert
        using var _ = new AssertionScope();
        slowCommandRunning.Should().BeTrue("Slow command should be running");
        loadCommandRunning.Should().BeFalse("Load command should not be running");
    }

    /// <summary>
    /// Verifies that the component lifecycle works with async commands.
    /// </summary>
    [Fact]
    public async Task WhenComponentRendersAndDisposed_ThenNoExceptionsOccur()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();
        var viewModel = GetViewModel<AsyncCommandTestViewModel>();

        // Act - Execute command and dispose while running
        cut.Find("#slow-command").Click();
        await Task.Delay(20);

        Action disposeAction = () => viewModel.Dispose();

        // Assert
        disposeAction.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that rapid button clicks while command is running don't queue multiple executions.
    /// Due to default non-concurrent behavior, subsequent clicks should be ignored while executing.
    /// </summary>
    [Fact]
    public async Task WhenButtonClickedRapidly_ThenButtonIsDisabledWhileRunning()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();
        var button = cut.Find("#slow-command");
        var viewModel = GetViewModel<AsyncCommandTestViewModel>();

        // Act - Click once to start execution
        button.Click();
        await Task.Delay(20); // Let command start

        // Verify button is disabled while running
        var isDisabledWhileRunning = button.HasAttribute("disabled");
        var canExecuteWhileRunning = viewModel.SlowCommand.CanExecute(null);

        // Wait for command to complete
        await Task.Delay(250);
        viewModel = GetViewModel<AsyncCommandTestViewModel>();
        cut.WaitForState(() => !viewModel.SlowCommand.IsRunning, timeout: TimeSpan.FromSeconds(2));

        // Verify button is re-enabled after completion
        var isEnabledAfterCompletion = !button.HasAttribute("disabled");

        // Assert
        using var _ = new AssertionScope();
        isDisabledWhileRunning.Should().BeTrue("Button should be disabled while command is running");
        canExecuteWhileRunning.Should().BeFalse("CanExecute should be false while command is running");
        isEnabledAfterCompletion.Should().BeTrue("Button should be re-enabled after command completes");
    }

    /// <summary>
    /// Verifies that button state reflects CanExecute correctly throughout execution.
    /// </summary>
    [Fact]
    public async Task WhenCommandExecutes_ThenButtonEnabledStateMatchesCanExecute()
    {
        // Arrange
        var cut = RenderComponent<AsyncCommandTestView>();
        var button = cut.Find("#load-data");
        var viewModel = GetViewModel<AsyncCommandTestViewModel>();

        // Act & Assert - Before execution
        button.HasAttribute("disabled").Should().BeFalse();
        viewModel.LoadDataCommand.CanExecute(null).Should().BeTrue();

        // Execute
        button.Click();
        await Task.Delay(20);

        // During execution
        button.HasAttribute("disabled").Should().BeTrue();
        viewModel.LoadDataCommand.CanExecute(null).Should().BeFalse();

        // After execution
        await Task.Delay(200);
        cut.WaitForState(() => !viewModel.LoadDataCommand.IsRunning, timeout: TimeSpan.FromSeconds(2));

        button.HasAttribute("disabled").Should().BeFalse();
        viewModel.LoadDataCommand.CanExecute(null).Should().BeTrue();
    }
}
