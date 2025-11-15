using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Provides a base class for MVVM-aware navigation link components in Blazor that can navigate to a specified URI and reflect active state.
/// </summary>
/// <remarks>
/// This class supports custom navigation logic, active state CSS, and integration with both Blazor's <see cref="NavigationManager"/> and MVVM navigation manager.
/// </remarks>
public abstract class MvvmNavLinkBase : ComponentBase, IDisposable
{
    private const string DefaultActiveClass = "active";

    private bool _isActive;
    private string? _hrefAbsolute;
    private string? _class;
    private string? _cssClass;
    private bool isDisposed;

    /// <summary>
    /// Gets or sets the MVVM navigation manager used by the NavLink for ViewModel-based navigation.
    /// </summary>
    [Inject]
    protected IMvvmNavigationManager MvvmNavigationManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Blazor navigation manager used by the NavLink for URI-based navigation.
    /// </summary>
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;

    /// <summary>
    /// Gets or sets the CSS class name applied to the NavLink when the current route matches the NavLink href.
    /// </summary>
    [Parameter]
    public string? ActiveClass { get; set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be added to the generated <c>a</c> element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the child content to be rendered inside the NavLink.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets a value representing the URL matching behavior for determining active state.
    /// </summary>
    [Parameter]
    public NavLinkMatch Match { get; set; }

    /// <summary>
    /// Gets or sets the relative URI or query string to append to the associated URI.
    /// </summary>
    [Parameter]
    public string? RelativeUri { get; set; }

    /// <summary>
    /// Releases resources used by the component.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Resolves the href for the NavLink. Must be implemented by derived classes.
    /// </summary>
    /// <returns>The resolved href string.</returns>
    protected abstract string ResolveHref();

    /// <summary>
    /// Called when the component is initialized. Subscribes to location change events.
    /// </summary>
    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    /// <summary>
    /// Called when component parameters are set. Updates href, active state, and CSS class.
    /// </summary>
    protected override void OnParametersSet()
    {
        _hrefAbsolute = BuildUri(ResolveHref(), RelativeUri);

        AdditionalAttributes?.Add("href", _hrefAbsolute ?? string.Empty);
        _isActive = ShouldMatch(NavigationManager.Uri);
        _class = null;

        if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out object? obj))
        {
            _class = Convert.ToString(obj, CultureInfo.InvariantCulture);
        }

