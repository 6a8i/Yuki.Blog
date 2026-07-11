var builder = DistributedApplication.CreateBuilder(args);

var yukiBlogDatabaseInstance = builder.AddPostgres("postgres-server");
var yukiBlogDatabase = yukiBlogDatabaseInstance.AddDatabase("yuki-blog-database");

var databaseMigration = builder.AddProject<Projects.Visma_Yuki_Blog_Database>("database-manager")
                                .WaitFor(yukiBlogDatabase)
                                .WithReference(yukiBlogDatabase);

builder.AddProject<Projects.Visma_Yuki_Blog_Api>("api")
    .WithReference(yukiBlogDatabase)
    .WaitForStart(yukiBlogDatabase)
    .WithReference(databaseMigration)
    .WaitForCompletion(databaseMigration)
    .WithHttpHealthCheck("/health");



await builder.Build().RunAsync();
