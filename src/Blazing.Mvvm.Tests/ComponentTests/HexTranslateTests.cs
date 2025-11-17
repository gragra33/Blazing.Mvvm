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

    /// <summary>
    /// Initializes a new instance of the <see cref="HexTranslateTests"/> class and registers required services and view models.
    /// </summary>
    public HexTranslateTests()
    {
        Services.AddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);
        Services.AddKeyedSingleton<HexTranslateViewModel>(nameof(HexTranslateViewModel));
        Services.AddSingleton<HexEntryViewModel>();
        Services.AddSingleton<TextEntryViewModel>();
    }

    /// <summary>
    /// Verifies that the Send Ascii button is disabled when the ASCII input is invalid.
    /// </summary>
    /// <param name="input">The ASCII input value to test.</param>
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

    /// <summary>
    /// Verifies that the Send Hex button is disabled when the Hex input is invalid.
    /// </summary>
    /// <param name="input">The Hex input value to test.</param>
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

    /// <summary>
    /// Verifies that clicking the Send Ascii button sends a message to convert ASCII to Hex and updates the UI and view models.
    /// </summary>
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

    /// <summary>
    /// Verifies that clicking the Send Hex button sends a message to convert Hex to ASCII and updates the UI and view models.
    /// </summary>
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

    /// <summary>
    /// Verifies that clicking the clear button clears both input fields and resets the view models.
    /// </summary>
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
