using Carter;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Visma.Yuki.Blog.Api.Endpoints.V1.Requests;
using Visma.Yuki.Blog.Application.Commands.Post;
using Visma.Yuki.Blog.Application.Ports.Driving;

namespace Visma.Yuki.Blog.Api.Endpoints.V1;

public class PostEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var postApi = app.NewVersionedApi("Posts");

        var group = postApi.MapGroup("/api/v{version:apiVersion}/posts")
                              .HasApiVersion(1.0)
                              .WithTags("Posts");

        group.MapPost("/", async ([FromBody] PostRequest request, [FromServices] IPostUseCase postUseCase, CancellationToken cancellationToken = default) =>
        {
            Result<Guid> result = await postUseCase.CreatePostAsync((CreatePostCommand) request,cancellationToken);

            if (result.IsFailed)
                return Results.BadRequest(result.Errors);

            return Results.Created($"/api/v1/posts/{result.Value}", result.Value);
        }).Produces<Guid>();
    }
}