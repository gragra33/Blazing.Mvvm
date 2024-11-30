using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.UnitTests;

internal sealed class ServiceDescriptorComparer : IEqualityComparer<ServiceDescriptor>
{
    private static readonly Lazy<ServiceDescriptorComparer> Lazy
        = new(() => new ServiceDescriptorComparer());

    public static ServiceDescriptorComparer Comparer
        => Lazy.Value;

    private ServiceDescriptorComparer()
    {
    }

    public bool Equals(ServiceDescriptor? x, ServiceDescriptor? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (x.IsKeyedService ^ y.IsKeyedService)
        {
            return false;
        }

        return x.IsKeyedService
            ? x.ServiceType == y.ServiceType && x.KeyedImplementationType == y.KeyedImplementationType && x.Lifetime == y.Lifetime
            : x.ServiceType == y.ServiceType && x.ImplementationType == y.ImplementationType && x.Lifetime == y.Lifetime && x.ServiceKey == y.ServiceKey;
    }

    public int GetHashCode(ServiceDescriptor serviceDescriptor)
    {
        return serviceDescriptor.IsKeyedService
            ? HashCode.Combine(serviceDescriptor.ServiceType, serviceDescriptor.KeyedImplementationType, serviceDescriptor.Lifetime)
            : HashCode.Combine(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, serviceDescriptor.Lifetime, serviceDescriptor.ServiceKey);
    }
}
