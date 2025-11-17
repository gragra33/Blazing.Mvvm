using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Platform;
using DynamicData;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Avalonia.Interactivity;

namespace Baksteen.Avalonia.Blazor;

/// <summary>
/// Provides a Blazor WebView control for Avalonia, enabling Blazor content to be hosted in Avalonia applications.
/// </summary>
public class BlazorWebView : NativeControlHost
{
    /// <summary>
    /// Represents a destroyable handle for the BlazorWebView native control.
    /// </summary>
    private class DestroyableBlazorWebViewHandle : INativeControlHostDestroyableControlHandle
    {
        private readonly BlazorWebView _parent;

        /// <summary>
        /// Gets the native window handle.
        /// </summary>
        public nint Handle { get; private set; }

        /// <summary>
        /// Gets the descriptor for the handle type.
        /// </summary>
        public string HandleDescriptor => "HWND";

        /// <summary>
        /// Initializes a new instance of the <see cref="DestroyableBlazorWebViewHandle"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="BlazorWebView"/> instance.</param>
        public DestroyableBlazorWebViewHandle(BlazorWebView parent)
        {
            _parent = parent;
            Handle = _parent._blazorWebView!.Handle;
        }

        /// <summary>
        /// Destroys the native control handle and disposes the BlazorWebView.
        /// </summary>
        public void Destroy()
        {
            _parent._blazorWebView?.Dispose();
            _parent._blazorWebView = null;
        }
    }

    private Uri? _source;
    private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView? _blazorWebView;
    private double _zoomFactor = 1.0;
    private string? _hostPage;
    private IServiceProvider _serviceProvider = default!;
    private RootComponentsCollection _rootComponents = new();
    private DestroyableBlazorWebViewHandle? _destroyableBlazorWebViewHandle;

    /// <summary>
    /// The <see cref="AvaloniaProperty" /> which backs the <see cref="ZoomFactor" /> property.
    /// </summary>
    public static readonly DirectProperty<BlazorWebView, double> ZoomFactorProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, double>(
            nameof(ZoomFactor),
            x => x.ZoomFactor,
            (x, y) => x.ZoomFactor = y);

    /// <summary>
    /// The <see cref="AvaloniaProperty" /> which backs the <see cref="Services" /> property.
    /// </summary>
    public static readonly DirectProperty<BlazorWebView, IServiceProvider> ServicesProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, IServiceProvider>(
            nameof(Services),
            x => x.Services,
            (x, y) => x.Services = y);

    /// <summary>
    /// The <see cref="AvaloniaProperty" /> which backs the <see cref="RootComponents" /> property.
    /// </summary>
    public static readonly DirectProperty<BlazorWebView, RootComponentsCollection> RootComponentsProperty
        = AvaloniaProperty.RegisterDirect<BlazorWebView, RootComponentsCollection>(
            nameof(RootComponents),
            x => x.RootComponents,
            (x, y) => x.RootComponents = y);

    /// <summary>
    /// Gets or sets the path to the host page for the Blazor WebView.
    /// </summary>
    public string? HostPage
    {
        get
        {
            if(_blazorWebView != null)
                _hostPage = _blazorWebView.HostPage;
            return _hostPage;
        }

        set
        {
            if(_hostPage != value)
            {
                _hostPage = value;
                if(_blazorWebView != null)
                    _blazorWebView.HostPage = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the source URI for the Blazor WebView.
    /// </summary>
    public Uri? Source
    {
        get
        {
            if(_blazorWebView != null)
                _source = _blazorWebView.WebView.Source;
            return _source;
        }

        set
        {
            if(_source != value)
            {
                _source = value;
                if(_blazorWebView != null)
                    _blazorWebView.WebView.Source = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the zoom factor for the Blazor WebView.
    /// </summary>
    public double ZoomFactor
    {
        get
        {
            if(_blazorWebView != null)
                _zoomFactor = _blazorWebView.WebView.ZoomFactor;
            return _zoomFactor;
        }

        set
        {
            if(Math.Abs(_zoomFactor - value) > 0.001D)
            {
                _zoomFactor = value;
                if(_blazorWebView != null) _blazorWebView.WebView.ZoomFactor = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the service provider for the Blazor WebView.
    /// </summary>
    public IServiceProvider Services
    {
        get => _serviceProvider;
        set
        {
            _serviceProvider = value;
            if(_blazorWebView != null) _blazorWebView.Services = _serviceProvider;
        }
    }

    /// <summary>
    /// Gets or sets the root components collection for the Blazor WebView.
    /// </summary>
    public RootComponentsCollection RootComponents
    {
        get => _rootComponents;
        set => _rootComponents = value;
    }

    /// <summary>
    /// Creates the native control core for the Blazor WebView.
    /// </summary>
    /// <param name="parent">The parent platform handle.</param>
    /// <returns>The platform handle for the native control.</returns>
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if(OperatingSystem.IsWindows())
        {
            _blazorWebView = new()
            {
                HostPage = _hostPage,
                Services = _serviceProvider,
            };

            _blazorWebView.WebView.ZoomFactor = Math.Clamp(_zoomFactor, 0.1, 4.0);
            _blazorWebView.RootComponents.AddRange(_rootComponents);
            _destroyableBlazorWebViewHandle=new DestroyableBlazorWebViewHandle(this);

            return _destroyableBlazorWebViewHandle;
        }

        return base.CreateNativeControlCore(parent);
    }

    /// <summary>
    /// Called when the control is unloaded. Destroys the native BlazorWebView control.
    /// </summary>
    /// <param name="e">The routed event arguments.</param>
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _destroyableBlazorWebViewHandle?.Destroy();
        base.OnUnloaded(e);
    }
}
