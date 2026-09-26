using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Creates transient ViewModels outside the dependency injection container so that the View which requested one owns it
/// and disposes it together with itself.
/// </summary>
/// <remarks>
/// <para>
/// The container keeps a reference to every disposable transient service it creates until the scope that created it is disposed.
/// On Blazor Server that scope is the circuit, and on Blazor WebAssembly it lasts as long as the application, so a transient
/// ViewModel resolved from it outlives its View and accumulates every time the View is recreated.
/// </para>
/// <para>
/// This factory replays the ViewModel's own <see cref="ServiceDescriptor"/> against the same <see cref="IServiceProvider"/>.
/// The ViewModel's constructor dependencies are therefore still resolved from, and shared with, that scope, while the ViewModel
/// itself is not tracked by it. Registrations that cannot be replayed faithfully are left to the container.
/// </para>
/// </remarks>
internal sealed class TransientViewModelFactory
{
    /// <summary>
    /// The latest ViewModel registration for each service type and key, built on first use.
    /// </summary>
    private readonly Lazy<Dictionary<(Type ServiceType, object? ServiceKey), ServiceDescriptor>> _descriptors;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransientViewModelFactory"/> class.
    /// </summary>
    /// <param name="services">
    /// The application's service collection. It is read on first use rather than now, so ViewModels registered after
    /// <see cref="ServicesExtension.AddMvvm"/> are included.
    /// </param>
    public TransientViewModelFactory(IServiceCollection services)
        => _descriptors = new(() => IndexViewModelDescriptors(services));

    /// <summary>
    /// Creates a new instance of a ViewModel registered with a <see cref="ServiceLifetime.Transient"/> lifetime, without the
    /// container tracking it.
    /// </summary>
    /// <param name="serviceType">The ViewModel service type the View requests.</param>
    /// <param name="serviceKey">The key the View requests the ViewModel with, or <see langword="null"/> for a non-keyed ViewModel.</param>
    /// <param name="serviceProvider">The service provider that resolves the ViewModel's constructor dependencies.</param>
    /// <param name="viewModel">When this method returns <see langword="true"/>, the created ViewModel; otherwise, <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the ViewModel was created and the caller owns it; <see langword="false"/> if the ViewModel is not
    /// transient, is not registered, or its registration cannot be replayed, in which case the caller should resolve it from the container.
    /// </returns>
    public bool TryCreate(Type serviceType, object? serviceKey, IServiceProvider serviceProvider, [NotNullWhen(true)] out object? viewModel)
    {
        viewModel = null;

        if (!_descriptors.Value.TryGetValue((serviceType, serviceKey), out var descriptor)
            || descriptor.Lifetime != ServiceLifetime.Transient)
        {
            return false;
        }

        if (descriptor.IsKeyedService)
        {
            viewModel = descriptor.KeyedImplementationFactory is not null
                ? descriptor.KeyedImplementationFactory(serviceProvider, serviceKey)
                : CreateFromType(descriptor.KeyedImplementationType, serviceProvider);
        }
        else
        {
            viewModel = descriptor.ImplementationFactory is not null
                ? descriptor.ImplementationFactory(serviceProvider)
                : CreateFromType(descriptor.ImplementationType, serviceProvider);
        }

        return viewModel is not null;
    }

    /// <summary>
    /// Constructs an implementation type with its dependencies resolved from <paramref name="serviceProvider"/>.
    /// </summary>
    /// <param name="implementationType">The type to construct.</param>
    /// <param name="serviceProvider">The service provider that resolves the constructor dependencies.</param>
    /// <returns>
    /// The new instance, or <see langword="null"/> when the type has no implementation type or a constructor receives its own
    /// service key through <see cref="ServiceKeyAttribute"/>, which only the container can supply.
    /// </returns>
    private static object? CreateFromType(Type? implementationType, IServiceProvider serviceProvider)
    {
        if (implementationType is null
            || implementationType.GetConstructors().Any(constructor => constructor.GetParameters().Any(parameter => parameter.IsDefined(typeof(ServiceKeyAttribute)))))
        {
            return null;
        }

        return ActivatorUtilities.CreateInstance(serviceProvider, implementationType);
    }

    /// <summary>
    /// Indexes the ViewModel registrations in <paramref name="services"/> by service type and key, keeping the last registration
    /// for each, which is the one the container resolves.
    /// </summary>
    /// <param name="services">The service collection to index.</param>
    /// <returns>The ViewModel registrations keyed by service type and service key.</returns>
    private static Dictionary<(Type ServiceType, object? ServiceKey), ServiceDescriptor> IndexViewModelDescriptors(IServiceCollection services)
    {
        Dictionary<(Type ServiceType, object? ServiceKey), ServiceDescriptor> descriptors = [];

        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType.IsAssignableTo(typeof(IViewModelBase)))
            {
                descriptors[(descriptor.ServiceType, descriptor.ServiceKey)] = descriptor;
            }
        }

        return descriptors;
    }
}
