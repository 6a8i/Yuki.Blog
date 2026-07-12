using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Domain.ValueObjects;

namespace Visma.Yuki.Blog.Tests.Unit.Domain.Entities;

public class AuthorTests
{
    [Fact]
    public void Constructor_WithValidNameAndSurname_ShouldSetProperties()
    {
        var id = Guid.NewGuid();
        const string name = "John";
        const string surname = "Doe";

        var author = new Author(id, name, surname);

        Assert.Equal(id, author.Id);
        Assert.Equal(name, author.Name);
        Assert.Equal(surname, author.Surname);
    }

    [Fact]
    public void Constructor_WithValidNameAndSurname_ShouldGenerateUniqueNameIdentifier()
    {
        var author = new Author(Guid.NewGuid(), "John", "Doe");

        Assert.NotNull(author.UniqueNameIdentifier);
        Assert.NotEmpty(author.UniqueNameIdentifier.Value);
    }

    [Fact]
    public void Constructor_WithSameNameAndSurname_ShouldGenerateSameUniqueNameIdentifier()
    {
        var author1 = new Author(Guid.NewGuid(), "John", "Doe");
        var author2 = new Author(Guid.NewGuid(), "John", "Doe");

        Assert.Equal(author1.UniqueNameIdentifier, author2.UniqueNameIdentifier);
    }

    [Fact]
    public void Constructor_WithDifferentNameAndSurname_ShouldGenerateDifferentUniqueNameIdentifier()
    {
        var author1 = new Author(Guid.NewGuid(), "John", "Doe");
        var author2 = new Author(Guid.NewGuid(), "Jane", "Smith");

        Assert.NotEqual(author1.UniqueNameIdentifier, author2.UniqueNameIdentifier);
    }

    [Fact]
    public void Constructor_WithCaseVariations_ShouldGenerateSameUniqueNameIdentifier()
    {
        var author1 = new Author(Guid.NewGuid(), "John", "Doe");
        var author2 = new Author(Guid.NewGuid(), "john", "doe");

        Assert.Equal(author1.UniqueNameIdentifier, author2.UniqueNameIdentifier);
    }

    [Fact]
    public void Constructor_WithExtraSpaces_ShouldGenerateSameUniqueNameIdentifier()
    {
        var author1 = new Author(Guid.NewGuid(), "John", "Doe");
        var author2 = new Author(Guid.NewGuid(), "  John  ", "  Doe  ");

        Assert.Equal(author1.UniqueNameIdentifier, author2.UniqueNameIdentifier);
    }

    [Fact]
    public void Constructor_WithPreExistingUniqueNameIdentifier_ShouldUseProvidedValue()
    {
        var id = Guid.NewGuid();
        const string name = "John";
        const string surname = "Doe";
        const string existingIdentifier = "abc123";

        var author = new Author(id, name, surname, existingIdentifier);

        Assert.Equal(id, author.Id);
        Assert.Equal(name, author.Name);
        Assert.Equal(surname, author.Surname);
        Assert.Equal(existingIdentifier, author.UniqueNameIdentifier.Value);
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Author(Guid.NewGuid(), null!, "Doe"));
    }

    [Fact]
    public void Constructor_WithNullSurname_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Author(Guid.NewGuid(), "John", null!));
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Author(Guid.NewGuid(), "", "Doe"));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Author(Guid.NewGuid(), "   ", "Doe"));
    }
}
