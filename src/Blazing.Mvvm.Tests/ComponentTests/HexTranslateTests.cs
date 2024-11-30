using AngleSharp.Dom;
using Blazing.Mvvm.Sample.WebApp.Client.Pages;
using Blazing.Mvvm.Sample.WebApp.Client.ViewModels;
using Bunit;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.ComponentTests;

public class HexTranslateTests : ComponentTestBase
{
    private const string AsciiInputAriaLabel = "ascii input";
    private const string HexInputAriaLabel = "hex input";

    private const string SendAsciiButtonSelector = "#send-ascii";
    private const string SendHexButtonSelector = "#send-hex";

    public HexTranslateTests()
    {
        Services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);
        Services.AddKeyedSingleton<HexTranslateViewModel>(nameof(HexTranslateViewModel));
        Services.AddSingleton<HexEntryViewModel>();
        Services.AddSingleton<TextEntryViewModel>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenComponentRendered_WhenAsciiInputInvalid_ThenSendAsciiButtonShouldBeDisabled(string input)
    {
        // Arrange
        var cut = RenderComponent<HexTranslate>();

        // Act
        cut.FindByLabelText(AsciiInputAriaLabel).Input(new ChangeEventArgs { Value = input });

        // Assert
        cut.Find(SendAsciiButtonSelector).IsDisabled().Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenComponentRendered_WhenHexInputInvalid_ThenSendHexButtonShouldBeDisabled(string input)
    {
        // Arrange
        var cut = RenderComponent<HexTranslate>();

        // Act
        cut.FindByLabelText(HexInputAriaLabel).Input(new ChangeEventArgs { Value = input });

        // Assert
        cut.Find(SendHexButtonSelector).IsDisabled().Should().BeTrue();
    }

    [Fact]
    public void GivenAsciiInputValid_WhenSendAsciiButtonClicked_ThenShouldSendConvertAsciiToHexMessage()
    {
        // Arrange
        const string input = "some text here";
        const string expectedHex = "736F6D6520746578742068657265";

        var cut = RenderComponent<HexTranslate>();
        var textEntryViewModel = GetViewModel<TextEntryViewModel>();
        var hexEntryViewModel = GetViewModel<HexEntryViewModel>();
        var asciiTextInput = cut.FindByLabelText(AsciiInputAriaLabel);

        asciiTextInput.Input(new ChangeEventArgs { Value = input });

        // Act
        cut.Find(SendAsciiButtonSelector).Click();

        // Assert
        using var _ = new AssertionScope();
        cut.FindByLabelText(HexInputAriaLabel).GetAttribute(AttributeNames.Value).Should().Be(expectedHex);
        hexEntryViewModel.HexText.Should().Be(expectedHex);
        asciiTextInput.GetAttribute(AttributeNames.Value).Should().Be(input);
        textEntryViewModel.AsciiText.Should().Be(input);
    }

    [Fact]
    public void GivenHexInputValid_WhenSendHexButtonClicked_ThenShouldSendConvertHexToAsciiMessage()
    {
        // Arrange
        const string input = "736F6D6520746578742068657265";
        const string expectedAscii = "some text here";

        var cut = RenderComponent<HexTranslate>();
        var hexEntryViewModel = GetViewModel<HexEntryViewModel>();
        var textEntryViewModel = GetViewModel<TextEntryViewModel>();
        var hexTextInput = cut.FindByLabelText(HexInputAriaLabel);

        hexTextInput.Input(new ChangeEventArgs { Value = input });

        // Act
        cut.Find(SendHexButtonSelector).Click();

        // Assert
        using var _ = new AssertionScope();
        cut.FindByLabelText(AsciiInputAriaLabel).GetAttribute(AttributeNames.Value).Should().Be(expectedAscii);
        textEntryViewModel.AsciiText.Should().Be(expectedAscii);
        hexTextInput.GetAttribute(AttributeNames.Value).Should().Be(input);
        hexEntryViewModel.HexText.Should().Be(input);
    }

    [Fact]
    public void GivenInputsHaveValues_WhenClearButtonClicked_ThenShouldClearInputs()
    {
        // Arrange
        const string clearButtonSelector = "#reset-child-inputs";
        const string asciiInput = "some text here";
        const string hexInput = "736F6D6520746578742068657265";

        var cut = RenderComponent<HexTranslate>();
        var hexEntryViewModel = GetViewModel<HexEntryViewModel>();
        var textEntryViewModel = GetViewModel<TextEntryViewModel>();

        textEntryViewModel.AsciiText = asciiInput;
        hexEntryViewModel.HexText = hexInput;

        // Act
        cut.Find(clearButtonSelector).Click();

        // Assert
        using var _ = new AssertionScope();
        textEntryViewModel.AsciiText.Should().BeEmpty();
        hexEntryViewModel.HexText.Should().BeEmpty();
        cut.FindByLabelText(HexInputAriaLabel).GetAttribute(AttributeNames.Value).Should().BeEmpty();
        cut.FindByLabelText(AsciiInputAriaLabel).GetAttribute(AttributeNames.Value).Should().BeEmpty();
    }
}
