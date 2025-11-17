using Blazing.Mvvm.Components;
using Blazing.Mvvm.Components.Parameter;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterResolver"/> covering parameter resolution modes and property setting scenarios for Views and ViewModels.
/// </summary>
public partial class ParameterResolverTests : ComponentTestBase
{
    private static readonly Dictionary<string, ComponentParameter> Parameters = new()
    {
        { nameof(IParameterTestView.Parameter1), ComponentParameter.CreateParameter(nameof(IParameterTestView.Parameter1), "Parameter1Value") },
        { nameof(IParameterTestView.Parameter2), ComponentParameter.CreateParameter(nameof(IParameterTestView.Parameter2), 10)  },
        { nameof(IParameterTestView.Parameter3), ComponentParameter.CreateParameter(nameof(IParameterTestView.Parameter3), null) },
        { nameof(IParameterTestView.ParentView), ComponentParameter. CreateCascadingValue(null, new TestParameterView()) },
        { IParameterTestView.NamedCascadingParameterName, ComponentParameter.CreateCascadingValue(IParameterTestView.NamedCascadingParameterName, new TestParameterViewModel()) },
        { nameof(IParameterTestView.QueryParameter), ComponentParameter.CreateCascadingValue(nameof(IParameterTestView.QueryParameter), "namedQueryParameterValue") },
        { IParameterTestView.NamedQueryParameterName, ComponentParameter.CreateCascadingValue(IParameterTestView.NamedQueryParameterName, DateOnly.FromDateTime(DateTime.Now)) }
    };

