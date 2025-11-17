using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Provides equality comparison for <see cref="ServiceDescriptor"/> instances, including support for keyed services.
/// </summary>
internal sealed class ServiceDescriptorComparer : IEqualityComparer<ServiceDescriptor>
{
    private static readonly Lazy<ServiceDescriptorComparer> Lazy
        = new(() => new ServiceDescriptorComparer());

    /// <summary>
    /// Gets the singleton instance of <see cref="ServiceDescriptorComparer"/>.
    /// </summary>
    public static ServiceDescriptorComparer Comparer
        => Lazy.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDescriptorComparer"/> class.
    /// </summary>
    private ServiceDescriptorComparer()
    {
    }

    /// <summary>
    /// Determines whether two <see cref="ServiceDescriptor"/> instances are equal.
    /// </summary>
    /// <param name="x">The first <see cref="ServiceDescriptor"/> to compare.</param>
    /// <param name="y">The second <see cref="ServiceDescriptor"/> to compare.</param>
    /// <returns><c>true</c> if the specified objects are equal; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Returns a hash code for the specified <see cref="ServiceDescriptor"/>.
    /// </summary>
    /// <param name="serviceDescriptor">The <see cref="ServiceDescriptor"/> for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    public int GetHashCode(ServiceDescriptor serviceDescriptor)
    {
        return serviceDescriptor.IsKeyedService
            ? HashCode.Combine(serviceDescriptor.ServiceKey, serviceDescriptor.ServiceType, serviceDescriptor.KeyedImplementationType, serviceDescriptor.Lifetime)
            : HashCode.Combine(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, serviceDescriptor.Lifetime);
    }
}
