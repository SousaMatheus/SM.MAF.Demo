using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

var services = new ServiceCollection();

var ollamaClient = new OllamaApiClient(
    "http://localhost:11434/",
    "mxbai-embed-large");
services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(ollamaClient);

using var serviceProvider = services.BuildServiceProvider();
var generator = serviceProvider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

var vector = await generator.GenerateVectorAsync("Texto para gerar embedding");
Console.WriteLine($"Dimensões: {vector.Length}");
