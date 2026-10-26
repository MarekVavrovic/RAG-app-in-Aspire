using Microsoft.Extensions.VectorData;

var builder = DistributedApplication.CreateBuilder(args);

bool useAzure= true;
string vectorFunction = useAzure ? DistanceFunction.CosineSimilarity : DistanceFunction.CosineDistance;

var cache = builder.AddRedis("cache").WithDbGate(); // Add Redis with DbGate 

var ollama = builder.AddOllama("ollama").WithDataVolume();

var embeddings = ollama.AddModel("embeddings", "all-minilm");

if (useAzure)
{    
    var existingAzureSearch = builder.AddParameter("existingAzSearch");
    var existingRg = builder.AddParameter("existingIcmBuddyRg");
    var azureSearch = builder.AddAzureSearch("azure-search").AsExisting(existingAzureSearch, existingRg);

    var existingFoundry = builder.AddParameter("existingFoundry");

    var foundry = builder.AddAzureAIFoundry("foundry").AsExisting(existingFoundry, existingRg);

    builder.AddProject<Projects.IcmChatAPI>("icmchatapi")
    .WithReference(foundry)
    .WithReference(embeddings)
    .WithReference(cache)
    .WithReference(azureSearch)
    .WithEnvironment("useAzure", useAzure.ToString())
    .WaitFor(foundry)
    .WaitFor(embeddings)
    .WaitFor(cache)
    .WaitFor(azureSearch);


    builder.AddProject<Projects.IngestionService>("ingestionservice")
        .WithReference(embeddings)
        .WithReference(azureSearch)
        .WithEnvironment("useAzure", useAzure.ToString())
        .WithEnvironment("vectorFunction", vectorFunction)
        .WaitFor(embeddings)
        .WaitFor(azureSearch);
}
else
{
    var vectorStore = builder.AddSqlite("vector-store").WithSqliteWeb();
    var chatModel = ollama.AddModel("chat", "llama3.2");

    builder.AddProject<Projects.IcmChatAPI>("icmchatapi")
    .WithReference(chatModel)
    .WithReference(vectorStore)
    .WithReference(embeddings)
    .WithReference(cache)
    .WithEnvironment("useAzure", useAzure.ToString())
    .WaitFor(chatModel)
    .WaitFor(vectorStore)
    .WaitFor(embeddings)
    .WaitFor(cache);


    builder.AddProject<Projects.IngestionService>("ingestionservice")
        .WithReference(embeddings)
        .WithReference(vectorStore)
        .WithEnvironment("useAzure", useAzure.ToString())
        .WithEnvironment("vectorFunction", vectorFunction)
        .WaitFor(embeddings)
        .WaitFor(vectorStore);  
}



builder.Build().Run();
