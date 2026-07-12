using Dapper;
using Npgsql;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Infrastructure.Repositories;

public class PostRepository(IUnitOfWork uow) : IPostPorts
{
    private readonly IUnitOfWork _uow = uow;
    public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO posts (id, title, description, content, authorId) 
            VALUES (@Id, @Title, @Description, @Content, @AuthorId);
        ";

        var connection = _uow.Connection;
        var transaction = _uow.Transaction;

        await connection.ExecuteAsync(
            sql, 
            param: new
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                Content = post.Content,
                AuthorId = post.Author.Id
            },
            transaction: transaction
        );
    }
}