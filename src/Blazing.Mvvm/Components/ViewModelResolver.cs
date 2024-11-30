using System.Collections.Concurrent;
using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Components;

internal static class ViewModelResolver
{
    private static readonly ConcurrentDictionary<Type, ViewModelKeyAttribute?> _vmKeyAttributes = [];

    public static TViewModel Resolve<TViewModel>(IView<TViewModel> view, IServiceProvider serviceProvider)
        where TViewModel : IViewModelBase
    {
        var viewType = view.GetType();
        var vmKey = _vmKeyAttributes.GetOrAdd(viewType, key => key.GetCustomAttribute<ViewModelKeyAttribute>());

        if (vmKey is null)
        {
            return serviceProvider.GetRequiredService<TViewModel>();
        }

        return serviceProvider.GetRequiredKeyedService<TViewModel>(vmKey.Key);
    }
}
