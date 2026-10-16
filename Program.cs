using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using SM.MAF.Demo.Infra.Data;
using SM.MAF.Demo.Models;
using SM.MAF.Demo.ViewModels;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, opt => opt.UseVector());
});

builder.Services.AddTransient(x => new OllamaApiClient(
    uriString: "http://localhost:11434",
    defaultModel: "mxbai-embed-large"
));

var app = builder.Build();

app.MapPost("api/v1/seed", async (AppDbContext context, OllamaApiClient ollama) =>
{
    var products = await context.Products.AsNoTracking().ToListAsync();

    foreach(var product in products)
    {
        var service = ollama.AsTextEmbeddingGenerationService();
        var embedding = await service.GenerateEmbeddingAsync(product.Category);

        var recomendation = new Recomendation
        {
            Title = product.Title,
            Category = product.Category,
            Embedding = new Vector(embedding)
        };

        await context.Recomendations.AddAsync(recomendation);
        await context.SaveChangesAsync();
    }

    return Results.Ok(new { message = "OK" });
});

app.MapPost("api/v1/product", async (AppDbContext context, OllamaApiClient ollama, ProductViewModel product) =>
{
    if(string.IsNullOrWhiteSpace(product.Title) || string.IsNullOrWhiteSpace(product.Category))
        return Results.BadRequest();

    var service = ollama.AsTextEmbeddingGenerationService();
    var embedding = await service.GenerateEmbeddingAsync(product.Category);

    var recomendation = new Recomendation
    {
        Title = product.Title,
        Category = product.Category,
        Embedding = new Vector(embedding)
    };

    await context.Recomendations.AddAsync(recomendation);
    await context.SaveChangesAsync();

    return Results.Created();
});

app.MapPost("api/v1/recomendation", async (AppDbContext context, OllamaApiClient ollama, QuestionViewModel model) =>
{
    if(string.IsNullOrWhiteSpace(model.Prompt))
        return Results.BadRequest();

    var service = ollama.AsTextEmbeddingGenerationService();
    var embedding = await service.GenerateEmbeddingAsync(model.Prompt);

    var recomendations = await context.Recomendations
        .OrderBy(x => x.Embedding.CosineDistance(new Vector(embedding.ToArray())))
        .Take(3)
        .Select(x => new { x.Title, x.Category })
        .ToListAsync();

    return Results.Ok(recomendations);
});

app.Run();
