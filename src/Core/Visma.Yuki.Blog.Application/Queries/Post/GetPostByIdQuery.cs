namespace Visma.Yuki.Blog.Application.Queries.Post;

public record GetPostByIdQuery(Guid Id, bool IncludeAuthor = false);
