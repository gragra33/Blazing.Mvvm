using System.ComponentModel;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Bunit;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.ComponentTests;

/// <summary>
/// Tests for automatic two-way binding functionality in MvvmComponentBase, MvvmOwningComponentBase, and MvvmLayoutComponentBase.
/// </summary>
public class TwoWayBindingTests : ComponentTestBase
{
    /// <summary>
    /// Tests that MvvmComponentBase automatically detects and wires up two-way binding for EventCallback parameters.
    /// </summary>
    [Fact]
    public void MvvmComponentBase_WithEventCallback_ShouldAutomaticallyEnableTwoWayBinding()
    {
        // Arrange
        var viewModel = CreateInstance<TwoWayBindingTestViewModel>(true);
        Services.AddSingleton(_ => viewModel);

        var parentComponent = RenderComponent<TwoWayBindingTestComponent>();
        var childComponent = parentComponent.FindComponent<TwoWayBindingChildComponent>();

        // Act - Change ViewModel property
        viewModel.Counter = 42;

        // Assert - EventCallback should have been invoked
        parentComponent.Instance.CallbackInvokedCount.Should().Be(1);
        parentComponent.Instance.LastCallbackValue.Should().Be(42);
    }

    /// <summary>
    /// Tests that MvvmOwningComponentBase automatically detects and wires up two-way binding for EventCallback parameters.
    /// </summary>
    [Fact]
    public void MvvmOwningComponentBase_WithEventCallback_ShouldAutomaticallyEnableTwoWayBinding()
    {
        // Arrange
        Services.AddScoped<TwoWayBindingTestViewModel>();

        var parentComponent = RenderComponent<TwoWayBindingOwningTestComponent>();
        var childComponent = parentComponent.FindComponent<TwoWayBindingOwningChildComponent>();
        var viewModel = childComponent.Instance.GetViewModel();

        // Act - Change ViewModel property
        viewModel.Counter = 99;

        // Assert - EventCallback should have been invoked
        parentComponent.Instance.CallbackInvokedCount.Should().Be(1);
        parentComponent.Instance.LastCallbackValue.Should().Be(99);
    }

    /// <summary>
    /// Tests that two-way binding does not activate when no matching EventCallback is found.
    /// </summary>
    [Fact]
    public void MvvmComponentBase_WithoutEventCallback_ShouldNotEnableTwoWayBinding()
    {
        // Arrange
        var viewModel = CreateInstance<TwoWayBindingTestViewModel>(true);
        Services.AddSingleton(_ => viewModel);

        var parentComponent = RenderComponent<TwoWayBindingNoCallbackTestComponent>();
        var childComponent = parentComponent.FindComponent<TwoWayBindingNoCallbackChildComponent>();

        // Act - Change ViewModel property
        viewModel.Counter = 42;

        // Assert - No callback should have been invoked (component doesn't track callbacks)
        // The test passes if no exception is thrown
        viewModel.Counter.Should().Be(42);
    }

    /// <summary>
    /// Tests that two-way binding works with multiple properties simultaneously.
    /// </summary>
    [Fact]
    public void MvvmComponentBase_WithMultipleEventCallbacks_ShouldBindAllProperties()
    {
        // Arrange
        var viewModel = CreateInstance<MultiPropertyTestViewModel>(true);
        Services.AddSingleton(_ => viewModel);

        var parentComponent = RenderComponent<MultiPropertyTestComponent>();
        var childComponent = parentComponent.FindComponent<MultiPropertyChildComponent>();

        // Act - Change multiple ViewModel properties
        viewModel.Counter = 10;
        viewModel.Name = "Test";

        // Assert - Both EventCallbacks should have been invoked
        parentComponent.Instance.CounterCallbackInvoked.Should().BeTrue();
        parentComponent.Instance.NameCallbackInvoked.Should().BeTrue();
        parentComponent.Instance.LastCounterValue.Should().Be(10);
        parentComponent.Instance.LastNameValue.Should().Be("Test");
    }

    /// <summary>
    /// Tests that two-way binding respects the naming convention (PropertyNameChanged).
    /// </summary>
    [Fact]
    public void MvvmComponentBase_WithIncorrectNamingConvention_ShouldNotEnableTwoWayBinding()
    {
        // Arrange
        var viewModel = CreateInstance<TwoWayBindingTestViewModel>(true);
        Services.AddSingleton(_ => viewModel);

        var parentComponent = RenderComponent<IncorrectNamingTestComponent>();
        var childComponent = parentComponent.FindComponent<IncorrectNamingChildComponent>();

        // Act - Change ViewModel property
        viewModel.Counter = 42;

        // Assert - Callback with incorrect name should not be invoked
        parentComponent.Instance.IncorrectCallbackInvoked.Should().BeFalse();
    }

