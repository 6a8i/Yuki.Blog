namespace Visma.Yuki.Blog.Infrastructure.Repositories.DatabaseEntities;

public record AuthorEntity(Guid Id, string Name, string Surname, string UniqueNameIdentifier);