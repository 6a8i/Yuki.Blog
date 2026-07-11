using Carter;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Visma.Yuki.Blog.Application.Ports.Driving;
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

        group.MapGet("/", async ([FromServices] IAuthorUseCase authorUseCase) =>
        {
            Result<IEnumerable<Author>> result = await authorUseCase.GetAuthorsAsync();
            
            if (result.IsFailed)
            {
                return Results.BadRequest(result.Errors[0].Message); // Usando Results para evitar o outro erro de tipo
            }

            return Results.Ok(result.Value);
        });

        group.MapPost("/", ([FromServices] IAuthorUseCase authorUseCase) => {
            
            return TypedResults.Json("Author created", statusCode: StatusCodes.Status201Created, contentType: "application/json");
        });
    }
}