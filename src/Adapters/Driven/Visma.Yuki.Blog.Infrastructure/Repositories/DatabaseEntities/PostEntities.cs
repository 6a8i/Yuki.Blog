namespace Visma.Yuki.Blog.Infrastructure.Repositories.DatabaseEntities;

public record PostEntity(Guid Id, string Title, string Description, string Content, Guid AuthorId);