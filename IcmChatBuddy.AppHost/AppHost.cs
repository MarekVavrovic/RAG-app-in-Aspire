using Microsoft.Extensions.AI;

var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama").WithDataVolume();
var chatModel = ollama.AddModel("chat", "llama3.2");
var embeddings = ollama.AddModel("embeddings", "all-minilm");


var vectorStore = builder.AddSqlite("vector-store").WithSqliteWeb();

builder.AddProject<Projects.IcmChatAPI>("icmchatapi")
    .WithReference(chatModel)
    .WaitFor(chatModel);

builder.AddProject<Projects.IngestionService>("ingestionservice")
    .WithReference(embeddings)
    .WithReference(vectorStore)
    .WaitFor(embeddings)
    .WaitFor(vectorStore);

builder.Build().Run();
