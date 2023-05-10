using Blazing.Mvvm.Sample.Wasm.Models;
using Blazing.Mvvm.Sample.Wasm.ViewModels;
using FluentAssertions;
using Moq.AutoMock;
using Xunit;

namespace Blazing.Mvvm.Tests;

public class ViewModelTests
{
    [Theory]
    [InlineData("736F6D6520746578742068657265", "some text here")]
    [InlineData("202020", "   ")]
    [InlineData("", "")]
    public void WhenReceiveConvertHexToAsciiMessage_ItConverts(string hex, string result)
    {
        // Arrange
        AutoMocker mocker = new();
        TextEntryViewModel sut = mocker.CreateInstance<TextEntryViewModel>();

        // Act
        sut.Receive(new ConvertHexToAsciiMessage(hex));

        // Assert
        sut.AsciiText.Should().Be(result);
    }

    [Theory]
    [InlineData("some text here", "736F6D6520746578742068657265")]
    [InlineData("   ", "202020")]
    [InlineData("", "")]
    public void WhenReceiveConvertAsciiToHexMessage_ItConverts(string ascii, string result)
    {
        // Arrange
        AutoMocker mocker = new();
        HexEntryViewModel sut = mocker.CreateInstance<HexEntryViewModel>();

        // Act
        sut.Receive(new ConvertAsciiToHexMessage(ascii));

        // Assert
        sut.HexText.Should().Be(result);
    }
}