    /// <summary>
    /// Tests that when parameter resolution mode is None, only view properties are set.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenParameterResolutionModeIsNone_ShouldSetViewProperties()
    {
        // Arrange
        Services.AddSingleton<TestParameterViewModel>();
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.None));
        var viewModel = GetViewModel<TestParameterViewModel>();

        // Act
        var view = RenderAndSetParameters<TestParameterView>();

        // Assert
        using var _ = new AssertionScope();
        AssertThatViewPropertiesAreSet(view.Instance);
        AssertThatViewModelPropertiesAreNotSet(viewModel);
    }

    /// <summary>
    /// Tests that when parameter resolution mode is ViewModel, only view model properties are set.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenParameterResolutionModeIsViewModel_ShouldSetViewModelPropertiesOnly()
    {
        // Arrange
        Services.AddSingleton<TestParameterViewModel>();
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewModel));
        var viewModel = GetViewModel<TestParameterViewModel>();

        // Act
        var view = RenderAndSetParameters<TestParameterView>();

        // Assert
        using var _ = new AssertionScope();
        AssertThatViewModelPropertiesAreSet(viewModel);
        AssertThatViewPropertiesAreNotSet(view.Instance);
    }

    /// <summary>
    /// Tests that when parameter resolution mode is ViewAndViewModel, both view and view model properties are set.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenParameterResolutionModeIsViewAndViewModel_ShouldSetViewAndViewModelProperties()
    {
        // Arrange
        Services.AddSingleton<TestParameterViewModel>();
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewAndViewModel));
        var viewModel = GetViewModel<TestParameterViewModel>();

        // Act
        var view = RenderAndSetParameters<TestParameterView>();

        // Assert
        using var _ = new AssertionScope();
        AssertThatViewPropertiesAreSet(view.Instance);
        AssertThatViewModelPropertiesAreSet(viewModel);
    }

    /// <summary>
    /// Tests that when parameter resolution mode is None and component inherits LayoutComponentBase, only view properties are set.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenParameterResolutionModeIsNoneAndComponentInheritsLayoutComponentBase_ShouldSetViewProperties()
    {
        // Arrange
        Services.AddSingleton<TestParameterViewModel>();
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.None));
        var viewModel = GetViewModel<TestParameterViewModel>();

        // Act
        var view = RenderAndSetParameters<TestParameterLayoutView>();

        // Assert
        using var _ = new AssertionScope();
        AssertThatViewPropertiesAreSet(view.Instance);
        AssertThatViewModelPropertiesAreNotSet(viewModel);
    }

    /// <summary>
    /// Tests that when parameter resolution mode is ViewModel and component inherits LayoutComponentBase, only view model properties are set.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenParameterResolutionModeIsSetViewModelAndComponentInheritsLayoutComponentBase_ShouldSetViewModelPropertiesOnly()
    {
        // Arrange
        Services.AddSingleton<TestParameterViewModel>();
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewModel));
        var viewModel = GetViewModel<TestParameterViewModel>();

        // Act
        var view = RenderAndSetParameters<TestParameterLayoutView>();

        // Assert
        using var _ = new AssertionScope();
        AssertThatViewModelPropertiesAreSet(viewModel);
        AssertThatViewPropertiesAreNotSet(view.Instance);
    }

    /// <summary>
    /// Tests that when parameter resolution mode is ViewAndViewModel and component inherits LayoutComponentBase, both view and view model properties are set.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenParameterResolutionModeIsSetViewAndViewModelAndComponentInheritsLayoutComponentBase_ShouldSetViewAndViewModelProperties()
    {
        // Arrange
        Services.AddSingleton<TestParameterViewModel>();
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewAndViewModel));
        var viewModel = GetViewModel<TestParameterViewModel>();

        // Act
        var view = RenderAndSetParameters<TestParameterLayoutView>();

        // Assert
        using var _ = new AssertionScope();
        AssertThatViewPropertiesAreSet(view.Instance);
        AssertThatViewModelPropertiesAreSet(viewModel);
    }

    /// <summary>
    /// Tests that when parameter resolution mode is None and component inherits OwningComponentBase, only view properties are set.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenParameterResolutionModeIsNoneAndComponentInheritsOwingComponentBase_ShouldSetViewProperties()
    {
        // Arrange
        Services.AddScoped(_ => CreateInstance<TestParameterViewModel>(true));
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.None));
        var viewModel = GetViewModel<TestParameterViewModel>();

        // Act
        var view = RenderAndSetParameters<TestParameterOwingView>();

        // Assert
        using var _ = new AssertionScope();
        AssertThatViewPropertiesAreSet(view.Instance);
        AssertThatViewModelPropertiesAreNotSet(viewModel);
    }

    /// <summary>
    /// Tests that when parameter resolution mode is ViewModel and component inherits OwningComponentBase, only view model properties are set.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenParameterResolutionModeIsViewModelAndComponentInheritsOwingComponentBase_ShouldSetViewModelPropertiesOnly()
    {
        // Arrange
        Services.AddScoped(_ => CreateInstance<TestParameterViewModel>(true));
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewModel));
        var viewModel = GetViewModel<TestParameterViewModel>();

        // Act
        var view = RenderAndSetParameters<TestParameterOwingView>();

        // Assert
        using var _ = new AssertionScope();
        AssertThatViewModelPropertiesAreSet(viewModel);
        AssertThatViewPropertiesAreNotSet(view.Instance);
    }

    /// <summary>
    /// Tests that when parameter resolution mode is ViewAndViewModel and component inherits OwningComponentBase, both view and view model properties are set.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenParameterResolutionModeIsViewAndViewModelAndComponentInheritsOwingComponentBase_ShouldSetViewAndViewModelProperties()
    {
        // Arrange
        Services.AddScoped(_ => CreateInstance<TestParameterViewModel>(true));
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewAndViewModel));
        var viewModel = GetViewModel<TestParameterViewModel>();

        // Act
        var view = RenderAndSetParameters<TestParameterOwingView>();

        // Assert
        using var _ = new AssertionScope();
        AssertThatViewPropertiesAreSet(view.Instance);
        AssertThatViewModelPropertiesAreSet(viewModel);
    }

    /// <summary>
    /// Tests that duplicate view parameter keys on the view model throw <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenDuplicateViewParameterKeysOnViewModel_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Services.AddSingleton<TestParameterDuplicateKeyViewModel>();
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewModel));

        // Act
        Action act = () => RenderComponent<TestParameterViewDuplicateKeyOnViewModel>(
            parameters => parameters.Add(p => p.Parameter1, "Parameter1"));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage($"Duplicate parameter name 'Parameter1' found on type '{typeof(TestParameterDuplicateKeyViewModel).FullName}'.");
    }

    /// <summary>
    /// Tests that properties without a setter throw <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void GivenSetParameters_WhenPropertyDoesNotHaveASetter_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Services.AddSingleton<TestParameterNoSetterViewModel>();
        Services.AddSingleton<IParameterResolver>(_ => new ParameterResolver(ParameterResolutionMode.ViewModel));

        // Act
        Action act = () => RenderComponent<TestParameterNoSetterOnViewModel>(
            parameters => parameters.Add(p => p.Parameter1, "Parameter1"));

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage($"The property 'Parameter1' on type '{typeof(TestParameterNoSetterViewModel).FullName}' does not have a setter.");
    }

    private static void AssertThatViewPropertiesAreSet(IParameterTestView view)
    {
        view.Parameter1.Should().Be(GetParamaterValue<string>(nameof(IParameterTestView.Parameter1)));
        view.Parameter2.Should().Be(GetParamaterValue<int>(nameof(IParameterTestView.Parameter2)));
        view.Parameter3.Should().BeNull();
        view.ParentView.Should().Be(GetParamaterValue<IParameterTestView>(nameof(IParameterTestView.ParentView)));
        view.ParentViewModel.Should().Be(GetParamaterValue<TestParameterViewModel>(IParameterTestView.NamedCascadingParameterName));
        view.QueryParameter.Should().Be(GetParamaterValue<string>(nameof(IParameterTestView.QueryParameter)));
        view.QueryParameter2.Should().Be(GetParamaterValue<DateOnly>(IParameterTestView.NamedQueryParameterName));
    }

    private static void AssertThatViewPropertiesAreNotSet(IParameterTestView view)
    {
        view.Parameter1.Should().BeNull();
        view.Parameter2.Should().Be(default);
        view.Parameter3.Should().BeNull();
        view.ParentView.Should().BeNull();
        view.ParentViewModel.Should().BeNull();
        view.QueryParameter.Should().BeNull();
        view.QueryParameter2.Should().BeNull();
    }

    private static void AssertThatViewModelPropertiesAreSet(TestParameterViewModel viewModel)
    {
        viewModel.Parameter1.Should().Be(GetParamaterValue<string>(nameof(IParameterTestView.Parameter1)));
        viewModel.GetProperty1().Should().Be(GetParamaterValue<int>(nameof(IParameterTestView.Parameter2)));
        viewModel.Parameter3.Should().BeNull();
        viewModel.ParentView.Should().Be(GetParamaterValue<IParameterTestView>(nameof(IParameterTestView.ParentView)));
        viewModel.AncestorViewModel.Should().Be(GetParamaterValue<TestParameterViewModel>(IParameterTestView.NamedCascadingParameterName));
        viewModel.QueryParameter.Should().Be(GetParamaterValue<string>(nameof(IParameterTestView.QueryParameter)));
        viewModel.GetNamedQueryParameter().Should().Be(GetParamaterValue<DateOnly>(IParameterTestView.NamedQueryParameterName));
    }

    private static void AssertThatViewModelPropertiesAreNotSet(TestParameterViewModel viewModel)
    {
        viewModel.Parameter1.Should().BeNull();
        viewModel.GetProperty1().Should().Be(default);
        viewModel.Parameter3.Should().BeNull();
        viewModel.ParentView.Should().BeNull();
        viewModel.AncestorViewModel.Should().BeNull();
        viewModel.QueryParameter.Should().BeNull();
        viewModel.GetNamedQueryParameter().Should().BeNull();
    }

    private static T GetParamaterValue<T>(string name)
        where T : notnull
    {
        if (!Parameters.TryGetValue(name, out var value))
        {
            throw new KeyNotFoundException($"The parameter '{name}' was not found.");
        }

        if (value.Value is null)
        {
            throw new ArgumentNullException($"The parameter '{name}' was null.");
        }

        return (T)value.Value;
    }

    private IRenderedComponent<T> RenderAndSetParameters<T>()
        where T : IComponent
    {
        // We need to navigate to the component to ensure that the query parameters are set.
        // https://bunit.dev/docs/providing-input/passing-parameters-to-components.html?tabs=csharp#passing-query-parameters-supplyparameterfromquery-to-a-component
        var fakeNavigationManager = Services.GetRequiredService<NavigationManager>();
        var uri = fakeNavigationManager.GetUriWithQueryParameters(new Dictionary<string, object?>
        {
            { nameof(IParameterTestView.QueryParameter), Parameters[nameof(IParameterTestView.QueryParameter)].Value},
            { IParameterTestView.NamedQueryParameterName,  Parameters[IParameterTestView.NamedQueryParameterName].Value}
        });
        fakeNavigationManager.NavigateTo(uri);

        return RenderComponent<T>([.. Parameters.Values]);
    }
}
