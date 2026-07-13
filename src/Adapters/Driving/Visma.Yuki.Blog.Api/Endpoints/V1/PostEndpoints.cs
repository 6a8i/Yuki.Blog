using Carter;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Visma.Yuki.Blog.Api.Endpoints.V1.Requests;
using Visma.Yuki.Blog.Api.Endpoints.V1.Responses;
using Visma.Yuki.Blog.Application.Commands.Post;
using Visma.Yuki.Blog.Application.Ports.Driving;
using Visma.Yuki.Blog.Application.Queries.Post;
using Visma.Yuki.Blog.Domain.Entities;

namespace Visma.Yuki.Blog.Api.Endpoints.V1;

public class PostEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var postApi = app.NewVersionedApi("Posts");

        var group = postApi.MapGroup("/api/v{version:apiVersion}/posts")
                              .HasApiVersion(1.0)
                              .WithTags("Posts");


        group.MapGet("/", async ([FromServices] IPostQueryHandler postQueryHandler, [FromQuery] bool includeAuthor = false, CancellationToken cancellationToken = default) =>
        {
            Result<IEnumerable<Post>> result = await postQueryHandler.HandleAsync(new GetAllPostsQuery(includeAuthor), cancellationToken);

            if (result.IsFailed)
                return Results.BadRequest(result.Errors);

            if(!result.Value.Any())
                return Results.NoContent();

            var items = result.Value.Select(p =>
            {
                var response = (PostResponse)p;
                response.Links = [new Link("self", "GET", $"/api/v1/posts/{response.Id}")];
                return response;
            }).ToList();

            var collection = new CollectionResponse<PostResponse>
            {
                Items = items,
                Links =
                [
                    new Link("self", "GET", "/api/v1/posts/"),
                    new Link("create", "POST", "/api/v1/posts/")
                ]
            };

            return Results.Ok(collection);
        }).Produces<CollectionResponse<PostResponse>>().ProducesProblem(400);

        group.MapGet("/{id}", async ([FromRoute] Guid id, [FromServices] IPostQueryHandler postQueryHandler, [FromQuery] bool includeAuthor = false, CancellationToken cancellationToken = default) =>
        {
            Result<Post?> result = await postQueryHandler.HandleAsync(new GetPostByIdQuery(id, includeAuthor), cancellationToken);

            if (result.IsFailed)
                return Results.BadRequest(result.Errors);

            if(result.Value is null)
                return Results.NotFound();

            var response = (PostResponse) result.Value;
            response.Links =
            [
                new Link("self", "GET", $"/api/v1/posts/{response.Id}"),
                new Link("collection", "GET", "/api/v1/posts/")
            ];

            return Results.Ok(response);

        }).Produces<PostResponse>().ProducesProblem(400).ProducesProblem(404);

        group.MapPost("/", async ([FromBody] PostRequest request, [FromServices] IPostCommandHandler postCommandHandler, CancellationToken cancellationToken = default) =>
        {
            Result<Post> result = await postCommandHandler.HandleAsync((CreatePostCommand) request, cancellationToken);

            if (result.IsFailed)
                return Results.BadRequest(result.Errors);

            PostResponse response = (PostResponse) result.Value;
            response.Links =
            [
                new Link("self", "GET", $"/api/v1/posts/{response.Id}"),
                new Link("collection", "GET", "/api/v1/posts/")
            ];

            return Results.Created($"/api/v1/posts/{response.Id}", response);
        }).Produces<PostResponse>(201).ProducesProblem(400);
    }
}