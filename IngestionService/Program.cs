using IngestionService;
using Microsoft.Extensions.AI;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddOllamaApiClient("embeddings")
    .AddEmbeddingGenerator()
     .UseOpenTelemetry(configure: c => c.EnableSensitiveData = builder.Environment.IsDevelopment());

bool useAzure = builder.Configuration.GetValue<bool>("useAzure");

if (useAzure)
{
    builder.AddAzureSearchClient("azure-search");
    builder.Services.AddIcmAzureSearchVectorStore();
}
else
{
    var sqliteConnectionString = builder.Configuration.GetConnectionString("vector-store");
    builder.Services.AddSqliteVectorStore(_ => sqliteConnectionString ??
            throw new InvalidOperationException("Connection string 'vector-store' not found."));
}

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
