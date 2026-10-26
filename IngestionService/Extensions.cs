
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;


namespace IngestionService
{
    public static class Extensions
    {
        public static IServiceCollection AddIcmAzureSearchVectorStore(this IServiceCollection services)
        {
            services.AddSingleton<VectorStore>(sp =>
            {
                var indexClient = sp.GetRequiredService<SearchIndexClient>();
                return new AzureAISearchVectorStore(indexClient, new AzureAISearchVectorStoreOptions
                {
                    EmbeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator>()
                });
            });

            return services;
        }
    }
}
