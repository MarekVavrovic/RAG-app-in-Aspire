var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama").WithDataVolume();
var chatModel = ollama.AddModel("chat", "llama3.2");
var embeddings = ollama.AddModel("embeddings", "all-minilm");


var vectorStore = builder.AddSqlite("vector-store").WithSqliteWeb();

var cache = builder.AddRedis("cache").WithDbGate(); // Add Redis with DbGate for management

builder.AddProject<Projects.IcmChatAPI>("icmchatapi")
    .WithReference(chatModel)
    .WithReference(vectorStore)
    .WithReference(embeddings)
    .WithReference(cache)
    .WaitFor(chatModel)
    .WaitFor(vectorStore)
    .WaitFor(embeddings)
    .WaitFor(cache);

builder.AddProject<Projects.IngestionService>("ingestionservice")
    .WithReference(embeddings)
    .WithReference(vectorStore)
    .WaitFor(embeddings)
    .WaitFor(vectorStore);

builder.Build().Run();
