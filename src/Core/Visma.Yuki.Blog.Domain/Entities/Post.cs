namespace Visma.Yuki.Blog.Domain.Entities;

public class Post
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }

    public string? Description { get; private set; }

    public string Content { get; private set; }

    public Guid AuthorId { get; set; }

    public Author? Author { get; private set; }

    public Post(Guid id, string title, string? description, string content, Author author)
    {
        Id = id;
        Title = title;
        Description = description;
        Content = content;
        Author = author ?? throw new ArgumentNullException(nameof(author));
        AuthorId = author.Id;
    }

    public Post(Guid id, string title, string? description, string content, Guid authorId, Author author)
    {
        Id = id;
        Title = title;
        Description = description;
        Content = content;
        AuthorId = authorId;
        Author = author;
    }
}