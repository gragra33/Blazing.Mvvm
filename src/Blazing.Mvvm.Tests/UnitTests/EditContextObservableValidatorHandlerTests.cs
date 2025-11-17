using Blazing.Mvvm.Components.Validation;
using Blazing.Mvvm.Sample.WebApp.Client.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="EditContextObservableValidatorHandler"/> covering validation, error handling, and disposal scenarios.
/// </summary>
public class EditContextObservableValidatorHandlerTests
{
    /// <summary>
    /// Tests that instantiating the handler with an invalid model throws <see cref="InvalidOperationException"/>.
    /// </summary>
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

    /// <summary>
    /// Tests that validation requests validate the model and update validation messages.
    /// </summary>
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

    /// <summary>
    /// Tests that invalid model property updates result in validation errors.
    /// </summary>
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

    /// <summary>
    /// Tests that valid model property updates clear previous validation errors.
    /// </summary>
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

    /// <summary>
    /// Tests that disposing the handler unsubscribes from events and clears validation messages.
    /// </summary>
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
