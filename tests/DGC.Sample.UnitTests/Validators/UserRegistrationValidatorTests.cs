using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Validators;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DGC.Sample.UnitTests.Validators;

public sealed class UserRegistrationValidatorTests
{
    private readonly IUserRepository _userRepository;
    private readonly UserRegistrationValidator _validator;

    public UserRegistrationValidatorTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _validator = new UserRegistrationValidator(_userRepository);
    }

    [Fact]
    public async Task ValidateAsync_WhenAdminEmailDoesNotEndWithDgc_ShouldHaveError()
    {
        // Arrange
        var request = new UserRegistrationRequest(
            "John Doe",
            "123456789",
            "012345678",
            "john@gmail.com", // Error: Should be @dgc.com for Admin
            "Admin",
            "Password123",
            "Password123"
        );

        _userRepository.ExistsByNationalIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("@dgc.com"));
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordsDoNotMatch_ShouldHaveError()
    {
        // Arrange
        var request = new UserRegistrationRequest(
            "John Doe",
            "123456789",
            "012345678",
            "john@dgc.com",
            "Admin",
            "Password123",
            "WrongPassword" // Error: Does not match
        );

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Fact]
    public async Task ValidateAsync_WhenNationalIdIsNotUnique_ShouldHaveError()
    {
        // Arrange
        var nationalId = "123456789";
        var request = new UserRegistrationRequest(
            "John Doe",
            nationalId,
            "012345678",
            "john@dgc.com",
            "Admin",
            "Password123",
            "Password123"
        );

        _userRepository.ExistsByNationalIdAsync(nationalId, Arg.Any<CancellationToken>())
            .Returns(true); // Error: Already exists

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NationalId" && e.ErrorMessage.Contains("already exists"));
    }

    [Fact]
    public async Task ValidateAsync_WhenAllRulesAreMet_ShouldBeValid()
    {
        // Arrange
        var request = new UserRegistrationRequest(
            "John Doe",
            "123456789",
            "012345678",
            "john@dgc.com",
            "Admin",
            "Password123",
            "Password123"
        );

        _userRepository.ExistsByNationalIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
