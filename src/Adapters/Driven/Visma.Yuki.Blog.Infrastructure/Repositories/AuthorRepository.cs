using Visma.Yuki.Blog.Domain.Entities;
using Visma.Yuki.Blog.Application.Ports.Driven;
using Dapper;
using Visma.Yuki.Blog.Infrastructure.Repositories.DatabaseEntities;

namespace Visma.Yuki.Blog.Infrastructure.Repositories;

public class AuthorRepository(IUnitOfWork uow) : IAuthorPorts
{
    private readonly IUnitOfWork _uow = uow;

    public Task AddAsync(Author Author, CancellationToken cancellationToken = default)
    {
        
        throw new NotImplementedException();
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
}
