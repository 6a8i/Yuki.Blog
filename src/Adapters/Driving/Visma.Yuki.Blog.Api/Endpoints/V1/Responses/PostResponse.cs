using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Api.Endpoints.V1.Responses;

public class PostResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Content { get; set; }
    public Guid AuthorId { get; set; }
    public AuthorResponse? AuthorInfo { get; set; }
    public List<Link> Links { get; set; } = [];

    public static explicit operator PostResponse(Post value)
    {
        return new() 
        {
            Id = value.Id, 
            Title = value.Title, 
            Description = value.Description, 
            Content = value.Content, 
            AuthorId = value.AuthorId, 
            AuthorInfo = value.Author is null ? null : (AuthorResponse)value.Author
        };
    }
}