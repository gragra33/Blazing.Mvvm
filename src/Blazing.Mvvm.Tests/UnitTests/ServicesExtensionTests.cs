using Blazing.Mvvm.Components;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Unit tests for MVVM service extension methods, verifying service registration and configuration scenarios.
/// </summary>
public class ServicesExtensionTests
{
    /// <summary>
    /// Tests that AddMvvm registers required services in the service collection.
    /// </summary>
    [Fact]
    public void GivenAddMvvm_WhenServicesAdded_ThenShouldContainRequiredServices()
    {
        // Arrange
        var mvvmNavigationServiceDescriptor = ServiceDescriptor.Singleton<IMvvmNavigationManager, MvvmNavigationManager>();
        var parameterResolverServiceDescriptor = ServiceDescriptor.Singleton<IParameterResolver>(_ => default!);
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm();

        // Assert
        using var _ = new AssertionScope();
        sut.Contains(mvvmNavigationServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
        sut.Contains(parameterResolverServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
    }

    /// <summary>
    /// Tests that AddMvvm registers services with the correct lifetime based on hosting model type.
    /// </summary>
    [Theory]
    [InlineData(BlazorHostingModelType.NotSpecified, ServiceLifetime.Singleton)]
    [InlineData(BlazorHostingModelType.WebAssembly, ServiceLifetime.Singleton)]
    [InlineData(BlazorHostingModelType.Hybrid, ServiceLifetime.Singleton)]
    [InlineData(BlazorHostingModelType.WebApp, ServiceLifetime.Scoped)]
    [InlineData(BlazorHostingModelType.Server, ServiceLifetime.Scoped)]
    [InlineData(BlazorHostingModelType.HybridMaui, ServiceLifetime.Scoped)]
    public void GivenAddMvvm_WhenHostingModelTypeConfigured_ThenShouldContainRequiredServices(BlazorHostingModelType blazorHostingModel, ServiceLifetime serviceLifetime)
    {
        // Arrange
        var mvvmNavigationServiceDescriptor = ServiceDescriptor.Describe(typeof(IMvvmNavigationManager), typeof(MvvmNavigationManager), serviceLifetime);
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.HostingModelType = blazorHostingModel);

        // Assert
        sut.Contains(mvvmNavigationServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
    }

    /// <summary>
    /// Tests that AddMvvm registers view models from the calling assembly.
    /// </summary>
    [Theory]
    [MemberData(nameof(ServicesExtensionTestData.ViewModelsInCallingAssembly), MemberType = typeof(ServicesExtensionTestData))]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromCallingAssembly_ThenShouldContainViewModel(ServiceDescriptor vmServiceDescriptor)
    {
        // Arrange
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm();

        // Assert
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
    }

    /// <summary>
    /// Tests that AddMvvm registers view models from the assembly containing a generic type.
    /// </summary>
    [Theory]
    [MemberData(nameof(ServicesExtensionTestData.ViewModelsInCallingAssembly), MemberType = typeof(ServicesExtensionTestData))]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromAssemblyContainingGenericType_ThenShouldContainViewModel(ServiceDescriptor vmServiceDescriptor)
    {
        // Arrange
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblyContaining<ServicesExtensionTests>());

        // Assert
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
    }

    /// <summary>
    /// Tests that AddMvvm registers view models from the assembly containing a specified type.
    /// </summary>
    [Theory]
    [MemberData(nameof(ServicesExtensionTestData.ViewModelsInCallingAssembly), MemberType = typeof(ServicesExtensionTestData))]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromAssemblyContainingType_ThenShouldContainViewModel(ServiceDescriptor vmServiceDescriptor)
    {
        // Arrange
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblyContaining(typeof(ServicesExtensionTests)));

        // Assert
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
    }

    /// <summary>
    /// Tests that AddMvvm registers view models from a specified assembly.
    /// </summary>
    [Theory]
    [MemberData(nameof(ServicesExtensionTestData.ViewModelsInCallingAssembly), MemberType = typeof(ServicesExtensionTestData))]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromSpecifiedAssembly_ThenShouldContainViewModel(ServiceDescriptor vmServiceDescriptor)
    {
        // Arrange
        var assembly = typeof(ServicesExtensionTests).Assembly;
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssembly(assembly));

