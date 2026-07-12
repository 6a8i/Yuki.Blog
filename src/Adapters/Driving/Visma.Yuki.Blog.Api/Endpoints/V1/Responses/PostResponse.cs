namespace Visma.Yuki.Blog.Api.Endpoints.V1.Responses;

public class PostResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Content { get; set; }
    public Guid AuthorId { get; set; }
    public AuthorResponse? AuthorInfo { get; set; }
}