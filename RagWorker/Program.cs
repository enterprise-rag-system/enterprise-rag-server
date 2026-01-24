using Microsoft.EntityFrameworkCore;
using RagService.Services;
using RagWorker.Consumer;
using RagWorker.Infrastructure.Ingestion;
using RagWorker.Infrastructure.Messaging;
using RagWorker.Infrastructure.Persistence;
using RagWorker.Infrastructure.Vector;
using RagWorker.Interfaces.AI;
using RagWorker.Interfaces.Ingestion;
using RagWorker.Interfaces.Messaging;
using RagWorker.Interfaces.Rag;
using RagWorker.Interfaces.Vector;
using RagWorker.Providers.Azure;
using RagWorker.Providers.Common;
using RagWorker.Providers.Ollama;
using RagWorker.Workers;

var builder = Host.CreateApplicationBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();


services.AddDbContext<RagDbContext>(options =>
{
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection"),
        o => o.UseVector());
});

services.AddScoped<IVectorStore, PgVectorStore>();
services.AddScoped<IRagQueryProcessor, RagQueryProcessor>();
services.AddScoped<IDocumentIngestionService, DocumentIngestionService>();
services.AddScoped<ITextExtractor, FileTextExtractor>();
services.AddScoped<IChunkingService, ChunkingService>();

services.AddSingleton<IMessageBus, RabbitMqMessageBus>();

services.Configure<AiProviderOptions>(
    configuration.GetSection("AI"));

services.Configure<AzureOpenAiOptions>(
    configuration.GetSection("AzureOpenAI"));

services.Configure<OllamaOptions>(
    configuration.GetSection("Ollama"));

var aiProvider =
    configuration.GetValue<string>("AI:Provider");

if (aiProvider == ProviderConstants.AzureOpenAi)
{
    services.AddScoped<IEmbeddingProvider, AzureEmbeddingProvider>();
    services.AddScoped<IChatCompletionProvider, AzureChatCompletionProvider>();
}
else if (aiProvider == ProviderConstants.Ollama)
{
    services.AddHttpClient<IEmbeddingProvider, OllamaEmbeddingProvider>();
    services.AddHttpClient<IChatCompletionProvider, OllamaChatCompletionProvider>();
}
else
{
    throw new InvalidOperationException(
        $"Unsupported AI provider: {aiProvider}");
}

services.AddHostedService<DocumentUploadedConsumer>();
services.AddHostedService<ChatMessageCreatedConsumer>();

var app = builder.Build();

//ApplyMigration();

app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<RagDbContext>();
        _db.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS ers");
        
        try
        {
            _db.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS vector");
        }
        catch (Exception ex)
        {
            // ⚠️ DO NOT crash app in prod
            // Log and continue (especially for managed DBs)
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "pgvector extension could not be created. Ensure it exists.");
        }

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}