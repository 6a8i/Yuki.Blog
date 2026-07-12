using Dapper;
using Npgsql;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Infrastructure.Repositories.DatabaseEntities;

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
                AuthorId = post.Author!.Id
            },
            transaction: transaction
        );
    }

    public async Task<IEnumerable<Post>> GetAllAsync(bool includeAuthor, CancellationToken cancellationToken)
    {
        string sql;

        var connection = _uow.Connection;
        var transaction = _uow.Transaction;

        if(includeAuthor)
        {
            sql = @"
            SELECT 
                p.id, p.title, p.description, p.content, p.authorId,
                a.id, a.name, a.surname, a.uniqueNameIdentifier
            FROM posts AS p
            INNER JOIN authors AS a ON a.id = p.authorId;
            ";

            IEnumerable<Post> posts = await connection.QueryAsync<PostEntity, AuthorEntity, Post>(
                sql, 
                map: (postDto, authorDto) =>
                {
                    var author = new Author(authorDto.Id, authorDto.Name, authorDto.Surname, authorDto.UniqueNameIdentifier);
                    return new Post(postDto.Id, postDto.Title, postDto.Description, postDto.Content, postDto.AuthorId, author);
                },
                transaction: transaction
            );

            return posts;
        }
        else
        {
            sql = @"
            SELECT 
                p.id, p.title, p.description, p.content, p.authorId
            FROM posts AS p
            ";

            var dbResults = await connection.QueryAsync<PostEntity>(
                sql, 
                transaction: transaction
            );

            return dbResults.Select(p => new Post(p.Id, p.Title, p.Description, p.Content, p.AuthorId, author: null!));
        }
    }
}