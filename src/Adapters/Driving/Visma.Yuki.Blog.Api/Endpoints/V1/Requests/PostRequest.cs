using Visma.Yuki.Blog.Application.Commands.Post;

namespace Visma.Yuki.Blog.Api.Endpoints.V1.Requests;

public class PostRequest
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Content { get; set; }
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorSurname { get; set; }

    public static explicit operator CreatePostCommand(PostRequest value)
    {
        return new(value.Title, value.Description, value.Content, value.AuthorId, value.AuthorName, value.AuthorSurname);
    }
}