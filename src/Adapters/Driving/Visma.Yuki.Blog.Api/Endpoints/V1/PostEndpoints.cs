using Carter;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Visma.Yuki.Blog.Api.Endpoints.V1.Requests;
using Visma.Yuki.Blog.Api.Endpoints.V1.Responses;
using Visma.Yuki.Blog.Application.Commands.Post;
using Visma.Yuki.Blog.Application.Ports.Driving;
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


        group.MapGet("/", async ([FromServices] IPostUseCase postUseCase, [FromQuery] bool includeAuthor = false, CancellationToken cancellationToken = default) =>
        {
            Result<IEnumerable<Post>> result = await postUseCase.GetAllAsync(includeAuthor, cancellationToken);

            if (result.IsFailed)
                return Results.BadRequest(result.Errors);

            if(!result.Value.Any())
                return Results.NoContent();

            return Results.Ok(result.Value.Select(p => new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Content = p.Content,
                AuthorId = p.AuthorId,
                AuthorInfo = p.Author is null ? null : (AuthorResponse)p.Author
            }));
        }).Produces<IEnumerable<PostResponse>>();

        group.MapPost("/", async ([FromBody] PostRequest request, [FromServices] IPostUseCase postUseCase, CancellationToken cancellationToken = default) =>
        {
            Result<Guid> result = await postUseCase.CreatePostAsync((CreatePostCommand) request,cancellationToken);

            if (result.IsFailed)
                return Results.BadRequest(result.Errors);

            return Results.Created($"/api/v1/posts/{result.Value}", result.Value);
        }).Produces<Guid>();
    }
}