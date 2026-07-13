using FluentValidation;
using NSubstitute;
using Visma.Yuki.Blog.Application.Commands.Post;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;

namespace Visma.Yuki.Blog.Tests.Unit.Application.UseCases;

public class PostCommandHandlerTests
{
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreatePostCommand> _createPostValidator = new CreatePostCommandValidator();
    private readonly IAuthorPorts _authorPorts = Substitute.For<IAuthorPorts>();
    private readonly IPostPorts _postPorts = Substitute.For<IPostPorts>();
    private readonly PostCommandHandler _sut;

    public PostCommandHandlerTests()
    {
        _sut = new PostCommandHandler(_uow, _createPostValidator, _authorPorts, _postPorts);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommandAndAuthorId_ShouldReturnSuccessWithPostId()
    {
        var command = new CreatePostCommand("My Post", "Description", "Content", Guid.NewGuid(), null, null);
        var author = new Author(command.AuthorId!.Value, "John", "Doe");

        _authorPorts.GetByIdAsync(command.AuthorId.Value, Arg.Any<CancellationToken>())
            .Returns(author);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommandAndAuthorName_ShouldReturnSuccessWithPostId()
    {
        var command = new CreatePostCommand("My Post", "Description", "Content", null, "John", "Doe");
        var author = new Author(Guid.NewGuid(), "John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns(author);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public async Task HandleAsync_WithAuthorNameAndAuthorNotFound_ShouldCreateAuthorAndPost()
    {
        var command = new CreatePostCommand("My Post", null, "Content", null, "Jane", "Smith");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _authorPorts.Received(1).AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>());
        await _postPorts.Received(1).AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithAuthorIdAndAuthorNotFound_ShouldReturnFailure()
    {
        var authorId = Guid.NewGuid();
        var command = new CreatePostCommand("My Post", "Description", "Content", authorId, null, null);

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("doesn't exists", result.Errors[0].Message);
    }

    [Fact]
    public async Task HandleAsync_WithAuthorIdAndAuthorNotFound_ShouldNotCallAddAsync()
    {
        var authorId = Guid.NewGuid();
        var command = new CreatePostCommand("My Post", "Description", "Content", authorId, null, null);

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        await _sut.HandleAsync(command, CancellationToken.None);

        await _postPorts.DidNotReceive().AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldBeginTransactionAndCommit()
    {
        var command = new CreatePostCommand("My Post", "Description", "Content", Guid.NewGuid(), null, null);
        var author = new Author(command.AuthorId!.Value, "John", "Doe");

        _authorPorts.GetByIdAsync(command.AuthorId.Value, Arg.Any<CancellationToken>())
            .Returns(author);

        await _sut.HandleAsync(command, CancellationToken.None);

        await _uow.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenTitleIsEmpty_ShouldReturnValidationFailure()
    {
        var command = new CreatePostCommand("", "Description", "Content", Guid.NewGuid(), null, null);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("Title is required"));
    }

    [Fact]
    public async Task HandleAsync_WhenContentIsEmpty_ShouldReturnValidationFailure()
    {
        var command = new CreatePostCommand("My Post", "Description", "", Guid.NewGuid(), null, null);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("Content is required"));
    }

    [Fact]
    public async Task HandleAsync_WhenNoAuthorIdentification_ShouldReturnValidationFailure()
    {
        var command = new CreatePostCommand("My Post", "Description", "Content", null, null, null);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("AuthorId"));
    }

    [Fact]
    public async Task HandleAsync_WhenTitleExceedsMaxLength_ShouldReturnValidationFailure()
    {
        var longTitle = new string('a', 201);
        var command = new CreatePostCommand(longTitle, "Description", "Content", Guid.NewGuid(), null, null);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("200 characters"));
    }

    [Fact]
    public async Task HandleAsync_WhenDescriptionExceedsMaxLength_ShouldReturnValidationFailure()
    {
        var longDescription = new string('a', 301);
        var command = new CreatePostCommand("My Post", longDescription, "Content", Guid.NewGuid(), null, null);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("300 characters"));
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldNotBeginTransaction()
    {
        var command = new CreatePostCommand("", "Description", "Content", Guid.NewGuid(), null, null);

        await _sut.HandleAsync(command, CancellationToken.None);

        await _uow.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAddAsyncThrowsException_ShouldRollbackAndReturnFailure()
    {
        var command = new CreatePostCommand("My Post", "Description", "Content", Guid.NewGuid(), null, null);
        var author = new Author(command.AuthorId!.Value, "John", "Doe");

        _authorPorts.GetByIdAsync(command.AuthorId.Value, Arg.Any<CancellationToken>())
            .Returns(author);

        _postPorts.AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Database error"));

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        await _uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToPorts()
    {
        var command = new CreatePostCommand("My Post", "Description", "Content", Guid.NewGuid(), null, null);
        var author = new Author(command.AuthorId!.Value, "John", "Doe");
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _authorPorts.GetByIdAsync(command.AuthorId!.Value, token)
            .Returns(author);

        await _sut.HandleAsync(command, token);

        await _uow.Received(1).BeginTransactionAsync(token);
        await _authorPorts.Received(1).GetByIdAsync(command.AuthorId!.Value, token);
        await _postPorts.Received(1).AddAsync(Arg.Any<Post>(), token);
        await _uow.Received(1).CommitAsync(token);
    }

    [Fact]
    public async Task HandleAsync_WithNoAuthorIdentificationAndValidationBypassed_ShouldReturnFailure()
    {
        var passthroughValidator = Substitute.For<IValidator<CreatePostCommand>>();
        passthroughValidator.ValidateAsync(Arg.Any<CreatePostCommand>(), Arg.Any<CancellationToken>())
            .Returns(new FluentValidation.Results.ValidationResult());

        var sut = new PostCommandHandler(_uow, passthroughValidator, _authorPorts, _postPorts);
        var command = new CreatePostCommand("My Post", "Description", "Content", null, null, null);

        var result = await sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("You must provide either", result.Errors[0].Message);
    }

    [Fact]
    public async Task HandleAsync_WithExistingAuthorByName_ShouldNotCreateNewAuthor()
    {
        var command = new CreatePostCommand("My Post", null, "Content", null, "John", "Doe");
        var existingAuthor = new Author(Guid.NewGuid(), "John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns(existingAuthor);

        await _sut.HandleAsync(command, CancellationToken.None);

        await _authorPorts.DidNotReceive().AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>());
        await _postPorts.Received(1).AddAsync(Arg.Is<Post>(p => p.Author.Id == existingAuthor.Id), Arg.Any<CancellationToken>());
    }
}
