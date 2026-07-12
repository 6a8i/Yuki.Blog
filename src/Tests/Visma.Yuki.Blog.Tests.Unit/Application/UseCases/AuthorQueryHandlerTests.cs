using NSubstitute;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.Queries.Author;
using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Tests.Unit.Application.UseCases;

public class AuthorQueryHandlerTests
{
    private readonly IAuthorPorts _authorPorts = Substitute.For<IAuthorPorts>();
    private readonly AuthorQueryHandler _sut;

    public AuthorQueryHandlerTests()
    {
        _sut = new AuthorQueryHandler(_authorPorts);
    }

    [Fact]
    public async Task HandleAsync_GetAllAuthors_WhenPortsReturnsAuthors_ShouldReturnSuccessResultWithAuthors()
    {
        var expectedAuthors = new List<Author>
        {
            new(Guid.NewGuid(), "John", "Doe"),
            new(Guid.NewGuid(), "Jane", "Smith")
        };

        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(expectedAuthors);

        var result = await _sut.HandleAsync(new GetAllAuthorsQuery());

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAuthors, result.Value);
    }

    [Fact]
    public async Task HandleAsync_GetAllAuthors_WhenPortsReturnsEmptyList_ShouldReturnSuccessResultWithEmptyCollection()
    {
        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _sut.HandleAsync(new GetAllAuthorsQuery());

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_GetAllAuthors_WhenPortsReturnsNull_ShouldReturnSuccessResultWithNull()
    {
        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IEnumerable<Author>)null!);

        var result = await _sut.HandleAsync(new GetAllAuthorsQuery());

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task HandleAsync_GetAllAuthors_ShouldCallPortsGetAllAsyncOnce()
    {
        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.HandleAsync(new GetAllAuthorsQuery());

        await _authorPorts.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetAllAuthors_WhenPortsThrowsException_ShouldPropagateException()
    {
        _authorPorts.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns<Task<IEnumerable<Author>>>(_ => throw new InvalidOperationException("Database unavailable"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.HandleAsync(new GetAllAuthorsQuery()));
    }

    [Fact]
    public async Task HandleAsync_GetAllAuthors_WithCancellationToken_ShouldPassTokenToPorts()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _authorPorts.GetAllAsync(token)
            .Returns([]);

        await _sut.HandleAsync(new GetAllAuthorsQuery(), token);

        await _authorPorts.Received(1).GetAllAsync(token);
    }

    [Fact]
    public async Task HandleAsync_GetAuthorById_WhenAuthorExists_ShouldReturnSuccessWithAuthor()
    {
        var authorId = Guid.NewGuid();
        var expectedAuthor = new Author(authorId, "John", "Doe");

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns(expectedAuthor);

        var result = await _sut.HandleAsync(new GetAuthorByIdQuery(authorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAuthor, result.Value);
    }

    [Fact]
    public async Task HandleAsync_GetAuthorById_WhenAuthorDoesNotExist_ShouldReturnSuccessWithNull()
    {
        var authorId = Guid.NewGuid();

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        var result = await _sut.HandleAsync(new GetAuthorByIdQuery(authorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task HandleAsync_GetAuthorById_ShouldCallPortsGetByIdAsyncOnce()
    {
        var authorId = Guid.NewGuid();

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        await _sut.HandleAsync(new GetAuthorByIdQuery(authorId), CancellationToken.None);

        await _authorPorts.Received(1).GetByIdAsync(authorId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetAuthorById_WhenPortsThrowsException_ShouldReturnFailedResult()
    {
        var authorId = Guid.NewGuid();

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns<Task<Author?>>(_ => throw new InvalidOperationException("Database unavailable"));

        var result = await _sut.HandleAsync(new GetAuthorByIdQuery(authorId), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("Database unavailable", result.Errors[0].Message);
    }

    [Fact]
    public async Task HandleAsync_GetAuthorById_WithCancellationToken_ShouldPassTokenToPorts()
    {
        var authorId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _authorPorts.GetByIdAsync(authorId, token)
            .Returns((Author?)null);

        await _sut.HandleAsync(new GetAuthorByIdQuery(authorId), token);

        await _authorPorts.Received(1).GetByIdAsync(authorId, token);
    }
}
