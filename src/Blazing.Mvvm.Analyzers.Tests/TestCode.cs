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
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using CommunityToolkit.Mvvm.Messaging;

    public abstract class ViewModelBase : System.ComponentModel.INotifyPropertyChanged, IViewModelBase
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

        // Lifecycle methods
        public virtual void OnInitialized() { }
        public virtual Task OnInitializedAsync() => Task.CompletedTask;
        public virtual void OnParametersSet() { }
        public virtual Task OnParametersSetAsync() => Task.CompletedTask;
        public virtual void OnAfterRender(bool firstRender) { }
        public virtual Task OnAfterRenderAsync(bool firstRender) => Task.CompletedTask;
        public virtual bool ShouldRender() => true;
        public virtual void NotifyStateChanged() => OnPropertyChanged();
    }

    public interface IViewModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        void OnAfterRender(bool firstRender);
        Task OnAfterRenderAsync(bool firstRender);
        void OnInitialized();
        Task OnInitializedAsync();
        void OnParametersSet();
        Task OnParametersSetAsync();
        bool ShouldRender();
        void NotifyStateChanged();
    }

    public abstract class RecipientViewModelBase : ViewModelBase, IDisposable
    {
        protected IMessenger Messenger { get; } = WeakReferenceMessenger.Default;
        protected virtual void OnActivated() { }
        
        private bool IsDisposed;
        
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            
            if (disposing)
            {
                // Cleanup logic
            }
            IsDisposed = true;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
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

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class NotifyPropertyChangedForAttribute : Attribute
    {
        public NotifyPropertyChangedForAttribute(string propertyName) { }
    }
}

namespace Blazing.Mvvm.Components
{
    using Microsoft.AspNetCore.Components;
    using Blazing.Mvvm.ComponentModel;

    public abstract class MvvmComponentBase<TViewModel> : ComponentBase
        where TViewModel : ViewModelBase
    {
        public TViewModel ViewModel { get; set; }
    }

    public abstract class MvvmOwningComponentBase<TViewModel> : MvvmComponentBase<TViewModel>
        where TViewModel : ViewModelBase
    {
    }

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
