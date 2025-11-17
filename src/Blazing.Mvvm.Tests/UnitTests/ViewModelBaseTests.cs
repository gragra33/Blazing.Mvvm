using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="ViewModelBase"/> covering lifecycle methods, property change notification, and rendering logic.
/// </summary>
public class ViewModelBaseTests
{
    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnAfterRender(bool)"/> is called with firstRender = true.
    /// </summary>
    [Fact]
    public void OnAfterRender_GivenFirstRender_ShouldCallVirtualMethod()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        viewModel.OnAfterRender(firstRender: true);

        // Assert
        viewModel.OnAfterRenderCalled.Should().BeTrue();
        viewModel.OnAfterRenderFirstRender.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnAfterRender(bool)"/> is called with firstRender = false.
    /// </summary>
    [Fact]
    public void OnAfterRender_GivenNotFirstRender_ShouldCallVirtualMethod()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        viewModel.OnAfterRender(firstRender: false);

        // Assert
        viewModel.OnAfterRenderCalled.Should().BeTrue();
        viewModel.OnAfterRenderFirstRender.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnAfterRenderAsync(bool)"/> is called with firstRender = true.
    /// </summary>
    [Fact]
    public async Task OnAfterRenderAsync_GivenFirstRender_ShouldCallVirtualMethod()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        await viewModel.OnAfterRenderAsync(firstRender: true);

        // Assert
        viewModel.OnAfterRenderAsyncCalled.Should().BeTrue();
        viewModel.OnAfterRenderAsyncFirstRender.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnAfterRenderAsync(bool)"/> is called with firstRender = false.
    /// </summary>
    [Fact]
    public async Task OnAfterRenderAsync_GivenNotFirstRender_ShouldCallVirtualMethod()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        await viewModel.OnAfterRenderAsync(firstRender: false);

        // Assert
        viewModel.OnAfterRenderAsyncCalled.Should().BeTrue();
        viewModel.OnAfterRenderAsyncFirstRender.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnInitialized()"/> is called.
    /// </summary>
    [Fact]
    public void OnInitialized_ShouldCallVirtualMethod()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        viewModel.OnInitialized();

        // Assert
        viewModel.OnInitializedCalled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnInitializedAsync()"/> is called.
    /// </summary>
    [Fact]
    public async Task OnInitializedAsync_ShouldCallVirtualMethod()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        await viewModel.OnInitializedAsync();

        // Assert
        viewModel.OnInitializedAsyncCalled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnParametersSet()"/> is called.
    /// </summary>
    [Fact]
    public void OnParametersSet_ShouldCallVirtualMethod()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        viewModel.OnParametersSet();

        // Assert
        viewModel.OnParametersSetCalled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnParametersSetAsync()"/> is called.
    /// </summary>
    [Fact]
    public async Task OnParametersSetAsync_ShouldCallVirtualMethod()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        await viewModel.OnParametersSetAsync();

        // Assert
        viewModel.OnParametersSetAsyncCalled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.ShouldRender()"/> returns true by default.
    /// </summary>
    [Fact]
    public void ShouldRender_ByDefault_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        var result = viewModel.ShouldRender();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.ShouldRender()"/> returns a custom value when overridden.
    /// </summary>
    [Fact]
    public void ShouldRender_WhenOverridden_ShouldReturnCustomValue()
    {
        // Arrange
        var viewModel = new TestViewModelBase();
        viewModel.ShouldRenderResult = false;

        // Act
        var result = viewModel.ShouldRender();

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.NotifyStateChanged()"/> triggers property changed notification.
    /// </summary>
    [Fact]
    public void NotifyStateChanged_ShouldTriggerPropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModelBase();
        var propertyChangedTriggered = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedTriggered = true;
        };

        // Act
        viewModel.NotifyStateChanged();

