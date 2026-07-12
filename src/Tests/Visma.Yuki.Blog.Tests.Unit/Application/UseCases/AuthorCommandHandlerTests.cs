using FluentValidation;
using NSubstitute;
using Visma.Yuki.Blog.Application.Commands.Author;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Application.UseCases;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;

namespace Visma.Yuki.Blog.Tests.Unit.Application.UseCases;

public class AuthorCommandHandlerTests
{
    private readonly IAuthorPorts _authorPorts = Substitute.For<IAuthorPorts>();
    private readonly IValidator<CreateAuthorCommand> _createAuthorValidator = new CreateAuthorCommandValidator();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly AuthorCommandHandler _sut;

    public AuthorCommandHandlerTests()
    {
        _sut = new AuthorCommandHandler(_authorPorts, _createAuthorValidator, _uow);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessWithAuthor()
    {
        var command = new CreateAuthorCommand("John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("John", result.Value.Name);
        Assert.Equal("Doe", result.Value.Surname);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCallAddAsyncAndCommit()
    {
        var command = new CreateAuthorCommand("John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        await _sut.HandleAsync(command, CancellationToken.None);

        await _authorPorts.Received(1).AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldBeginTransaction()
    {
        var command = new CreateAuthorCommand("John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        await _sut.HandleAsync(command, CancellationToken.None);

        await _uow.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAuthorAlreadyExists_ShouldReturnFailure()
    {
        var command = new CreateAuthorCommand("John", "Doe");
        var existingAuthor = new Author(Guid.NewGuid(), "John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns(existingAuthor);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("already exists", result.Errors[0].Message);
    }

    [Fact]
    public async Task HandleAsync_WhenAuthorAlreadyExists_ShouldNotCallAddAsync()
    {
        var command = new CreateAuthorCommand("John", "Doe");
        var existingAuthor = new Author(Guid.NewGuid(), "John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns(existingAuthor);

        await _sut.HandleAsync(command, CancellationToken.None);

        await _authorPorts.DidNotReceive().AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsEmpty_ShouldReturnValidationFailure()
    {
        var command = new CreateAuthorCommand("", "Doe");

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("name is required"));
    }

    [Fact]
    public async Task HandleAsync_WhenSurnameIsEmpty_ShouldReturnValidationFailure()
    {
        var command = new CreateAuthorCommand("John", "");

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("surname is required"));
    }

    [Fact]
    public async Task HandleAsync_WhenNameExceedsMaxLength_ShouldReturnValidationFailure()
    {
        var longName = new string('a', 151);
        var command = new CreateAuthorCommand(longName, "Doe");

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("Name cannot exceed 150 characters"));
    }

    [Fact]
    public async Task HandleAsync_WhenSurnameExceedsMaxLength_ShouldReturnValidationFailure()
    {
        var longSurname = new string('a', 151);
        var command = new CreateAuthorCommand("John", longSurname);

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("Surname cannot exceed 150 characters"));
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldNotBeginTransaction()
    {
        var command = new CreateAuthorCommand("", "");

        await _sut.HandleAsync(command, CancellationToken.None);

        await _uow.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAddAsyncThrowsException_ShouldRollbackAndReturnFailure()
    {
        var command = new CreateAuthorCommand("John", "Doe");

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), Arg.Any<CancellationToken>())
            .Returns((Author?)null);

        _authorPorts.AddAsync(Arg.Any<Author>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Database error"));

        var result = await _sut.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("Database error", result.Errors[0].Message);
        await _uow.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToPorts()
    {
        var command = new CreateAuthorCommand("John", "Doe");
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _authorPorts.GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), token)
            .Returns((Author?)null);

        await _sut.HandleAsync(command, token);

        await _uow.Received(1).BeginTransactionAsync(token);
        await _authorPorts.Received(1).GetByUniqueNameIdentifierAsync(Arg.Any<UniqueNameIdentifier>(), token);
        await _authorPorts.Received(1).AddAsync(Arg.Any<Author>(), token);
        await _uow.Received(1).CommitAsync(token);
    }
}
