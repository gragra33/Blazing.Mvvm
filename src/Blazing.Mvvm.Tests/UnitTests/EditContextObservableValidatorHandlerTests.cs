using Blazing.Mvvm.Components.Validation;
using Blazing.Mvvm.Sample.WebApp.Client.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazing.Mvvm.Tests.UnitTests;

public class EditContextObservableValidatorHandlerTests
{
    [Fact]
    public void GivenInstantiation_WhenCannotHandleEditContextModel_ShouldThrowInvalidOperationException()
    {
        // Arrange
        const string expectedMessage = "EditContextObservableValidatorHandler requires a model that inherits from ObservableValidator and has a protected method named 'ValidateAllProperties'.";
        var editContext = new EditContext(new object());
        editContext.GetValidationMessages();

        // Act
        var act = () => new EditContextObservableValidatorHandler(editContext);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public void GivenInstantiated_WhenValidationsRequested_ShouldValidateModel()
    {
        // Arrange
        var contactInfo = new ContactInfo();
        var editContext = new EditContext(contactInfo);
        var sut = new EditContextObservableValidatorHandler(editContext);

        // Act
        editContext.Validate();

        // Assert
        editContext.GetValidationMessages().Should().NotBeEmpty();
        contactInfo.GetErrors().Should().NotBeEmpty();
        editContext.GetValidationMessages().Should().BeEquivalentTo(contactInfo.GetErrors().Select(x => x.ErrorMessage));
    }

    [Fact]
    public void GivenModelPropertyHasNoErrors_WhenModelPropertyIsInvalid_ShouldUpdateValidationMessagesWithErrors()
    {
        // Arrange
        var contactInfo = new ContactInfo();
        var editContext = new EditContext(contactInfo);
        var sut = new EditContextObservableValidatorHandler(editContext);

        // Act
        contactInfo.Name = "1";

        // Assert
        editContext.GetValidationMessages().Should().NotBeEmpty();
        contactInfo.GetErrors().Should().NotBeEmpty();
        editContext.GetValidationMessages().Should().BeEquivalentTo(contactInfo.GetErrors().Select(x => x.ErrorMessage));
    }

    [Fact]
    public void GivenModelPropertyHasErrors_WhenModelPropertyIsValid_ShouldClearPropertyErrors()
    {
        // Arrange
        var contactInfo = new ContactInfo();
        var editContext = new EditContext(contactInfo);
        var sut = new EditContextObservableValidatorHandler(editContext);
        contactInfo.Name = "1";

        // Act
        contactInfo.Name = "John Doe";

        // Assert
        editContext.GetValidationMessages().Should().BeEmpty();
        contactInfo.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void GivenHandler_WhenDisposed_ShouldUnsubscribeFromEventsAndClearValidationMessages()
    {
        var editContext = new EditContext(new ContactInfo());
        var sut = new EditContextObservableValidatorHandler(editContext);
        editContext.Validate();

        // Act
        sut.Dispose();
        editContext.Validate();

        // Assert
        editContext.GetValidationMessages().Should().BeEmpty();
    }
}
