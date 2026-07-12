using FluentResults;
using NSubstitute;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Tests.Unit.Application.UseCases;

public class AuthorUseCaseTests
{
    private readonly IAuthorPorts _authorPorts = Substitute.For<IAuthorPorts>();
    private readonly AuthorUseCase _sut;

    public AuthorUseCaseTests()
    {
        _sut = new AuthorUseCase(_authorPorts);
    }

    [Fact]
    public async Task GetAuthorsAsync_WhenPortsReturnsAuthors_ShouldReturnSuccessResultWithAuthors()
    {
        var expectedAuthors = new List<Author>
        {
            new(Guid.NewGuid(), "John", "Doe"),
            new(Guid.NewGuid(), "Jane", "Smith")
        };

        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(expectedAuthors);

        var result = await _sut.GetAuthorsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAuthors, result.Value);
    }

    [Fact]
    public async Task GetAuthorsAsync_WhenPortsReturnsEmptyList_ShouldReturnSuccessResultWithEmptyCollection()
    {
        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _sut.GetAuthorsAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAuthorsAsync_WhenPortsReturnsNull_ShouldReturnSuccessResultWithNull()
    {
        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IEnumerable<Author>)null!);

        var result = await _sut.GetAuthorsAsync();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetAuthorsAsync_ShouldCallPortsGetAllAsyncOnce()
    {
        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.GetAuthorsAsync();

        await _authorPorts.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAuthorsAsync_WhenPortsThrowsException_ShouldPropagateException()
    {
        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns<Task<IEnumerable<Author>>>(_ => throw new InvalidOperationException("Database unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetAuthorsAsync());
    }

    [Fact]
    public async Task GetAuthorsAsync_WithCancellationToken_ShouldPassTokenToPorts()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _authorPorts.GetAllAsync(token)
            .Returns([]);

        await _sut.GetAuthorsAsync(token);

        await _authorPorts.Received(1).GetAllAsync(token);
    }
}
