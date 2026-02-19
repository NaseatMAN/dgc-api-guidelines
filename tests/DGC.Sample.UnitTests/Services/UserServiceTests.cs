using DGC.Sample.Application.Dtos;
using DGC.Sample.Application.Interfaces;
using DGC.Sample.Application.Services;
using DGC.Sample.Domain.Entities;
using DGC.Sample.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DGC.Sample.UnitTests.Services;

public sealed class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _userService = new UserService(_userRepository);
    }

    [Fact]
    public async Task CreateAsync_WhenNationalIdExists_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new UserCreateRequest("John Doe", "123456789", "012345678", "john@dgc.com");
        _userRepository.ExistsByNationalIdAsync(request.NationalId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var act = () => _userService.CreateAsync(request, default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .Where(e => e.ResponseBody.Error.Details!.Any(d => d.Message.Contains("already exists")));
    }

    [Fact]
    public async Task UpdateAsync_WhenNationalIdExistsForOtherUser_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, NationalId = "old-id" };
        var request = new UserUpdateRequest("John Doe", "new-id", "012345678", "john@dgc.com");

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(existingUser);
        _userRepository.ExistsByNationalIdAsync("new-id", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var act = () => _userService.UpdateAsync(userId, request, default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .Where(e => e.ResponseBody.Error.Details!.Any(d => d.Message.Contains("already exists")));
    }
}
