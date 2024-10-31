using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Common;

public class MockServiceProvider : IServiceProvider
{
    private readonly AutoMocker _autoMocker;

    public MockServiceProvider(AutoMocker autoMocker)
        => _autoMocker = autoMocker;

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IComponentActivator))
        {
            return null;
        }

        // Create real instances of ViewModels and not mock them.
        if (!serviceType.IsAbstract && serviceType.IsAssignableTo(typeof(IViewModelBase)) && !_autoMocker.ResolvedObjects.ContainsKey(serviceType))
        {
            _autoMocker.With(serviceType);
        }

        return _autoMocker.Get(serviceType);
    }
}
