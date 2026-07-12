namespace Visma.Yuki.Blog.Api.Endpoints.V1.Responses;

public class CollectionResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public List<Link> Links { get; set; } = [];
}
