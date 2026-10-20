using IcmChatAPI;
using IcmChatAPI.Models;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

string sqliteConnectionString = builder.Configuration.GetConnectionString("vector-store") 
                ?? throw new InvalidOperationException("Connection string 'vector-store' not found.");
builder.Services.AddSqliteCollection<Guid, IcmChunk>("data-icm-chunks", sqliteConnectionString);

//redis
builder.AddRedisDistributedCache("cache");


builder.AddOllamaApiClient("chat")
    .AddChatClient()
    .UseFunctionInvocation()
    .UseDistributedCache()
    .UseOpenTelemetry(configure: c => c.EnableSensitiveData = builder.Environment.IsDevelopment());

builder.AddOllamaApiClient("embeddings")
    .AddEmbeddingGenerator()
    .UseOpenTelemetry(configure: c => c.EnableSensitiveData = builder.Environment.IsDevelopment());

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddOllamaResilienceHandlers();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