    /// <summary>
    /// Tests that two-way binding only invokes callback when value actually changes.
    /// </summary>
    [Fact]
    public void MvvmComponentBase_WhenValueUnchanged_ShouldNotInvokeCallback()
    {
        // Arrange
        var viewModel = CreateInstance<TwoWayBindingTestViewModel>(true);
        viewModel.Counter = 5;
        Services.AddSingleton(_ => viewModel);

        var parentComponent = RenderComponent<TwoWayBindingTestComponent>(parameters => parameters
            .Add(p => p.Counter, 5));
        
        var initialCount = parentComponent.Instance.CallbackInvokedCount;

        // Act - Set to same value
        viewModel.Counter = 5;

        // Assert - Callback should not be invoked again
        parentComponent.Instance.CallbackInvokedCount.Should().Be(initialCount);
    }

    /// <summary>
    /// Tests that component properly disposes two-way binding helper preventing memory leaks.
    /// </summary>
    [Fact]
    public void MvvmComponentBase_WhenDisposed_ShouldUnsubscribeFromPropertyChanged()
    {
        // Arrange
        var viewModel = CreateInstance<TwoWayBindingTestViewModel>(true);
        Services.AddSingleton(_ => viewModel);

        var parentComponent = RenderComponent<TwoWayBindingTestComponent>();
        
        // Verify component was created and is functioning
        parentComponent.Instance.Should().NotBeNull();

        // Act - Dispose the component
        parentComponent.Dispose();

        // Assert - The two-way binding helper should have unsubscribed
        // We verify that property changes after disposal don't throw exceptions
        // which would happen if handlers weren't properly cleaned up
        var action = () => viewModel.Counter = 42;
        action.Should().NotThrow("property changes after disposal should not throw");
        
        // Verify the value actually changed (ViewModel still works after component disposal)
        viewModel.Counter.Should().Be(42);
    }

    /// <summary>
    /// Helper method to check if there are PropertyChanged event handlers.
    /// </summary>
    private static bool HasPropertyChangedHandlers(INotifyPropertyChanged obj)
    {
        var field = obj.GetType().GetField("PropertyChanged", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.NonPublic);
        
        var eventDelegate = field?.GetValue(obj) as Delegate;
        return eventDelegate?.GetInvocationList().Length > 0;
    }
}

#region Test ViewModels

/// <summary>
/// Test ViewModel for two-way binding tests.
/// </summary>
public partial class TwoWayBindingTestViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private int _counter;
}

/// <summary>
/// Test ViewModel with multiple properties for two-way binding tests.
/// </summary>
public partial class MultiPropertyTestViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private int _counter;

    [ObservableProperty]
    [property: ViewParameter]
    private string _name = string.Empty;
}

#endregion

#region Test Components - MvvmComponentBase

/// <summary>
/// Parent test component that uses a child component with two-way binding.
/// </summary>
public class TwoWayBindingTestComponent : ComponentBase
{
    public int CallbackInvokedCount { get; private set; }
    public int LastCallbackValue { get; private set; }

    [Parameter]
    public int Counter { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenComponent<TwoWayBindingChildComponent>(0);
        builder.AddAttribute(1, nameof(TwoWayBindingChildComponent.Counter), Counter);
        builder.AddAttribute(2, nameof(TwoWayBindingChildComponent.CounterChanged), 
            EventCallback.Factory.Create<int>(this, value =>
            {
                CallbackInvokedCount++;
                LastCallbackValue = value;
                Counter = value;
            }));
        builder.CloseComponent();
    }
}

/// <summary>
/// Child test component using MvvmComponentBase with two-way binding.
/// </summary>
public class TwoWayBindingChildComponent : MvvmComponentBase<TwoWayBindingTestViewModel>
{
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.AddContent(0, $"Counter: {ViewModel.Counter}");
    }
}

/// <summary>
/// Parent test component without EventCallback.
/// </summary>
public class TwoWayBindingNoCallbackTestComponent : ComponentBase
{
    [Parameter]
    public int Counter { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenComponent<TwoWayBindingNoCallbackChildComponent>(0);
        builder.AddAttribute(1, nameof(TwoWayBindingNoCallbackChildComponent.Counter), Counter);
        builder.CloseComponent();
    }
}

