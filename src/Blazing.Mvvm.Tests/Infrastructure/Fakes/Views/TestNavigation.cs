using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes.Views;

[Route("/test")]
[Route("/test/{echo}")]
public class TestNavigation : MvvmComponentBase<ITestNavigationViewModel>
{
    [Parameter]
    public string? Echo { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddContent(1, "Test Navigation");

        // Hex translate button
        builder.OpenElement(2, "button");
        builder.AddAttribute(3, "id", "hex-translate");
        builder.AddAttribute(4, "type", "button");
        builder.AddAttribute(5, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => ViewModel.HexTranslateNavigateCommand.Execute(null)));
        builder.AddContent(6, "Hex Translator");
        builder.CloseElement();

        // Test button
        builder.OpenElement(7, "button");
        builder.AddAttribute(8, "id", "test");
        builder.AddAttribute(9, "type", "button");
        builder.AddAttribute(10, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => ViewModel.TestNavigateCommand.Execute(null)));
        builder.AddContent(11, "Test");
        builder.CloseElement();

        // Test relative path button
        builder.OpenElement(12, "button");
        builder.AddAttribute(13, "id", "test-relative-path");
        builder.AddAttribute(14, "type", "button");
        builder.AddAttribute(15, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => ViewModel.TestNavigateCommand.Execute("this is a MvvmNavLink test")));
        builder.AddContent(16, "Test with Relative Path");
        builder.CloseElement();

        // Test query string button
        builder.OpenElement(17, "button");
        builder.AddAttribute(18, "id", "test-query-string");
        builder.AddAttribute(19, "type", "button");
        builder.AddAttribute(20, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => ViewModel.TestNavigateCommand.Execute("?test=this%20is%20a%20MvvmNavLink%20querystring%20test")));
        builder.AddContent(21, "Test with QueryString");
        builder.CloseElement();

        // Test relative path + query string button
        builder.OpenElement(22, "button");
        builder.AddAttribute(23, "id", "test-relpath-qstring");
        builder.AddAttribute(24, "type", "button");
        builder.AddAttribute(25, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => ViewModel.TestNavigateCommand.Execute("this is a MvvmNavLink test/?test=this%20is%20a%20MvvmNavLink%20querystring%20test")));
        builder.AddContent(26, "Test with Relative Path + QueryString");
        builder.CloseElement();

        // Display relative path if present
        if (!string.IsNullOrWhiteSpace(ViewModel.Echo))
        {
            builder.OpenElement(27, "p");
            builder.AddAttribute(28, "aria-label", "relative path");
            builder.AddContent(29, $"Relative Path: {ViewModel.Echo}");
            builder.CloseElement();
        }

        // Display query string if present
        if (!string.IsNullOrWhiteSpace(ViewModel.QueryString))
        {
            builder.OpenElement(30, "p");
            builder.AddAttribute(31, "aria-label", "query string");
            builder.AddContent(32, $"QueryString: {ViewModel.QueryString}");
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}