        // Assert
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
    }

    /// <summary>
    /// Tests that AddMvvm registers view models from specified assemblies.
    /// </summary>
    [Theory]
    [MemberData(nameof(ServicesExtensionTestData.ViewModelsInCallingAssembly), MemberType = typeof(ServicesExtensionTestData))]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromSpecifiedAssemblies_ThenShouldContainViewModel(ServiceDescriptor vmServiceDescriptor)
    {
        // Arrange
        var assemblies = new[] { typeof(ServicesExtensionTests).Assembly };
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblies(assemblies));

        // Assert
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
    }

    /// <summary>
    /// Tests that AddMvvm registers transient view models from the fixture assembly marker type.
    /// </summary>
    [Theory]
    [MemberData(nameof(ServicesExtensionTestData.ViewModelsInDependentAssembly), MemberType = typeof(ServicesExtensionTestData))]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromDependentAssemblyContainingType_ThenShouldContainTransientViewModels(ServiceDescriptor vmServiceDescriptor)
    {
        // Arrange
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblyContaining<Blazing.Mvvm.Sample.WebApp.Client._Imports>());

        // Assert
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
    }

    private static class ServicesExtensionTestData
    {
        public static TheoryData<ServiceDescriptor> ViewModelsInCallingAssembly = new()
        {
            { ServiceDescriptor.Transient<TestViewModel, TestViewModel>() },
            { ServiceDescriptor.Transient<TransientTestViewModel, TransientTestViewModel>() },
            { ServiceDescriptor.Transient<ITransientTestViewModel, TransientTestViewModel>() },
            { ServiceDescriptor.Transient<AbstractBaseViewModel, ConcreteViewModel>() },
            { ServiceDescriptor.KeyedTransient<TransientKeyedTestViewModel, TransientKeyedTestViewModel>(nameof(TransientKeyedTestViewModel)) },

            { ServiceDescriptor.Scoped<ScopedTestViewModel, ScopedTestViewModel>() },
            { ServiceDescriptor.Scoped<IScopedTestViewModel, ScopedTestViewModel>() },
            { ServiceDescriptor.KeyedScoped<ScopedKeyedTestViewModel, ScopedKeyedTestViewModel>(nameof(ScopedKeyedTestViewModel)) },

            { ServiceDescriptor.Singleton<SingletonTestViewModel, SingletonTestViewModel>() },
            { ServiceDescriptor.Singleton<ISingletonTestViewModel, SingletonTestViewModel>() },
            { ServiceDescriptor.KeyedSingleton<ISingletonTestViewModel, SingletonTestViewModel>(nameof(SingletonTestViewModel)) },
            { ServiceDescriptor.KeyedSingleton<SingletonKeyedTestViewModel, SingletonKeyedTestViewModel>(nameof(SingletonKeyedTestViewModel)) }
        };

        public static TheoryData<ServiceDescriptor> ViewModelsInDependentAssembly = new()
        {
            { ServiceDescriptor.Transient<Blazing.Mvvm.Sample.WebApp.Client.ViewModels.EditContactViewModel, Blazing.Mvvm.Sample.WebApp.Client.ViewModels.EditContactViewModel>() },
            { ServiceDescriptor.Transient<Blazing.Mvvm.Sample.WebApp.Client.ViewModels.HexEntryViewModel, Blazing.Mvvm.Sample.WebApp.Client.ViewModels.HexEntryViewModel>() },
            { ServiceDescriptor.Transient<Blazing.Mvvm.Sample.WebApp.Client.ViewModels.ITestNavigationViewModel, Blazing.Mvvm.Sample.WebApp.Client.ViewModels.TestNavigationViewModel>() },
            { ServiceDescriptor.KeyedTransient<Blazing.Mvvm.Sample.WebApp.Client.ViewModels.ITestKeyedNavigationViewModel, Blazing.Mvvm.Sample.WebApp.Client.ViewModels.TestKeyedNavigationViewModel>(nameof(Blazing.Mvvm.Sample.WebApp.Client.ViewModels.TestKeyedNavigationViewModel)) },
            { ServiceDescriptor.Transient<Blazing.Mvvm.Sample.WebApp.Client.ViewModels.MainLayoutViewModel, Blazing.Mvvm.Sample.WebApp.Client.ViewModels.MainLayoutViewModel>() },
            { ServiceDescriptor.Transient<Blazing.Mvvm.Sample.WebApp.Client.ViewModels.TextEntryViewModel, Blazing.Mvvm.Sample.WebApp.Client.ViewModels.TextEntryViewModel>() },
            { ServiceDescriptor.KeyedTransient<Blazing.Mvvm.Sample.WebApp.Client.ViewModels.HexTranslateViewModel, Blazing.Mvvm.Sample.WebApp.Client.ViewModels.HexTranslateViewModel>(nameof(Blazing.Mvvm.Sample.WebApp.Client.ViewModels.HexTranslateViewModel)) },

            { ServiceDescriptor.Scoped<Blazing.Mvvm.Sample.WebApp.Client.ViewModels.FetchDataViewModel, Blazing.Mvvm.Sample.WebApp.Client.ViewModels.FetchDataViewModel>() },

            { ServiceDescriptor.Singleton<Blazing.Mvvm.Sample.WebApp.Client.ViewModels.CounterViewModel, Blazing.Mvvm.Sample.WebApp.Client.ViewModels.CounterViewModel>() }
        };
    }
}
