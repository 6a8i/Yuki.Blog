namespace Visma.Yuki.Blog.Api.Endpoints.V1.Responses;

public class AuthorResponse(Guid id, string name, string surname)
{
    public Guid Id { get; private set; } = id;
    public string FullName { get; private set; } = $"{name} {surname}";
}