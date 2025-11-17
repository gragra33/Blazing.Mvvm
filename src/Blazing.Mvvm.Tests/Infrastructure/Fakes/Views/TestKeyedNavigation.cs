using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes.Views;

/// <summary>
/// A test view for the <see cref="ITestKeyedNavigationViewModel"/> supporting keyed navigation scenarios.
/// Used in unit tests to verify view-model binding, routing, and navigation commands.
/// </summary>
/// <remarks>
/// This view is associated with the <c>TestKeyedNavigationViewModel</c> using the <see cref="ViewModelKeyAttribute"/> and is routed to "/keyedtest" and "/keyedtest/{echo}".
/// </remarks>
[ViewModelKey("TestKeyedNavigationViewModel")]
[Route("/keyedtest")]
[Route("/keyedtest/{echo}")]
public class TestKeyedNavigation : MvvmComponentBase<ITestKeyedNavigationViewModel>
{
    /// <summary>
    /// Gets or sets the echo parameter from the route.
    /// </summary>
    [Parameter]
    public string? Echo { get; set; }

    /// <summary>
    /// Builds the render tree for the test keyed navigation view.
    /// Renders navigation buttons and displays navigation state.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddContent(1, "Test Keyed Navigation");

        // Keyed test button
        builder.OpenElement(2, "button");
        builder.AddAttribute(3, "id", "keyedtest");
        builder.AddAttribute(4, "type", "button");
        builder.AddAttribute(5, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => ViewModel.KeyedTestNavigateCommand.Execute(null)));
        builder.AddContent(6, "Keyed Test");
        builder.CloseElement();

        // Keyed test relative path button
        builder.OpenElement(7, "button");
        builder.AddAttribute(8, "id", "keyedtest-relative-path");
        builder.AddAttribute(9, "type", "button");
        builder.AddAttribute(10, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => ViewModel.KeyedTestNavigateCommandWithParams.Execute("this is a MvvmKeyNavLink test")));
        builder.AddContent(11, "Keyed Test with Relative Path");
        builder.CloseElement();

        // Keyed test query string button
        builder.OpenElement(12, "button");
        builder.AddAttribute(13, "id", "keyedtest-query-string");
        builder.AddAttribute(14, "type", "button");
        builder.AddAttribute(15, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => ViewModel.KeyedTestNavigateCommandWithParams.Execute("?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test")));
        builder.AddContent(16, "Keyed Test with QueryString");
        builder.CloseElement();

        // Keyed test relative path + query string button
        builder.OpenElement(17, "button");
        builder.AddAttribute(18, "id", "keyedtest-relpath-qstring");
        builder.AddAttribute(19, "type", "button");
        builder.AddAttribute(20, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => ViewModel.KeyedTestNavigateCommandWithParams.Execute("this is a MvvmKeyNavLink test/?test=this%20is%20a%20MvvmKeyNavLink%20querystring%20test")));
        builder.AddContent(21, "Keyed Test with Relative Path + QueryString");
        builder.CloseElement();

        // Display relative path if present
        if (!string.IsNullOrWhiteSpace(ViewModel.Echo))
        {
            builder.OpenElement(22, "p");
            builder.AddAttribute(23, "aria-label", "relative path");
            builder.AddContent(24, $"Relative Path: {ViewModel.Echo}");
            builder.CloseElement();
        }

        // Display query string if present
        if (!string.IsNullOrWhiteSpace(ViewModel.QueryString))
        {
            builder.OpenElement(25, "p");
            builder.AddAttribute(26, "aria-label", "query string");
            builder.AddContent(27, $"QueryString: {ViewModel.QueryString}");
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}