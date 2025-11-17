using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// Represents a test interface for keyed navigation view models in infrastructure fakes.
/// Provides navigation state, query string, echo, and test parameters, along with commands for navigation actions.
/// </summary>
public interface ITestKeyedNavigationViewModel : IViewModelBase, IDisposable
{
    /// <summary>
    /// Gets or sets the query string value for navigation.
    /// </summary>
    string QueryString { get; set; }
    /// <summary>
    /// Gets or sets the test parameter value for navigation. Nullable.
    /// </summary>
    string? Test { get; set; }
    /// <summary>
    /// Gets or sets the echo parameter value for navigation. Nullable.
    /// </summary>
    string? Echo { get; set; }
    /// <summary>
    /// Gets the command for keyed test navigation without parameters.
    /// </summary>
    RelayCommand KeyedTestNavigateCommand { get; }
    /// <summary>
    /// Gets the command for keyed test navigation with parameters.
    /// </summary>
    RelayCommand<string> KeyedTestNavigateCommandWithParams { get; }
}
