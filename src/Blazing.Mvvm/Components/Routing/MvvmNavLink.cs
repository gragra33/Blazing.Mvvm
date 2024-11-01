using System.Globalization;
using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// A component that renders an anchor tag, automatically toggling its 'active'
/// class based on whether its 'href' matches the current URI. Navigation is based on ViewModel (class/interface).
/// </summary>
/// <typeparam name="TViewModel">The type of the ViewModel.</typeparam>
public sealed class MvvmNavLink<TViewModel> : MvvmKeyNavLink where TViewModel : IViewModelBase
{
    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        _hrefAbsolute = BuildUri(!string.IsNullOrEmpty(NavigationKey)
            ? NavigationManager.ToAbsoluteUri(MvvmNavigationManager.GetUri(NavigationKey)).AbsoluteUri
            : NavigationManager.ToAbsoluteUri(MvvmNavigationManager.GetUri<TViewModel>()).AbsoluteUri, RelativeUri);

        AdditionalAttributes?.Add("href", _hrefAbsolute);

        _isActive = ShouldMatch(NavigationManager.Uri);

        _class = null;

        if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("class", out object? obj))
        {
            _class = Convert.ToString(obj, CultureInfo.InvariantCulture);
        }

        UpdateCssClass();
    }
}