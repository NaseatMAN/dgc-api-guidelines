using DGC.Sample.Api.Extensions;
using DGC.Sample.Domain.Constants.ApiErrorConstants;
using DGC.Sample.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DGC.Sample.UnitTests.Extensions;

public sealed class AzureProblemDetailsWriterTests
{
    private readonly AzureProblemDetailsWriter _writer;

    public AzureProblemDetailsWriterTests()
    {
        _writer = new AzureProblemDetailsWriter();
    }

    [Fact]
    public async Task WriteAsync_WhenMissingApiVersion_ShouldThrowBadRequestException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var problemDetails = new ProblemDetails
        {
            Type = "https://docs.api-versioning.org/problems#unspecified",
            Title = "API versioning error",
            Status = StatusCodes.Status400BadRequest
        };
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        };

        // Act
        var act = async () => await _writer.WriteAsync(context);

        // Assert
        var exception = await act.Should().ThrowAsync<BadRequestException>();
        exception.Which.ResponseBody.Error.Code.Should().Be(BadRequestErrorCode.MissingApiVersionParameter);
        exception.Which.Message.Should().Be("The api-version query parameter (?api-version=) is required for all requests");
    }

    [Fact]
    public async Task WriteAsync_WhenUnsupportedApiVersion_ShouldThrowBadRequestException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?api-version=1.0");
        
        var problemDetails = new ProblemDetails
        {
            Type = "https://docs.api-versioning.org/problems#unsupported",
            Title = "API versioning error",
            Status = StatusCodes.Status400BadRequest
        };
        problemDetails.Extensions.Add("apiVersion", "1.0");

        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        };

        // Act
        var act = async () => await _writer.WriteAsync(context);

        // Assert
        var exception = await act.Should().ThrowAsync<BadRequestException>();
        exception.Which.ResponseBody.Error.Code.Should().Be(BadRequestErrorCode.UnsupportedApiVersionValue);
        exception.Which.Message.Should().Be("Unsupported api-version '1.0'.");
    }

    [Fact]
    public void CanWrite_ShouldReturnTrue()
    {
        // Arrange
        var context = new ProblemDetailsContext
        {
            HttpContext = new DefaultHttpContext(),
            ProblemDetails = new ProblemDetails()
        };

        // Act
        var result = _writer.CanWrite(context);

        // Assert
        result.Should().BeTrue();
    }
}
