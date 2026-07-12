using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Api.Endpoints.V1.Responses;

public class AuthorResponse(Guid id, string name, string surname)
{
    public Guid Id { get; private set; } = id;
    public string FullName { get; private set; } = $"{name} {surname}";

    public static explicit operator AuthorResponse(Author value)
    {
        return new(value.Id, value.Name, value.Surname);
    }
}