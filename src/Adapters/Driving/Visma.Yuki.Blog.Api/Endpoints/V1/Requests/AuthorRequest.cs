using Visma.Yuki.Blog.Application.Commands.Author;

namespace Visma.Yuki.Blog.Api.Endpoints.V1.Requests;

public class AuthorRequest
{
    public string Name { get; set; }
    public string Surname { get; set; }

    public static explicit operator CreateAuthorCommand(AuthorRequest value)
    {
        return new(value.Name, value.Surname);
    }
}