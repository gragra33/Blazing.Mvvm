namespace Blazing.Mvvm.Analyzers.Tests;

/// <summary>
/// Common test code snippets and type stubs for analyzer tests.
/// These stub types allow test code to compile without requiring the actual Blazing.Mvvm assembly.
/// </summary>
public static class TestCode
{
    /// <summary>
    /// Stub types that represent Blazing.Mvvm types for test compilation.
    /// </summary>
    public const string BlazingMvvmStubs = @"
namespace Blazing.Mvvm.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using CommunityToolkit.Mvvm.Messaging;

    public interface IViewModelBase : INotifyPropertyChanged, IDisposable
    {
        void OnInitialized();
        Task OnInitializedAsync();
        void OnParametersSet();
        Task OnParametersSetAsync();
        void OnAfterRender(bool firstRender);
        Task OnAfterRenderAsync(bool firstRender);
        bool ShouldRender();
    }

    public abstract class ViewModelBase : IViewModelBase
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Lifecycle methods mirror the public virtual members on the production base classes.
        public virtual void OnInitialized() { }
        public virtual Task OnInitializedAsync() => Task.CompletedTask;
        public virtual void OnParametersSet() { }
        public virtual Task OnParametersSetAsync() => Task.CompletedTask;
        public virtual void OnAfterRender(bool firstRender) { }
        public virtual Task OnAfterRenderAsync(bool firstRender) => Task.CompletedTask;
        public virtual bool ShouldRender() => true;

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public abstract class RecipientViewModelBase : ViewModelBase
    {
        protected IMessenger Messenger { get; } = WeakReferenceMessenger.Default;
        protected virtual void OnActivated() { }
    }

    public abstract class ValidatorViewModelBase : ViewModelBase { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewModelDefinitionAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; set; }
        public Type ViewType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ViewParameterAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewModelKeyAttribute : Attribute
    {
        public ViewModelKeyAttribute(string key) { }
    }
}

namespace CommunityToolkit.Mvvm.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ObservablePropertyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class NotifyPropertyChangedForAttribute : Attribute
    {
        public NotifyPropertyChangedForAttribute(string propertyName) { }
    }
}

namespace Microsoft.AspNetCore.Http
{
    public interface IHttpContextAccessor { }
}

namespace Blazing.Mvvm.Components
{
    using Microsoft.AspNetCore.Components;
    using Blazing.Mvvm.ComponentModel;

    public abstract class MvvmComponentBase<TViewModel> : ComponentBase, System.IDisposable, System.IAsyncDisposable
        where TViewModel : ViewModelBase
    {
        public TViewModel ViewModel { get; set; }

        public void Dispose()
        {
        }

        System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync() => default;

        protected virtual void Dispose(bool disposing)
        {
        }
    }

    public abstract class MvvmOwningComponentBase<TViewModel> : MvvmComponentBase<TViewModel>
        where TViewModel : ViewModelBase
    {
    }

    public abstract class MvvmLayoutComponentBase<TViewModel> : LayoutComponentBase, System.IDisposable, System.IAsyncDisposable
        where TViewModel : ViewModelBase
    {
        public TViewModel ViewModel { get; set; }

        public void Dispose()
        {
        }

        System.Threading.Tasks.ValueTask System.IAsyncDisposable.DisposeAsync() => default;

        protected virtual void Dispose(bool disposing)
        {
        }
    }

    public class MvvmNavLink<TViewModel> where TViewModel : ViewModelBase
    {
    }
}

namespace Blazing.Mvvm.Components.Routing
{
    using Blazing.Mvvm.ComponentModel;

    public class MvvmNavLink<TViewModel> where TViewModel : ViewModelBase
    {
    }
}

namespace Blazing.Mvvm.Services
{
    public interface INavigationService
    {
        void NavigateTo<TViewModel>();
        void NavigateTo(string key);
    }
}
";
}
