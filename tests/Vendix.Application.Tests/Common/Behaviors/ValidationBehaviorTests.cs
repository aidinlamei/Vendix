using FluentValidation;
using MediatR;
using Vendix.Application.Common.Behaviors;
using ValidationException = Vendix.Application.Common.Exceptions.ValidationException;

namespace Vendix.Application.Tests.Common.Behaviors;

/// <summary>
/// Unit tests for <see cref="ValidationBehavior{TRequest,TResponse}"/>.
/// </summary>
public class ValidationBehaviorTests
{
    // Test request and response types
    public record TestRequest(string Name, int Value) : IRequest<TestResponse>;
    public record TestResponse(bool Success);

    // Test validator
    public class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.");

            RuleFor(x => x.Value)
                .GreaterThan(0)
                .WithMessage("Value must be greater than zero.");
        }
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCallNextHandler()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest("Valid Name", 10);
        var expectedResponse = new TestResponse(true);
        var nextCalled = false;

        RequestHandlerDelegate<TestResponse> next = (CancellationToken ct) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest("", -1); // Invalid: empty name and negative value

        RequestHandlerDelegate<TestResponse> next = (CancellationToken ct) => Task.FromResult(new TestResponse(true));

        // Act
        var act = () => behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Name");
        exception.Which.Errors.Should().ContainKey("Value");
    }

    [Fact]
    public async Task Handle_NoValidators_ShouldCallNextHandler()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest("", -1); // Would be invalid, but no validators
        var expectedResponse = new TestResponse(true);
        var nextCalled = false;

        RequestHandlerDelegate<TestResponse> next = (CancellationToken ct) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_PartiallyInvalidRequest_ShouldThrowValidationExceptionWithSpecificErrors()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest("Valid Name", -1); // Only value is invalid

        RequestHandlerDelegate<TestResponse> next = (CancellationToken ct) => Task.FromResult(new TestResponse(true));

        // Act
        var act = () => behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().NotContainKey("Name");
        exception.Which.Errors.Should().ContainKey("Value");
    }

    [Fact]
    public async Task Handle_MultipleValidators_ShouldCombineErrors()
    {
        // Arrange
        var additionalValidator = new InlineValidator<TestRequest>();
        additionalValidator.RuleFor(x => x.Name)
            .MaximumLength(5)
            .WithMessage("Name is too long.");

        var validators = new List<IValidator<TestRequest>>
        {
            new TestRequestValidator(),
            additionalValidator
        };

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest("TooLongName", 10); // Valid for first validator, invalid for second

        RequestHandlerDelegate<TestResponse> next = (CancellationToken ct) => Task.FromResult(new TestResponse(true));

        // Act
        var act = () => behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Name");
        exception.Which.Errors["Name"].Should().Contain(e => e.Contains("too long"));
    }
}
