using Carter;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Visma.Yuki.Blog.Api.Endpoints.V1.Requests;
using Visma.Yuki.Blog.Api.Endpoints.V1.Responses;
using Visma.Yuki.Blog.Application.Commands.Author;
using Visma.Yuki.Blog.Application.Ports.Driving;
using Visma.Yuki.Blog.Application.Queries.Author;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Api.Endpoints.V1;

public class AuthorEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var authorsApi = app.NewVersionedApi("Authors");

        var group = authorsApi.MapGroup("/api/v{version:apiVersion}/authors")
                              .HasApiVersion(1.0)
                              .WithTags("Authors");

        group.MapGet("/", async ([FromServices] IAuthorQueryHandler authorQueryHandler, CancellationToken cancellationToken = default) =>
        {
            Result<IEnumerable<Author>> result = await authorQueryHandler.HandleAsync(new GetAllAuthorsQuery(), cancellationToken);
            
            if (result.IsFailed)
            {
                return Results.BadRequest(result.Errors[0].Message);
            }

            if(!result.Value.Any())
                return Results.NoContent();
            else
                return Results.Ok(result.Value.Select(a => new AuthorResponse(a.Id, a.Name, a.Surname)));
        }).Produces<IEnumerable<AuthorResponse>>();

        group.MapGet("/{id}", async ([FromRoute] Guid id, [FromServices] IAuthorQueryHandler authorQueryHandler, CancellationToken cancellationToken = default) =>
        {
            Result<Author?> result = await authorQueryHandler.HandleAsync(new GetAuthorByIdQuery(id), cancellationToken);

            if (result.IsFailed)
            {
                return Results.BadRequest(result.Errors);
            }

            if (result.Value is null)
                return Results.NotFound();

            return Results.Ok((AuthorResponse)result.Value);
        }).Produces<AuthorResponse>();

        group.MapPost("/",async ([FromBody] AuthorRequest request, 
                                [FromServices] IAuthorCommandHandler authorCommandHandler, 
                                CancellationToken cancellationToken = default) => 
        {
            Result<Author> result = await authorCommandHandler.HandleAsync((CreateAuthorCommand) request, cancellationToken);

            if (result.IsFailed)
            {
                return Results.BadRequest(result.Errors);
            }

            AuthorResponse response = (AuthorResponse)result.Value;

            return Results.Created($"/api/v1/authors/{response.Id}", response);
        }).Produces<AuthorResponse>();
    }
}