        UpdateCssClass();
    }

    /// <summary>
    /// Removes the port from the given URI string.
    /// </summary>
    /// <param name="uri">The URI string.</param>
    /// <returns>The URI string without the port.</returns>
    private static string StripPort(string uri)
    {
        UriBuilder builder = new(uri);
        builder.Port = -1; // This will remove the port
        return builder.ToString();
    }

    /// <summary>
    /// Builds the render tree for the NavLink component.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "a");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "class", _cssClass);
        builder.AddContent(3, ChildContent);
        builder.CloseElement();
    }

    /// <summary>
    /// Releases resources used by the component and unsubscribes from events.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/>; false if called from a finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }

        isDisposed = true;
    }

    /// <summary>
    /// Builds the URI by appending the relative URI to the base URI.
    /// </summary>
    /// <param name="uri">The base URI.</param>
    /// <param name="relativeUri">The relative URI to append.</param>
    /// <returns>The combined URI string.</returns>
    private static string BuildUri(string uri, string? relativeUri)
    {
        if (string.IsNullOrWhiteSpace(relativeUri))
        {
            return uri;
        }

        UriBuilder builder = new(uri);

        if (relativeUri.StartsWith('?'))
        {
            builder.Query = relativeUri.TrimStart('?');
        }
        else if (relativeUri.Contains('?'))
        {
            string[] parts = relativeUri.Split('?');
            builder.Path = builder.Path.TrimEnd('/') + "/" + parts[0].TrimStart('/');
            builder.Query = parts[1];
        }
        else
        {
            builder.Path = builder.Path.TrimEnd('/') + "/" + relativeUri.TrimStart('/');
        }

        return builder.ToString();
    }

    /// <summary>
    /// Updates the CSS class based on the active state of the NavLink.
    /// </summary>
    private void UpdateCssClass()
        => _cssClass = _isActive ? CombineWithSpace(_class, ActiveClass ?? DefaultActiveClass) : _class;

    /// <summary>
    /// Handles location change events and updates the active state and CSS class if needed.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event arguments containing the new location.</param>
    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        bool shouldBeActiveNow = ShouldMatch(args.Location);

        if (shouldBeActiveNow != _isActive)
        {
            _isActive = shouldBeActiveNow;
            UpdateCssClass();
            StateHasChanged();
        }
    }

    /// <summary>
    /// Determines if the current URI should match the NavLink for active state.
    /// </summary>
    /// <param name="currentUriAbsolute">The current absolute URI.</param>
    /// <returns>True if the URI matches; otherwise, false.</returns>
    private bool ShouldMatch(string currentUriAbsolute)
    {
        if (_hrefAbsolute == null)
        {
            return false;
        }

        string hrefAbsolute = StripPort(_hrefAbsolute);
        string uriAbsolute = StripPort(currentUriAbsolute);
        if (EqualsHrefExactlyOrIfTrailingSlashAdded(uriAbsolute))
        {
            return true;
        }

        return Match == NavLinkMatch.Prefix
               && IsStrictlyPrefixWithSeparator(uriAbsolute, hrefAbsolute);
    }

    /// <summary>
    /// Checks if the current URI exactly matches the href or if a trailing slash is added.
    /// </summary>
    /// <param name="currentUriAbsolute">The current absolute URI.</param>
    /// <returns>True if the URI matches; otherwise, false.</returns>
    private bool EqualsHrefExactlyOrIfTrailingSlashAdded(string currentUriAbsolute)
    {
        Debug.Assert(_hrefAbsolute != null);

        string hrefAbsolute = StripPort(_hrefAbsolute);
        string uriAbsolute = StripPort(currentUriAbsolute);
        if (string.Equals(uriAbsolute, hrefAbsolute, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (uriAbsolute.Length == hrefAbsolute.Length - 1)
        {
            // Special case: highlight links to http://host/path/ even if you're
            // at http://host/path (with no trailing slash)
            // This is because the router accepts an absolute URI value of "same
            // as base URI but without trailing slash" as equivalent to "base URI",
            // which in turn is because it's common for servers to return the same page
            // for http://host/vdir as they do for host://host/vdir/ as it's no
            // good to display a blank page in that case.
            return _hrefAbsolute[^1] == '/' && hrefAbsolute.StartsWith(uriAbsolute, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    /// <summary>
    /// Combines two strings with a space separator.
    /// </summary>
    /// <param name="str1">The first string.</param>
    /// <param name="str2">The second string.</param>
    /// <returns>The combined string.</returns>
    private static string CombineWithSpace(string? str1, string str2)
        => str1 == null ? str2 : $"{str1} {str2}";

    /// <summary>
    /// Determines if the value is strictly a prefix with a separator for route matching.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="prefix">The prefix to check against.</param>
    /// <returns>True if the value is a prefix with a separator; otherwise, false.</returns>
    private static bool IsStrictlyPrefixWithSeparator(string value, string prefix)
    {
        int prefixLength = prefix.Length;
        if (value.Length > prefixLength)
        {
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && (
                    // Only match when there's a separator character either at the end of the
                    // prefix or right after it.
                    // Example: "/abc" is treated as a prefix of "/abc/def" but not "/abcdef"
                    // Example: "/abc/" is treated as a prefix of "/abc/def" but not "/abcdef"
                    prefixLength == 0
                    || !char.IsLetterOrDigit(prefix[prefixLength - 1])
                    || !char.IsLetterOrDigit(value[prefixLength])
                );
        }

        return false;
    }
}
