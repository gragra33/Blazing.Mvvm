using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ParameterResolution.Sample.Wasm.ViewModels;

/// <summary>
/// ViewModel that demonstrates the Parameter Resolution feature of Blazing.Mvvm.
/// </summary>
/// <remarks>
/// This ViewModel showcases various parameter mapping strategies including property name mapping,
/// custom name mapping with <see cref="ViewParameterAttribute"/>, and integration with
/// <see cref="ObservablePropertyAttribute"/> and <see cref="RelayCommandAttribute"/> from CommunityToolkit.Mvvm.
/// Parameters are resolved from query string values defined in the associated View component.
/// </remarks>
public sealed partial class ParameterDemoViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the title parameter value resolved from the View.
    /// </summary>
    /// <value>
    /// The title string that was passed via the "Title" query parameter.
    /// </value>
    /// <remarks>
    /// This property demonstrates direct property name mapping where the <see cref="ViewParameterAttribute"/>
    /// automatically maps to a View parameter with the same name.
    /// </remarks>
    [ObservableProperty]
    [property: ViewParameter]
    private string _title = default!;

    /// <summary>
    /// Gets or sets the counter value resolved from the "Count" query parameter.
    /// </summary>
    /// <value>
    /// An integer value representing the count passed from the View's "Count" parameter.
    /// </value>
    /// <remarks>
    /// This property demonstrates custom name mapping where <see cref="ViewParameterAttribute"/>
    /// maps the View's "Count" parameter to the ViewModel's "Counter" property.
    /// </remarks>
    [ObservableProperty]
    [property: ViewParameter("Count")]
    private int _counter;

    /// <summary>
    /// Gets or sets the optional content parameter value resolved from the View.
    /// </summary>
    /// <value>
    /// The content string that was passed via the "Content" query parameter, or <c>null</c> if not provided.
    /// </value>
    /// <remarks>
    /// This property demonstrates nullable parameter support, where the parameter may or may not be provided by the View.
    /// </remarks>
    [ViewParameter]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the formatted display message showing all resolved parameter values.
    /// </summary>
    /// <value>
    /// A formatted string containing the Title, Counter, and Content values.
    /// </value>
    [ObservableProperty]
    private string _displayMessage = string.Empty;

    /// <summary>
    /// Called when component parameters are set.
    /// </summary>
    /// <remarks>
    /// This lifecycle method is invoked after all <see cref="ViewParameterAttribute"/> properties
    /// have been resolved from the View's parameters. It updates the display message to reflect
    /// the current parameter values.
    /// </remarks>
    public override void OnParametersSet()
    {
        UpdateDisplayMessage();
    }

    /// <summary>
    /// Increments the counter value by one and updates the display message.
    /// </summary>
    /// <remarks>
    /// This command demonstrates how RelayCommand integrates with the MVVM pattern,
    /// allowing the View to invoke ViewModel logic while maintaining separation of concerns.
    /// The generated command property is <c>IncrementCounterCommand</c>.
    /// </remarks>
    [RelayCommand]
    private void IncrementCounter()
    {
        Counter++;
        UpdateDisplayMessage();
    }

    /// <summary>
    /// Decrements the counter value by one and updates the display message.
    /// </summary>
    /// <remarks>
    /// This command demonstrates how RelayCommand integrates with the MVVM pattern,
    /// allowing the View to invoke ViewModel logic while maintaining separation of concerns.
    /// The generated command property is <c>DecrementCounterCommand</c>.
    /// </remarks>
    [RelayCommand]
    private void DecrementCounter()
    {
        Counter--;
        UpdateDisplayMessage();
    }

    /// <summary>
    /// Updates the display message with the current parameter values.
    /// </summary>
    /// <remarks>
    /// This method formats and sets the <see cref="DisplayMessage"/> property to show
    /// the Title, Counter, and Content values. It's called after parameters are set
    /// and whenever the counter value changes.
    /// </remarks>
    private void UpdateDisplayMessage()
    {
        DisplayMessage = $"Received Parameters:\n" +
                        $"Title: {Title}\n" +
                        $"Counter (via Count param): {Counter}\n" +
                        $"Content: {Content ?? "(null)"}";
    }
}
