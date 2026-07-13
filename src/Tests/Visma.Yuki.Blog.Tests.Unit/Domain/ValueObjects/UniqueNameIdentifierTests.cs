using Visma.Yuki.Blog.Domain.ValueObjects;

namespace Visma.Yuki.Blog.Tests.Unit.Domain.ValueObjects;

public class UniqueNameIdentifierTests
{
    [Fact]
    public void Create_WithValidNameAndSurname_ShouldReturnIdentifierWithValue()
    {
        var identifier = UniqueNameIdentifier.Create("John", "Doe");

        Assert.NotEmpty(identifier.Value);
    }

    [Fact]
    public void Create_WithValidNameAndSurname_ShouldReturnHashOfCorrectLength()
    {
        var identifier = UniqueNameIdentifier.Create("John", "Doe");

        Assert.True(identifier.Value.Length <= 50);
    }

    [Fact]
    public void Create_WithSameNameAndSurname_ShouldReturnSameValue()
    {
        var id1 = UniqueNameIdentifier.Create("John", "Doe");
        var id2 = UniqueNameIdentifier.Create("John", "Doe");

        Assert.Equal(id1.Value, id2.Value);
    }

    [Fact]
    public void Create_WithDifferentNameAndSurname_ShouldReturnDifferentValue()
    {
        var id1 = UniqueNameIdentifier.Create("John", "Doe");
        var id2 = UniqueNameIdentifier.Create("Jane", "Smith");

        Assert.NotEqual(id1.Value, id2.Value);
    }

    [Fact]
    public void Create_WithSameNameDifferentSurname_ShouldReturnDifferentValue()
    {
        var id1 = UniqueNameIdentifier.Create("John", "Doe");
        var id2 = UniqueNameIdentifier.Create("John", "Smith");

        Assert.NotEqual(id1.Value, id2.Value);
    }

    [Fact]
    public void Create_WithDifferentNameSameSurname_ShouldReturnDifferentValue()
    {
        var id1 = UniqueNameIdentifier.Create("John", "Doe");
        var id2 = UniqueNameIdentifier.Create("Jane", "Doe");

        Assert.NotEqual(id1.Value, id2.Value);
    }

    [Theory]
    [InlineData("John", "Doe", "john", "doe")]
    [InlineData("JOHN", "DOE", "john", "doe")]
    [InlineData("John", "Doe", "JOHN", "DOE")]
    public void Create_WithCaseVariations_ShouldReturnSameValue(
        string name1, string surname1, string name2, string surname2)
    {
        var id1 = UniqueNameIdentifier.Create(name1, surname1);
        var id2 = UniqueNameIdentifier.Create(name2, surname2);

        Assert.Equal(id1.Value, id2.Value);
    }

    [Theory]
    [InlineData("  John  ", "  Doe  ", "John", "Doe")]
    [InlineData("John", "Doe", "  John  ", "  Doe  ")]
    [InlineData("\tJohn\t", "\tDoe\t", "John", "Doe")]
    public void Create_WithExtraWhitespace_ShouldNormalizeAndReturnSameValue(
        string name1, string surname1, string name2, string surname2)
    {
        var id1 = UniqueNameIdentifier.Create(name1, surname1);
        var id2 = UniqueNameIdentifier.Create(name2, surname2);

        Assert.Equal(id1.Value, id2.Value);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => UniqueNameIdentifier.Create(null!, "Doe"));
    }

    [Fact]
    public void Create_WithNullSurname_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => UniqueNameIdentifier.Create("John", null!));
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("   ", "Doe")]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    public void Create_WithEmptyOrWhitespaceNameOrSurname_ShouldThrowArgumentException(
        string name, string surname)
    {
        Assert.Throws<ArgumentException>(() => UniqueNameIdentifier.Create(name, surname));
    }

    [Theory]
    [InlineData("123", "Doe")]
    [InlineData("John", "456")]
    [InlineData("123", "456")]
    public void Create_WithNumericNameOrSurname_ShouldThrowArgumentException(string name, string surname)
    {
        var ex = Assert.Throws<ArgumentException>(() => UniqueNameIdentifier.Create(name, surname));
        Assert.Contains("cannot consist solely of numbers", ex.Message);
    }

    [Fact]
    public void FromString_WithValidValue_ShouldReturnIdentifierWithThatValue()
    {
        const string value = "abc123hash";

        var identifier = UniqueNameIdentifier.FromString(value);

        Assert.Equal(value, identifier.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromString_WithNullEmptyOrWhitespace_ShouldThrowArgumentException(string? value)
    {
        Assert.Throws<ArgumentException>(() => UniqueNameIdentifier.FromString(value!));
    }

    [Fact]
    public void FromString_WithSameValue_ShouldReturnEqualIdentifiers()
    {
        const string value = "abc123hash";

        var id1 = UniqueNameIdentifier.FromString(value);
        var id2 = UniqueNameIdentifier.FromString(value);

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        var id1 = UniqueNameIdentifier.Create("John", "Doe");
        var id2 = UniqueNameIdentifier.Create("John", "Doe");

        Assert.True(id1.Equals(id2));
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        var id1 = UniqueNameIdentifier.Create("John", "Doe");
        var id2 = UniqueNameIdentifier.Create("Jane", "Smith");

        Assert.False(id1.Equals(id2));
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var id = UniqueNameIdentifier.Create("John", "Doe");

        Assert.False(id.Equals(null));
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        var id = UniqueNameIdentifier.Create("John", "Doe");

        Assert.False(id.Equals("some string"));
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        var id1 = UniqueNameIdentifier.Create("John", "Doe");
        var id2 = UniqueNameIdentifier.Create("John", "Doe");

        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var id = UniqueNameIdentifier.FromString("abc123hash");

        Assert.Equal("abc123hash", id.ToString());
    }
}
