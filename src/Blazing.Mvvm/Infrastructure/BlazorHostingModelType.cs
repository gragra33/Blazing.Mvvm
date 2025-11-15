namespace Blazing.Mvvm;

/// <summary>
/// Specifies the supported ASP.NET Core Blazor hosting models for applications.
/// </summary>
public enum BlazorHostingModelType
{
    /// <summary>
    /// No hosting model is specified. Used as a default or placeholder value.
    /// </summary>
    NotSpecified,

    /// <summary>
    /// Blazor Server hosting model. Runs on the server and interacts with the client over SignalR.
    /// </summary>
    Server,

    /// <summary>
    /// Blazor WebAssembly hosting model. Runs entirely in the browser using WebAssembly.
    /// </summary>
    WebAssembly,

    /// <summary>
    /// Blazor Hybrid hosting model. Runs in a native desktop or mobile app using WebView.
    /// </summary>
    Hybrid,

    /// <summary>
    /// Blazor Hybrid with .NET MAUI. Integrates Blazor with .NET MAUI for cross-platform native apps.
    /// </summary>
    HybridMaui,

    /// <summary>
    /// Blazor Interactive WebApp hosting model. Supports interactive server-side rendering and navigation.
    /// </summary>
    WebApp
}
