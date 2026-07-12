using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Dapper;
using Visma.Yuki.Blog.Infrastructure.Repositories.DatabaseEntities;
using Visma.Yuki.Blog.Domain.ValueObjects;

namespace Visma.Yuki.Blog.Infrastructure.Repositories;

public class AuthorRepository(IUnitOfWork uow) : IAuthorPorts
{
    private readonly IUnitOfWork _uow = uow;

    public async Task AddAsync(Author author, CancellationToken cancellationToken = default)
    {
        
        const string sql = @"
            INSERT INTO authors (id, uniquenameidentifier, name, surname) 
            VALUES (@Id, @UniqueNameIdentifier, @Name, @Surname);
        ";

        var connection = _uow.Connection;
        var transaction = _uow.Transaction;

        var dbResults = await connection.ExecuteAsync(
            sql, 
            param: new
            {
                Id = author.Id,
                UniqueNameIdentifier = author.UniqueNameIdentifier.Value,
                Name = author.Name,
                Surname = author.Surname
            },
            transaction: transaction
        );
    }
    
    public async Task<IEnumerable<Author>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id, 
                name AS Name, 
                surname AS Surname, 
                uniquenameidentifier AS UniqueNameIdentifier 
            FROM authors";

        var connection = _uow.Connection;
        var transaction = _uow.Transaction;

        var dbResults = await connection.QueryAsync<AuthorEntity>(
            sql, 
            transaction: transaction
        );

        if(dbResults is not null)
            return dbResults.Select(dto => new Author(
                dto.Id, 
                dto.Name, 
                dto.Surname, 
                dto.UniqueNameIdentifier
            ));
        else
            return [];
    }

    public async Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                id AS Id, 
                name AS Name, 
                surname AS Surname, 
                uniquenameidentifier AS UniqueNameIdentifier 
            FROM authors AS a
            WHERE a.id = @id";

        var connection = _uow.Connection;
        var transaction = _uow.Transaction;

        var dbResults = await connection.QueryFirstOrDefaultAsync<AuthorEntity>(
            sql, 
            param: new {id = id},
            transaction: transaction
        );

        if(dbResults is not null)
            return new(dbResults.Id, dbResults.Name, dbResults.Surname, dbResults.UniqueNameIdentifier);
        else
            return default;
    }

    public async Task<Author?> GetByUniqueNameIdentifierAsync(UniqueNameIdentifier uniqueNameIdentifier, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                id AS Id, 
                name AS Name, 
                surname AS Surname, 
                uniquenameidentifier AS UniqueNameIdentifier 
            FROM authors AS a
            WHERE a.uniquenameidentifier = @UniqueNameIdentifier;";

        var connection = _uow.Connection;
        var transaction = _uow.Transaction;

        var dbResults = await connection.QueryFirstOrDefaultAsync<AuthorEntity>(
            sql, 
            param: new {UniqueNameIdentifier = uniqueNameIdentifier.Value},
            transaction: transaction
        );

        if(dbResults is not null)
            return new(dbResults.Id, dbResults.Name, dbResults.Surname, dbResults.UniqueNameIdentifier);
        else
            return default;
    }
}