        // Assert
        propertyChangedTriggered.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase"/> implements <see cref="IViewModelBase"/>.
    /// </summary>
    [Fact]
    public void ViewModelBase_ShouldImplementIViewModelBase()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act & Assert
        viewModel.Should().BeAssignableTo<IViewModelBase>();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase"/> implements <see cref="System.ComponentModel.INotifyPropertyChanged"/>.
    /// </summary>
    [Fact]
    public void ViewModelBase_ShouldImplementINotifyPropertyChanged()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act & Assert
        viewModel.Should().BeAssignableTo<System.ComponentModel.INotifyPropertyChanged>();
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnAfterRenderAsync(bool)"/> returns a completed task by default.
    /// </summary>
    [Fact]
    public async Task OnAfterRenderAsync_ByDefault_ShouldReturnCompletedTask()
    {
        // Arrange
        var viewModel = new DefaultViewModelBase();

        // Act
        var task = viewModel.OnAfterRenderAsync(true);

        // Assert
        task.Should().Be(Task.CompletedTask);
        await task; // Should complete without throwing
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnInitializedAsync()"/> returns a completed task by default.
    /// </summary>
    [Fact]
    public async Task OnInitializedAsync_ByDefault_ShouldReturnCompletedTask()
    {
        // Arrange
        var viewModel = new DefaultViewModelBase();

        // Act
        var task = viewModel.OnInitializedAsync();

        // Assert
        task.Should().Be(Task.CompletedTask);
        await task; // Should complete without throwing
    }

    /// <summary>
    /// Tests that <see cref="ViewModelBase.OnParametersSetAsync()"/> returns a completed task by default.
    /// </summary>
    [Fact]
    public async Task OnParametersSetAsync_ByDefault_ShouldReturnCompletedTask()
    {
        // Arrange
        var viewModel = new DefaultViewModelBase();

        // Act
        var task = viewModel.OnParametersSetAsync();

        // Assert
        task.Should().Be(Task.CompletedTask);
        await task; // Should complete without throwing
    }

    /// <summary>
    /// Tests that multiple calls to lifecycle methods maintain state.
    /// </summary>
    [Fact]
    public void ViewModelBase_MultipleCalls_ShouldMaintainState()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        viewModel.OnInitialized();
        viewModel.OnParametersSet();
        viewModel.OnAfterRender(true);

        // Assert
        using var _ = new AssertionScope();
        viewModel.OnInitializedCalled.Should().BeTrue();
        viewModel.OnParametersSetCalled.Should().BeTrue();
        viewModel.OnAfterRenderCalled.Should().BeTrue();
    }

    /// <summary>
    /// Tests that async lifecycle methods work in parallel.
    /// </summary>
    [Fact]
    public async Task ViewModelBase_AsyncMethods_ShouldWorkInParallel()
    {
        // Arrange
        var viewModel = new TestViewModelBase();

        // Act
        var tasks = new[]
        {
            viewModel.OnInitializedAsync(),
            viewModel.OnParametersSetAsync(),
            viewModel.OnAfterRenderAsync(true)
        };

        await Task.WhenAll(tasks);

        // Assert
        using var _ = new AssertionScope();
        viewModel.OnInitializedAsyncCalled.Should().BeTrue();
        viewModel.OnParametersSetAsyncCalled.Should().BeTrue();
        viewModel.OnAfterRenderAsyncCalled.Should().BeTrue();
    }

    // Test classes
    public class TestViewModelBase : ViewModelBase
    {
        public bool OnAfterRenderCalled { get; private set; }
        public bool OnAfterRenderFirstRender { get; private set; }
        public bool OnAfterRenderAsyncCalled { get; private set; }
        public bool OnAfterRenderAsyncFirstRender { get; private set; }
        public bool OnInitializedCalled { get; private set; }
        public bool OnInitializedAsyncCalled { get; private set; }
        public bool OnParametersSetCalled { get; private set; }
        public bool OnParametersSetAsyncCalled { get; private set; }
        public bool ShouldRenderResult { get; set; } = true;

        public override void OnAfterRender(bool firstRender)
        {
            OnAfterRenderCalled = true;
            OnAfterRenderFirstRender = firstRender;
        }

        public override Task OnAfterRenderAsync(bool firstRender)
        {
            OnAfterRenderAsyncCalled = true;
            OnAfterRenderAsyncFirstRender = firstRender;
            return Task.CompletedTask;
        }

        public override void OnInitialized()
        {
            OnInitializedCalled = true;
        }

        public override Task OnInitializedAsync()
        {
            OnInitializedAsyncCalled = true;
            return Task.CompletedTask;
        }

        public override void OnParametersSet()
        {
            OnParametersSetCalled = true;
        }

        public override Task OnParametersSetAsync()
        {
            OnParametersSetAsyncCalled = true;
            return Task.CompletedTask;
        }

        public override bool ShouldRender()
        {
            return ShouldRenderResult;
        }
    }

    public class DefaultViewModelBase : ViewModelBase
    {
        // Uses default implementations to test base behavior
    }
}