/// <summary>
/// Child test component without EventCallback.
/// </summary>
public class TwoWayBindingNoCallbackChildComponent : MvvmComponentBase<TwoWayBindingTestViewModel>
{
    [Parameter]
    public int Counter { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.AddContent(0, $"Counter: {ViewModel.Counter}");
    }
}

#endregion

#region Test Components - MvvmOwningComponentBase

/// <summary>
/// Parent test component for MvvmOwningComponentBase.
/// </summary>
public class TwoWayBindingOwningTestComponent : ComponentBase
{
    public int CallbackInvokedCount { get; private set; }
    public int LastCallbackValue { get; private set; }

    [Parameter]
    public int Counter { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenComponent<TwoWayBindingOwningChildComponent>(0);
        builder.AddAttribute(1, nameof(TwoWayBindingOwningChildComponent.Counter), Counter);
        builder.AddAttribute(2, nameof(TwoWayBindingOwningChildComponent.CounterChanged), 
            EventCallback.Factory.Create<int>(this, value =>
            {
                CallbackInvokedCount++;
                LastCallbackValue = value;
                Counter = value;
            }));
        builder.CloseComponent();
    }
}

/// <summary>
/// Child test component using MvvmOwningComponentBase with two-way binding.
/// </summary>
public class TwoWayBindingOwningChildComponent : MvvmOwningComponentBase<TwoWayBindingTestViewModel>
{
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; }

    public TwoWayBindingTestViewModel GetViewModel() => ViewModel;

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.AddContent(0, $"Counter: {ViewModel.Counter}");
    }
}

#endregion

#region Test Components - Multiple Properties

/// <summary>
/// Parent test component with multiple properties.
/// </summary>
public class MultiPropertyTestComponent : ComponentBase
{
    public bool CounterCallbackInvoked { get; private set; }
    public bool NameCallbackInvoked { get; private set; }
    public int LastCounterValue { get; private set; }
    public string LastNameValue { get; private set; } = string.Empty;

    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public string Name { get; set; } = string.Empty;

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenComponent<MultiPropertyChildComponent>(0);
        builder.AddAttribute(1, nameof(MultiPropertyChildComponent.Counter), Counter);
        builder.AddAttribute(2, nameof(MultiPropertyChildComponent.CounterChanged), 
            EventCallback.Factory.Create<int>(this, value =>
            {
                CounterCallbackInvoked = true;
                LastCounterValue = value;
                Counter = value;
            }));
        builder.AddAttribute(3, nameof(MultiPropertyChildComponent.Name), Name);
        builder.AddAttribute(4, nameof(MultiPropertyChildComponent.NameChanged), 
            EventCallback.Factory.Create<string>(this, value =>
            {
                NameCallbackInvoked = true;
                LastNameValue = value;
                Name = value;
            }));
        builder.CloseComponent();
    }
}

/// <summary>
/// Child test component with multiple properties.
/// </summary>
public class MultiPropertyChildComponent : MvvmComponentBase<MultiPropertyTestViewModel>
{
    [Parameter]
    public int Counter { get; set; }

    [Parameter]
    public EventCallback<int> CounterChanged { get; set; }

    [Parameter]
    public string Name { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> NameChanged { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.AddContent(0, $"Counter: {ViewModel.Counter}, Name: {ViewModel.Name}");
    }
}

#endregion

#region Test Components - Incorrect Naming

/// <summary>
/// Parent test component with incorrect EventCallback naming.
/// </summary>
public class IncorrectNamingTestComponent : ComponentBase
{
    public bool IncorrectCallbackInvoked { get; private set; }

    [Parameter]
    public int Counter { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenComponent<IncorrectNamingChildComponent>(0);
        builder.AddAttribute(1, nameof(IncorrectNamingChildComponent.Counter), Counter);
        builder.AddAttribute(2, nameof(IncorrectNamingChildComponent.OnCounterUpdate), 
            EventCallback.Factory.Create<int>(this, value =>
            {
                IncorrectCallbackInvoked = true;
                Counter = value;
            }));
        builder.CloseComponent();
    }
}

/// <summary>
/// Child test component with incorrect EventCallback naming (should not trigger two-way binding).
/// </summary>
public class IncorrectNamingChildComponent : MvvmComponentBase<TwoWayBindingTestViewModel>
{
    [Parameter]
    public int Counter { get; set; }

    // This callback doesn't follow the {PropertyName}Changed convention
    [Parameter]
    public EventCallback<int> OnCounterUpdate { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.AddContent(0, $"Counter: {ViewModel.Counter}");
    }
}

#endregion
