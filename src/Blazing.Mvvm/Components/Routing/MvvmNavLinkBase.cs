using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// The base class for NavLink components that can be used to navigate to a specified URI.
/// </summary>
public abstract class MvvmNavLinkBase : ComponentBase, IDisposable
{
    private const string DefaultActiveClass = "active";

    private bool _isActive;
    private string? _hrefAbsolute;
    private string? _class;
    private string? _cssClass;
    private bool isDisposed;

    /// <summary>
    /// The MVVM navigation manager used by the NavLink.
    /// </summary>
    [Inject]
    protected IMvvmNavigationManager MvvmNavigationManager { get; set; } = default!;

    /// <summary>
    /// The navigation manager used by the NavLink.
    /// </summary>
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets the CSS class name applied to the NavLink when the
    /// current route matches the NavLink href.
    /// </summary>
    [Parameter]
    public string? ActiveClass { get; set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be added to the generated
    /// <c>a</c> element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets or sets the child content of the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets a value representing the URL matching behavior.
    /// </summary>
    [Parameter]
    public NavLinkMatch Match { get; set; }

    /// <summary>
    /// Relative URI or QueryString appended to the associate URI.
    /// </summary>
    [Parameter]
    public string? RelativeUri { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets the href for the NavLink.
    /// </summary>
    /// <returns>The href.</returns>
    protected abstract string ResolveHref();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        // We'll consider re-rendering on each location change
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "a");

        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "class", _cssClass);
        builder.AddContent(3, ChildContent);

        builder.CloseElement();
    }

    /// <summary>
    /// Disposes the component and releases unmanaged resources.
    /// </summary>
    /// <param name="disposing">A boolean value indicating whether the method was called from the dispose method or from a finalizer.</param>
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
    /// <returns>The combined URI.</returns>
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
    /// Updates the CSS class based on the active state.
    /// </summary>
    private void UpdateCssClass()
        => _cssClass = _isActive ? CombineWithSpace(_class, ActiveClass ?? DefaultActiveClass) : _class;

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        // We could just re-render always, but for this component we know the
        // only relevant state change is to the _isActive property.
        bool shouldBeActiveNow = ShouldMatch(args.Location);

        if (shouldBeActiveNow != _isActive)
        {
            _isActive = shouldBeActiveNow;
            UpdateCssClass();
            StateHasChanged();
        }
    }

    /// <summary>
    /// Determines if the current URI should match the NavLink.
    /// </summary>
    /// <param name="currentUriAbsolute">The current absolute URI.</param>
    /// <returns>True if the URI matches; otherwise, false.</returns>
    private bool ShouldMatch(string currentUriAbsolute)
    {
        if (_hrefAbsolute == null)
        {
            return false;
        }

        if (EqualsHrefExactlyOrIfTrailingSlashAdded(currentUriAbsolute))
        {
            return true;
        }

        return Match == NavLinkMatch.Prefix
               && IsStrictlyPrefixWithSeparator(currentUriAbsolute, _hrefAbsolute);
    }

    /// <summary>
    /// Checks if the current URI exactly matches the href or if a trailing slash is added.
    /// </summary>
    /// <param name="currentUriAbsolute">The current absolute URI.</param>
    /// <returns>True if the URI matches; otherwise, false.</returns>
    private bool EqualsHrefExactlyOrIfTrailingSlashAdded(string currentUriAbsolute)
    {
        Debug.Assert(_hrefAbsolute != null);

        if (string.Equals(currentUriAbsolute, _hrefAbsolute, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (currentUriAbsolute.Length == _hrefAbsolute.Length - 1)
        {
            // Special case: highlight links to http://host/path/ even if you're
            // at http://host/path (with no trailing slash)
            //
            // This is because the router accepts an absolute URI value of "same
            // as base URI but without trailing slash" as equivalent to "base URI",
            // which in turn is because it's common for servers to return the same page
            // for http://host/vdir as they do for host://host/vdir/ as it's no
            // good to display a blank page in that case.
            if (_hrefAbsolute[^1] == '/'
                && _hrefAbsolute.StartsWith(currentUriAbsolute, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
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
    /// Determines if the value is strictly a prefix with a separator.
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
