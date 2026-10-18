using Blazing.Mvvm.Components;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using DependentAssemblyViewModels = Blazing.Mvvm.Tests.Assets.ViewModels;
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
        sut.Contains(mvvmNavigationServiceDescriptor, ServiceDescriptorComparer.Comparer).ShouldBeTrue();
        sut.Contains(parameterResolverServiceDescriptor, ServiceDescriptorComparer.Comparer).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that AddMvvm registers the transient ViewModel factory only when <see cref="LibraryConfiguration.DisposeTransientViewModels"/> is enabled.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenAddMvvm_WhenDisposeTransientViewModelsConfigured_ThenFactoryRegisteredOnlyWhenEnabled(bool isEnabled)
    {
        // Arrange
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.DisposeTransientViewModels = isEnabled);

        // Assert
        sut.Any(descriptor => descriptor.ServiceType == typeof(TransientViewModelFactory)).ShouldBe(isEnabled);
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
        sut.Contains(mvvmNavigationServiceDescriptor, ServiceDescriptorComparer.Comparer).ShouldBeTrue();
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
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).ShouldBeTrue();
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
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).ShouldBeTrue();
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
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).ShouldBeTrue();
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
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).ShouldBeTrue();
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
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).ShouldBeTrue();
    }

    /// <summary>
    /// Tests that AddMvvm registers transient view models from a dependent assembly containing a type.
    /// </summary>
    [Theory]
    [MemberData(nameof(ServicesExtensionTestData.ViewModelsInDependentAssembly), MemberType = typeof(ServicesExtensionTestData))]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromDependentAssemblyContainingType_ThenShouldContainTransientViewModels(ServiceDescriptor vmServiceDescriptor)
    {
        // Arrange
        var sut = new ServiceCollection();
        typeof(ServicesExtensionTests).Assembly.ShouldNotBeSameAs(typeof(DependentAssemblyViewModels.CounterViewModel).Assembly);

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblyContaining<DependentAssemblyViewModels.CounterViewModel>());

        // Assert
        sut.Contains(vmServiceDescriptor, ServiceDescriptorComparer.Comparer).ShouldBeTrue();
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
            { ServiceDescriptor.Transient<DependentAssemblyViewModels.TestViewModel, DependentAssemblyViewModels.TestViewModel>() },
            { ServiceDescriptor.Transient<DependentAssemblyViewModels.ITransientTestViewModel, DependentAssemblyViewModels.TransientTestViewModel>() },
            { ServiceDescriptor.Transient<DependentAssemblyViewModels.TransientTestViewModel, DependentAssemblyViewModels.TransientTestViewModel>() },
            { ServiceDescriptor.KeyedTransient<DependentAssemblyViewModels.TransientKeyedTestViewModel, DependentAssemblyViewModels.TransientKeyedTestViewModel>(nameof(DependentAssemblyViewModels.TransientKeyedTestViewModel)) },
            { ServiceDescriptor.Scoped<DependentAssemblyViewModels.IScopedTestViewModel, DependentAssemblyViewModels.ScopedTestViewModel>() },
            { ServiceDescriptor.Scoped<DependentAssemblyViewModels.ScopedTestViewModel, DependentAssemblyViewModels.ScopedTestViewModel>() },
            { ServiceDescriptor.Singleton<DependentAssemblyViewModels.CounterViewModel, DependentAssemblyViewModels.CounterViewModel>() },
            { ServiceDescriptor.Singleton<DependentAssemblyViewModels.ISingletonTestViewModel, DependentAssemblyViewModels.SingletonTestViewModel>() },
            { ServiceDescriptor.KeyedSingleton<DependentAssemblyViewModels.ISingletonTestViewModel, DependentAssemblyViewModels.SingletonTestViewModel>(nameof(DependentAssemblyViewModels.SingletonTestViewModel)) }
        };
    }
}
