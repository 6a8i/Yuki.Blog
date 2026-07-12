using NSubstitute;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.Queries.Post;
using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Tests.Unit.Application.UseCases;

public class PostQueryHandlerTests
{
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IPostPorts _postPorts = Substitute.For<IPostPorts>();
    private readonly PostQueryHandler _sut;

    public PostQueryHandlerTests()
    {
        _sut = new PostQueryHandler(_uow, _postPorts);
    }

    [Fact]
    public async Task HandleAsync_GetAllPosts_WhenPostsExist_ShouldReturnSuccessWithPosts()
    {
        var author = new Author(Guid.NewGuid(), "John", "Doe");
        var posts = new List<Post>
        {
            new(Guid.NewGuid(), "Post 1", "Desc 1", "Content 1", author),
            new(Guid.NewGuid(), "Post 2", "Desc 2", "Content 2", author)
        };

        _postPorts.GetAllAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(posts);

        var result = await _sut.HandleAsync(new GetAllPostsQuery(false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task HandleAsync_GetAllPosts_WhenNoPostsExist_ShouldReturnSuccessWithEmptyCollection()
    {
        _postPorts.GetAllAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Post>());

        var result = await _sut.HandleAsync(new GetAllPostsQuery(false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task HandleAsync_GetAllPosts_ShouldCallPostPortsGetAllAsync()
    {
        _postPorts.GetAllAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Post>());

        await _sut.HandleAsync(new GetAllPostsQuery(false), CancellationToken.None);

        await _postPorts.Received(1).GetAllAsync(false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetAllPosts_WithIncludeAuthorTrue_ShouldPassIncludeAuthorToPorts()
    {
        _postPorts.GetAllAsync(true, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Post>());

        await _sut.HandleAsync(new GetAllPostsQuery(true), CancellationToken.None);

        await _postPorts.Received(1).GetAllAsync(true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetAllPosts_WithIncludeAuthorFalse_ShouldPassIncludeAuthorToPorts()
    {
        _postPorts.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Post>());

        await _sut.HandleAsync(new GetAllPostsQuery(false), CancellationToken.None);

        await _postPorts.Received(1).GetAllAsync(false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetAllPosts_ShouldBeginTransactionAndCommit()
    {
        _postPorts.GetAllAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Post>());

        await _sut.HandleAsync(new GetAllPostsQuery(false), CancellationToken.None);

        await _uow.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetAllPosts_WhenPortsThrowsException_ShouldRollbackAndReturnFailure()
    {
        _postPorts.GetAllAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns<IEnumerable<Post>>(_ => throw new InvalidOperationException("Database error"));

        var result = await _sut.HandleAsync(new GetAllPostsQuery(false), CancellationToken.None);

        Assert.True(result.IsFailed);
        await _uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetAllPosts_WithCancellationToken_ShouldPassTokenToPorts()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _postPorts.GetAllAsync(Arg.Any<bool>(), token)
            .Returns(Array.Empty<Post>());

        await _sut.HandleAsync(new GetAllPostsQuery(false), token);

        await _uow.Received(1).BeginTransactionAsync(token);
        await _postPorts.Received(1).GetAllAsync(Arg.Any<bool>(), token);
        await _uow.Received(1).CommitAsync(token);
    }

    [Fact]
    public async Task HandleAsync_GetPostById_WhenPostExists_ShouldReturnSuccessWithPost()
    {
        var postId = Guid.NewGuid();
        var author = new Author(Guid.NewGuid(), "John", "Doe");
        var post = new Post(postId, "My Post", "Desc", "Content", author);

        _postPorts.GetPostByIdAsync(postId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var result = await _sut.HandleAsync(new GetPostByIdQuery(postId, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(postId, result.Value!.Id);
    }

    [Fact]
    public async Task HandleAsync_GetPostById_WhenPostDoesNotExist_ShouldReturnSuccessWithNull()
    {
        var postId = Guid.NewGuid();

        _postPorts.GetPostByIdAsync(postId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        var result = await _sut.HandleAsync(new GetPostByIdQuery(postId, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task HandleAsync_GetPostById_ShouldCallPostPortsGetPostByIdAsync()
    {
        var postId = Guid.NewGuid();

        _postPorts.GetPostByIdAsync(postId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        await _sut.HandleAsync(new GetPostByIdQuery(postId, false), CancellationToken.None);

        await _postPorts.Received(1).GetPostByIdAsync(postId, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetPostById_WithIncludeAuthorTrue_ShouldPassIncludeAuthorToPorts()
    {
        var postId = Guid.NewGuid();

        _postPorts.GetPostByIdAsync(postId, true, Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        await _sut.HandleAsync(new GetPostByIdQuery(postId, true), CancellationToken.None);

        await _postPorts.Received(1).GetPostByIdAsync(postId, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetPostById_WithIncludeAuthorFalse_ShouldPassIncludeAuthorToPorts()
    {
        var postId = Guid.NewGuid();

        _postPorts.GetPostByIdAsync(postId, false, Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        await _sut.HandleAsync(new GetPostByIdQuery(postId, false), CancellationToken.None);

        await _postPorts.Received(1).GetPostByIdAsync(postId, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_GetPostById_WhenPortsThrowsException_ShouldReturnFailure()
    {
        var postId = Guid.NewGuid();

        _postPorts.GetPostByIdAsync(postId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns<Post?>(_ => throw new InvalidOperationException("Database error"));

        var result = await _sut.HandleAsync(new GetPostByIdQuery(postId, false), CancellationToken.None);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task HandleAsync_GetPostById_WithCancellationToken_ShouldPassTokenToPorts()
    {
        var postId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _postPorts.GetPostByIdAsync(postId, Arg.Any<bool>(), token)
            .Returns((Post?)null);

        await _sut.HandleAsync(new GetPostByIdQuery(postId, false), token);

        await _postPorts.Received(1).GetPostByIdAsync(postId, Arg.Any<bool>(), token);
    }

    [Fact]
    public async Task HandleAsync_GetPostById_WhenPostExistsWithAuthor_ShouldReturnPostWithAuthor()
    {
        var postId = Guid.NewGuid();
        var author = new Author(Guid.NewGuid(), "Jane", "Smith");
        var post = new Post(postId, "Title", "Desc", "Content", author);

        _postPorts.GetPostByIdAsync(postId, true, Arg.Any<CancellationToken>())
            .Returns(post);

        var result = await _sut.HandleAsync(new GetPostByIdQuery(postId, true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value!.Author);
        Assert.Equal("Jane", result.Value.Author.Name);
    }
}
