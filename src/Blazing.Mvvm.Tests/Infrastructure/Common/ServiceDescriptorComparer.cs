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

        if (x.IsKeyedService)
        {
            return CheckEquality(x.ServiceKey, y.ServiceKey)
                && CheckEquality(x.ServiceType, y.ServiceType)
                && CheckEquality(x.KeyedImplementationType, y.KeyedImplementationType)
                && CheckEquality(x.Lifetime, y.Lifetime);
        }

        return CheckEquality(x.ServiceType, y.ServiceType)
            && CheckEquality(x.ImplementationType, y.ImplementationType)
            && CheckEquality(x.Lifetime, y.Lifetime);

        static bool CheckEquality<T>(T first, T second)
            => EqualityComparer<T>.Default.Equals(first, second);
    }

    public int GetHashCode(ServiceDescriptor serviceDescriptor)
    {
        return serviceDescriptor.IsKeyedService
            ? HashCode.Combine(serviceDescriptor.ServiceKey, serviceDescriptor.ServiceType, serviceDescriptor.KeyedImplementationType, serviceDescriptor.Lifetime)
            : HashCode.Combine(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, serviceDescriptor.Lifetime);
    }
}
