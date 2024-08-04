using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Infrastructure;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.UnitTests;

public class ServicesExtensionTests
{
    [Fact]
    public void GivenAddMvvm_WhenServicesAdded_ThenShouldContainRequiredServices()
    {
        // Arrange
        var mvvmNavigationServiceDescriptor = ServiceDescriptor.Singleton<IMvvmNavigationManager, MvvmNavigationManager>();
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm();

        // Assert
        sut.Contains(mvvmNavigationServiceDescriptor, ServiceDescriptorComparer.Comparer).Should().BeTrue();
    }

    [Theory]
    [InlineData(BlazorHostingModelType.NotSpecified, ServiceLifetime.Singleton)]
    [InlineData(BlazorHostingModelType.WebAssembly, ServiceLifetime.Singleton)]
    [InlineData(BlazorHostingModelType.Hybrid, ServiceLifetime.Singleton)]
    [InlineData(BlazorHostingModelType.WebApp, ServiceLifetime.Scoped)]
    [InlineData(BlazorHostingModelType.Server, ServiceLifetime.Scoped)]
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

    [Fact]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromCallingAssembly_ThenShouldContainViewModels()
    {
        // Arrange
        const int expectedViewModelCount = 11;
        const int expectedTransientViewModelCount = 4;
        const int expectedScopedViewModelCount = 3;
        const int expectedSingletonViewModelCount = 4;

        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm();

        // Assert
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase))).Should().Be(expectedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Transient).Should().Be(expectedTransientViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Scoped).Should().Be(expectedScopedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Singleton).Should().Be(expectedSingletonViewModelCount);
    }

    [Fact]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromAssemblyContainingGenericType_ThenShouldContainViewModels()
    {
        // Arrange
        const int expectedViewModelCount = 11;
        const int expectedTransientViewModelCount = 4;
        const int expectedScopedViewModelCount = 3;
        const int expectedSingletonViewModelCount = 4;

        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblyContaining<ServicesExtensionTests>());

        // Assert
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase))).Should().Be(expectedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Transient).Should().Be(expectedTransientViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Scoped).Should().Be(expectedScopedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Singleton).Should().Be(expectedSingletonViewModelCount);
    }

    [Fact]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromAssemblyContainingType_ThenShouldContainViewModels()
    {
        // Arrange
        const int expectedViewModelCount = 11;
        const int expectedTransientViewModelCount = 4;
        const int expectedScopedViewModelCount = 3;
        const int expectedSingletonViewModelCount = 4;

        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblyContaining(typeof(ServicesExtensionTests)));

        // Assert
        using var _ = new AssertionScope();
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase))).Should().Be(expectedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Transient).Should().Be(expectedTransientViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Scoped).Should().Be(expectedScopedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Singleton).Should().Be(expectedSingletonViewModelCount);
    }

    [Fact]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromSpecifiedAssembly_ShouldContainViewModels()
    {
        // Arrange
        const int expectedViewModelCount = 11;
        const int expectedTransientViewModelCount = 4;
        const int expectedScopedViewModelCount = 3;
        const int expectedSingletonViewModelCount = 4;

        var assembly = typeof(ServicesExtensionTests).Assembly;
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssembly(assembly));

        // Assert
        using var _ = new AssertionScope();
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase))).Should().Be(expectedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Transient).Should().Be(expectedTransientViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Scoped).Should().Be(expectedScopedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Singleton).Should().Be(expectedSingletonViewModelCount);
    }

    [Fact]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromSpecifiedAssemblies_ThenShouldContainViewModels()
    {
        // Arrange
        const int expectedViewModelCount = 11;
        const int expectedTransientViewModelCount = 4;
        const int expectedScopedViewModelCount = 3;
        const int expectedSingletonViewModelCount = 4;

        var assemblies = new[] { typeof(ServicesExtensionTests).Assembly };
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblies(assemblies));

        // Assert
        using var _ = new AssertionScope();
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase))).Should().Be(expectedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Transient).Should().Be(expectedTransientViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Scoped).Should().Be(expectedScopedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Singleton).Should().Be(expectedSingletonViewModelCount);
    }

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

    [Fact]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromDependentAssemblyContainingType_ThenShouldContainViewModels()
    {
        // Arrange
        const int expectedViewModelCount = 8;
        const int expectedTransientViewModelCount = 6;
        const int expectedScopedViewModelCount = 1;
        const int expectedSingletonViewModelCount = 1;

        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblyContaining<Sample.WebApp.Client._Imports>());

        // Assert
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase))).Should().Be(expectedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Transient).Should().Be(expectedTransientViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Scoped).Should().Be(expectedScopedViewModelCount);
        sut.Count(x => x.ServiceType.IsAssignableTo(typeof(IViewModelBase)) && x.Lifetime is ServiceLifetime.Singleton).Should().Be(expectedSingletonViewModelCount);
    }

    [Theory]
    [MemberData(nameof(ServicesExtensionTestData.ViewModelsInDependentAssembly), MemberType = typeof(ServicesExtensionTestData))]
    public void GivenAddMvvm_WhenViewModelsRegisteredFromDependentAssemblyContainingType_ThenShouldContainTransientViewModels(ServiceDescriptor vmServiceDescriptor)
    {
        // Arrange
        var sut = new ServiceCollection();

        // Act
        sut.AddMvvm(c => c.RegisterViewModelsFromAssemblyContaining<Sample.WebApp.Client._Imports>());

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
            { ServiceDescriptor.KeyedTransient<TransientKeyedTestViewModel, TransientKeyedTestViewModel>("Transient") },

            { ServiceDescriptor.Scoped<ScopedTestViewModel, ScopedTestViewModel>() },
            { ServiceDescriptor.Scoped<IScopedTestViewModel, ScopedTestViewModel>() },
            { ServiceDescriptor.KeyedScoped<ScopedKeyedTestViewModel, ScopedKeyedTestViewModel>("Scoped") },

            { ServiceDescriptor.Singleton<SingletonTestViewModel, SingletonTestViewModel>() },
            { ServiceDescriptor.Singleton<ISingletonTestViewModel, SingletonTestViewModel>() },
            { ServiceDescriptor.KeyedSingleton<ISingletonTestViewModel, SingletonTestViewModel>("ISingleton") },
            { ServiceDescriptor.KeyedSingleton<SingletonKeyedTestViewModel, SingletonKeyedTestViewModel>("Singleton") }
        };

        public static TheoryData<ServiceDescriptor> ViewModelsInDependentAssembly = new()
        {
            { ServiceDescriptor.Transient<Sample.WebApp.Client.ViewModels.EditContactViewModel, Sample.WebApp.Client.ViewModels.EditContactViewModel>() },
            { ServiceDescriptor.Transient<Sample.WebApp.Client.ViewModels.HexEntryViewModel, Sample.WebApp.Client.ViewModels.HexEntryViewModel>() },
            { ServiceDescriptor.KeyedTransient<Sample.WebApp.Client.ViewModels.HexTranslateViewModel, Sample.WebApp.Client.ViewModels.HexTranslateViewModel>(nameof(Sample.WebApp.Client.ViewModels.HexTranslateViewModel)) },
            { ServiceDescriptor.Transient<Sample.WebApp.Client.ViewModels.ITestNavigationViewModel, Sample.WebApp.Client.ViewModels.TestNavigationViewModel>() },
            { ServiceDescriptor.Transient<Sample.WebApp.Client.ViewModels.MainLayoutViewModel, Sample.WebApp.Client.ViewModels.MainLayoutViewModel>() },
            { ServiceDescriptor.Transient<Sample.WebApp.Client.ViewModels.TextEntryViewModel, Sample.WebApp.Client.ViewModels.TextEntryViewModel>() },

            { ServiceDescriptor.Scoped<Sample.WebApp.Client.ViewModels.FetchDataViewModel, Sample.WebApp.Client.ViewModels.FetchDataViewModel>() },

            { ServiceDescriptor.Singleton<Sample.WebApp.Client.ViewModels.CounterViewModel, Sample.WebApp.Client.ViewModels.CounterViewModel>() }
        };
    }
}
