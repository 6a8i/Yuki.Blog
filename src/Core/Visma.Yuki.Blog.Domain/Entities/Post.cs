namespace Visma.Yuki.Blog.Domain.Entities;

public class Post
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }

    public string? Description { get; private set; }

    public string Content { get; private set; }

    public Author Author { get; private set; }

    public Post(Guid id, string title, string? description, string content, Author author)
    {
        Id = id;
        Title = title;
        Description = description;
        Content = content;
        Author = author;
    }
}