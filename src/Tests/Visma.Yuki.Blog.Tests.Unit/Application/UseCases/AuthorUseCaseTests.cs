using FluentResults;
using FluentValidation;
using NSubstitute;
using Visma.Yuki.Blog.Application.Commands.Author;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;

namespace Visma.Yuki.Blog.Tests.Unit.Application.UseCases;

public class AuthorUseCaseTests
{
    private readonly IAuthorPorts _authorPorts = Substitute.For<IAuthorPorts>();
    private readonly IValidator<CreateAuthorCommand> _createAuthorValidator = new CreateAuthorCommandValidator();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly AuthorUseCase _sut;

    public AuthorUseCaseTests()
    {
        _sut = new AuthorUseCase(_authorPorts, _createAuthorValidator, _uow);
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

    [Fact]
    public async Task GetAuthorAsync_WhenAuthorExists_ShouldReturnSuccessWithAuthor()
    {
        var authorId = Guid.NewGuid();
        var expectedAuthor = new Author(authorId, "John", "Doe");

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns(expectedAuthor);

        var result = await _sut.GetAuthorAsync(authorId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAuthor, result.Value);
    }

    [Fact]
    public async Task GetAuthorAsync_WhenAuthorDoesNotExist_ShouldReturnSuccessWithNull()
    {
        var authorId = Guid.NewGuid();

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        var result = await _sut.GetAuthorAsync(authorId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetAuthorAsync_ShouldCallPortsGetByIdAsyncOnce()
    {
        var authorId = Guid.NewGuid();

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        await _sut.GetAuthorAsync(authorId, CancellationToken.None);

        await _authorPorts.Received(1).GetByIdAsync(authorId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAuthorAsync_WhenPortsThrowsException_ShouldReturnFailedResult()
    {
        var authorId = Guid.NewGuid();

        _authorPorts.GetByIdAsync(authorId, Arg.Any<CancellationToken>())
            .Returns<Task<Author?>>(_ => throw new InvalidOperationException("Database unavailable"));

        var result = await _sut.GetAuthorAsync(authorId, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("Database unavailable", result.Errors[0].Message);
    }

    [Fact]
    public async Task GetAuthorAsync_WithCancellationToken_ShouldPassTokenToPorts()
    {
        var authorId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _authorPorts.GetByIdAsync(authorId, token)
            .Returns((Author?)null);

        await _sut.GetAuthorAsync(authorId, token);

        await _authorPorts.Received(1).GetByIdAsync(authorId, token);
    }

    [Fact]
    public async Task CreateAuthorAsync_WithValidCommand_ShouldReturnSuccessWithAuthor()
    {
        var command = new CreateAuthorCommand("John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        var result = await _sut.CreateAuthorAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("John", result.Value.Name);
        Assert.Equal("Doe", result.Value.Surname);
    }

    [Fact]
    public async Task CreateAuthorAsync_WithValidCommand_ShouldCallAddAsyncAndCommit()
    {
        var command = new CreateAuthorCommand("John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        await _sut.CreateAuthorAsync(command, CancellationToken.None);

        await _authorPorts.Received(1).AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAuthorAsync_WithValidCommand_ShouldBeginTransaction()
    {
        var command = new CreateAuthorCommand("John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        await _sut.CreateAuthorAsync(command, CancellationToken.None);

        await _uow.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAuthorAsync_WhenAuthorAlreadyExists_ShouldReturnFailure()
    {
        var command = new CreateAuthorCommand("John", "Doe");
        var existingAuthor = new Author(Guid.NewGuid(), "John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns(existingAuthor);

        var result = await _sut.CreateAuthorAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("already exists", result.Errors[0].Message);
    }

    [Fact]
    public async Task CreateAuthorAsync_WhenAuthorAlreadyExists_ShouldNotCallAddAsync()
    {
        var command = new CreateAuthorCommand("John", "Doe");
        var existingAuthor = new Author(Guid.NewGuid(), "John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns(existingAuthor);

        await _sut.CreateAuthorAsync(command, CancellationToken.None);

        await _authorPorts.DidNotReceive().AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAuthorAsync_WhenNameIsEmpty_ShouldReturnValidationFailure()
    {
        var command = new CreateAuthorCommand("", "Doe");

        var result = await _sut.CreateAuthorAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("name is required"));
    }

    [Fact]
    public async Task CreateAuthorAsync_WhenSurnameIsEmpty_ShouldReturnValidationFailure()
    {
        var command = new CreateAuthorCommand("John", "");

        var result = await _sut.CreateAuthorAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("surname is required"));
    }

    [Fact]
    public async Task CreateAuthorAsync_WhenNameExceedsMaxLength_ShouldReturnValidationFailure()
    {
        var longName = new string('a', 151);
        var command = new CreateAuthorCommand(longName, "Doe");

        var result = await _sut.CreateAuthorAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("Name cannot exceed 150 characters"));
    }

    [Fact]
    public async Task CreateAuthorAsync_WhenSurnameExceedsMaxLength_ShouldReturnValidationFailure()
    {
        var longSurname = new string('a', 151);
        var command = new CreateAuthorCommand("John", longSurname);

        var result = await _sut.CreateAuthorAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("Surname cannot exceed 150 characters"));
    }

    [Fact]
    public async Task CreateAuthorAsync_WhenValidationFails_ShouldNotBeginTransaction()
    {
        var command = new CreateAuthorCommand("", "");

        await _sut.CreateAuthorAsync(command, CancellationToken.None);

        await _uow.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAuthorAsync_WhenAddAsyncThrowsException_ShouldRollbackAndReturnFailure()
    {
        var command = new CreateAuthorCommand("John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        _authorPorts.AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Database error"));

        var result = await _sut.CreateAuthorAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("Database error", result.Errors[0].Message);
        await _uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAuthorAsync_WithCancellationToken_ShouldPassTokenToPorts()
    {
        var command = new CreateAuthorCommand("John", "Doe");
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), token)
            .Returns((Author?)null);

        await _sut.CreateAuthorAsync(command, token);

        await _uow.Received(1).BeginTransactionAsync(token);
        await _authorPorts.Received(1).GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), token);
        await _authorPorts.Received(1).AddAsync(Arg.Any<Author>(), token);
        await _uow.Received(1).CommitAsync(token);
    }
}
