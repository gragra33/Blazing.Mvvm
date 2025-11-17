using System.Reflection;
using Blazing.Mvvm.Components;

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="LibraryConfiguration"/> covering default values, property setters, method chaining, and assembly registration scenarios.
/// </summary>
public class LibraryConfigurationTests
{
    /// <summary>
    /// Tests that the constructor initializes default values.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var config = new LibraryConfiguration();

        // Assert
        using var _ = new AssertionScope();
        config.HostingModelType.Should().Be(BlazorHostingModelType.NotSpecified);
        config.ParameterResolutionMode.Should().Be(ParameterResolutionMode.None);
        config.ViewModelAssemblies.Should().BeEmpty();
        config.BasePath.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="HostingModelType"/> is settable.
    /// </summary>
    [Fact]
    public void HostingModelType_ShouldBeSettable()
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act
        config.HostingModelType = BlazorHostingModelType.WebAssembly;

        // Assert
        config.HostingModelType.Should().Be(BlazorHostingModelType.WebAssembly);
    }

    /// <summary>
    /// Tests that <see cref="ParameterResolutionMode"/> is settable.
    /// </summary>
    [Fact]
    public void ParameterResolutionMode_ShouldBeSettable()
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act
        config.ParameterResolutionMode = ParameterResolutionMode.ViewModel;

        // Assert
        config.ParameterResolutionMode.Should().Be(ParameterResolutionMode.ViewModel);
    }

    /// <summary>
    /// Tests that <see cref="BasePath"/> is settable.
    /// </summary>
    [Fact]
    public void BasePath_ShouldBeSettable()
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act
        config.BasePath = "/myapp";

        // Assert
        config.BasePath.Should().Be("/myapp");
    }

    /// <summary>
    /// Tests that registering view models from a generic type adds the assembly.
    /// </summary>
    [Fact]
    public void RegisterViewModelsFromAssemblyContaining_GivenGenericType_ShouldAddAssembly()
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act
        config.RegisterViewModelsFromAssemblyContaining<LibraryConfigurationTests>();

        // Assert
        config.ViewModelAssemblies.Should().Contain(typeof(LibraryConfigurationTests).Assembly);
    }

    /// <summary>
    /// Tests that registering view models from a type adds the assembly.
    /// </summary>
    [Fact]
    public void RegisterViewModelsFromAssemblyContaining_GivenType_ShouldAddAssembly()
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act
        config.RegisterViewModelsFromAssemblyContaining(typeof(LibraryConfigurationTests));

        // Assert
        config.ViewModelAssemblies.Should().Contain(typeof(LibraryConfigurationTests).Assembly);
    }

    /// <summary>
    /// Tests that registering view models from a single assembly adds the assembly.
    /// </summary>
    [Fact]
    public void RegisterViewModelsFromAssembly_GivenSingleAssembly_ShouldAddAssembly()
    {
        // Arrange
        var config = new LibraryConfiguration();
        var assembly = typeof(LibraryConfigurationTests).Assembly;

        // Act
        config.RegisterViewModelsFromAssembly(assembly);

        // Assert
        config.ViewModelAssemblies.Should().Contain(assembly);
    }

    /// <summary>
    /// Tests that registering view models from multiple assemblies adds all assemblies.
    /// </summary>
    [Fact]
    public void RegisterViewModelsFromAssembly_GivenMultipleAssemblies_ShouldAddAllAssemblies()
    {
        // Arrange
        var config = new LibraryConfiguration();
        var assembly1 = typeof(LibraryConfigurationTests).Assembly;
        var assembly2 = typeof(string).Assembly;

        // Act
        config.RegisterViewModelsFromAssembly(assembly1, assembly2);

        // Assert
        using var _ = new AssertionScope();
        config.ViewModelAssemblies.Should().Contain(assembly1);
        config.ViewModelAssemblies.Should().Contain(assembly2);
    }

    /// <summary>
    /// Tests that registering view models from an enumerable adds all assemblies.
    /// </summary>
    [Fact]
    public void RegisterViewModelsFromAssemblies_GivenEnumerable_ShouldAddAllAssemblies()
    {
        // Arrange
        var config = new LibraryConfiguration();
        var assemblies = new[]
        {
            typeof(LibraryConfigurationTests).Assembly,
            typeof(string).Assembly
        };

        // Act
        config.RegisterViewModelsFromAssemblies(assemblies);

        // Assert
        using var _ = new AssertionScope();
        config.ViewModelAssemblies.Should().Contain(assemblies[0]);
        config.ViewModelAssemblies.Should().Contain(assemblies[1]);
    }

    /// <summary>
    /// Tests that duplicate assemblies are only added once.
    /// </summary>
    [Fact]
    public void RegisterViewModelsFromAssemblies_GivenDuplicateAssemblies_ShouldOnlyAddOnce()
    {
        // Arrange
        var config = new LibraryConfiguration();
        var assembly = typeof(LibraryConfigurationTests).Assembly;

        // Act
        config.RegisterViewModelsFromAssembly(assembly);
        config.RegisterViewModelsFromAssembly(assembly); // Add duplicate

        // Assert
        config.ViewModelAssemblies.Should().ContainSingle(a => a == assembly);
    }

    /// <summary>
    /// Tests that multiple calls to register view models from assemblies add distinct assemblies.
    /// </summary>
    [Fact]
    public void RegisterViewModelsFromAssemblyContaining_GivenMultipleCalls_ShouldAddDistinctAssemblies()
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act
        config.RegisterViewModelsFromAssemblyContaining<LibraryConfigurationTests>();
        config.RegisterViewModelsFromAssemblyContaining<string>();
        config.RegisterViewModelsFromAssemblyContaining<LibraryConfigurationTests>(); // Duplicate

        // Assert
        using var _ = new AssertionScope();
        config.ViewModelAssemblies.Should().HaveCount(2);
        config.ViewModelAssemblies.Should().Contain(typeof(LibraryConfigurationTests).Assembly);
        config.ViewModelAssemblies.Should().Contain(typeof(string).Assembly);
    }

    /// <summary>
    /// Tests that <see cref="ViewModelAssemblies"/> is a read-write collection.
    /// </summary>
    [Fact]
    public void ViewModelAssemblies_ShouldBeReadWriteCollection()
    {
        // Arrange
        var config = new LibraryConfiguration();
        var assembly = typeof(LibraryConfigurationTests).Assembly;

        // Act
        config.ViewModelAssemblies.Add(assembly);

        // Assert
        config.ViewModelAssemblies.Should().Contain(assembly);
    }

    /// <summary>
    /// Tests that <see cref="HostingModelType"/> accepts all valid values.
    /// </summary>
    [Theory]
    [InlineData(BlazorHostingModelType.NotSpecified)]
    [InlineData(BlazorHostingModelType.WebAssembly)]
    [InlineData(BlazorHostingModelType.Server)]
    [InlineData(BlazorHostingModelType.WebApp)]
    [InlineData(BlazorHostingModelType.Hybrid)]
    [InlineData(BlazorHostingModelType.HybridMaui)]
    public void HostingModelType_ShouldAcceptAllValidValues(BlazorHostingModelType hostingType)
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act
        config.HostingModelType = hostingType;

        // Assert
        config.HostingModelType.Should().Be(hostingType);
    }

    /// <summary>
    /// Tests that <see cref="ParameterResolutionMode"/> accepts all valid values.
    /// </summary>
    [Theory]
    [InlineData(ParameterResolutionMode.None)]
    [InlineData(ParameterResolutionMode.ViewModel)]
    [InlineData(ParameterResolutionMode.ViewAndViewModel)]
    public void ParameterResolutionMode_ShouldAcceptAllValidValues(ParameterResolutionMode resolutionMode)
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act
        config.ParameterResolutionMode = resolutionMode;

        // Assert
        config.ParameterResolutionMode.Should().Be(resolutionMode);
    }

    /// <summary>
    /// Tests that <see cref="BasePath"/> accepts various path formats.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("/app")]
    [InlineData("/app/")]
    [InlineData("app")]
    [InlineData("app/")]
    [InlineData("/my-app/sub-path")]
    public void BasePath_ShouldAcceptVariousPathFormats(string? basePath)
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act
        config.BasePath = basePath;

        // Assert
        config.BasePath.Should().Be(basePath);
    }

    /// <summary>
    /// Tests that configuration supports method chaining.
    /// </summary>
    [Fact]
    public void Configuration_ShouldSupportMethodChaining()
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act & Assert - Should not throw
        var act = () =>
        {
            config.RegisterViewModelsFromAssemblyContaining<LibraryConfigurationTests>();
            config.RegisterViewModelsFromAssemblyContaining<string>();
        };

        act.Should().NotThrow();
        config.ViewModelAssemblies.Should().HaveCount(2);
    }

    /// <summary>
    /// Tests that <see cref="ViewModelAssemblies"/> is empty when no assemblies are registered.
    /// </summary>
    [Fact]
    public void ViewModelAssemblies_WhenEmpty_ShouldBeEmptyCollection()
    {
        // Arrange
        var config = new LibraryConfiguration();

        // Act & Assert
        config.ViewModelAssemblies.Should().BeEmpty();
        config.ViewModelAssemblies.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that registering view models from an empty enumerable does not throw.
    /// </summary>
    [Fact]
    public void RegisterViewModelsFromAssemblies_GivenEmptyEnumerable_ShouldNotThrow()
    {
        // Arrange
        var config = new LibraryConfiguration();
        var emptyAssemblies = Enumerable.Empty<Assembly>();

        // Act
        var act = () => config.RegisterViewModelsFromAssemblies(emptyAssemblies);

        // Assert
        act.Should().NotThrow();
        config.ViewModelAssemblies.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that registering view models from an empty array does not throw.
    /// </summary>
    [Fact]
    public void RegisterViewModelsFromAssembly_GivenEmptyArray_ShouldNotThrow()
    {
        // Arrange
        var config = new LibraryConfiguration();
        var emptyAssemblies = Array.Empty<Assembly>();

        // Act
        var act = () => config.RegisterViewModelsFromAssembly(emptyAssemblies);

        // Assert
        act.Should().NotThrow();
        config.ViewModelAssemblies.Should().BeEmpty();
    }
}