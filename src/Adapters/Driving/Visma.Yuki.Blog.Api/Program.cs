using Asp.Versioning;
using Carter;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.Services.AddApiVersioning(options => options.ApiVersionReader = new UrlSegmentApiVersionReader());
builder.Services.AddSharedServices();
builder.Services.AddCarter();

builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
{
    options.InputFormatters.Add(new Microsoft.AspNetCore.Mvc.Formatters.XmlSerializerInputFormatter(options));
    options.OutputFormatters.Add(new Microsoft.AspNetCore.Mvc.Formatters.XmlSerializerOutputFormatter());
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference("/", options =>
{
    options
        .WithTitle("Yuki Blog API")
        .ForceDarkMode()
        .ShowOperationId()
        .HideDarkModeToggle()
        .WithTheme(ScalarTheme.DeepSpace);
});

app.UseHttpsRedirection();

app.MapCarter();

app.MapDefaultEndpoints();

await app.RunAsync();

