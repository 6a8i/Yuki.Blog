var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Visma_Yuki_Blog_Api>("api")
    .WithHttpHealthCheck("/health");



builder.Build().Run();
