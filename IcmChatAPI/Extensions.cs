using Azure.Search.Documents.Indexes;
using IcmChatAPI.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;

namespace IcmChatAPI
{
    public static class Extensions
    {

        public static IServiceCollection AddIcmAzureSearchCollection(
            this IServiceCollection services, string name)
        {
            services.AddSingleton<VectorStoreCollection<Guid, IcmChunk>>(sp =>
            {
                var indexClient = sp.GetRequiredService<SearchIndexClient>();

                return new AzureAISearchCollection<Guid, IcmChunk>(
                    indexClient,
                    name,
                    new AzureAISearchCollectionOptions
                    {
                        EmbeddingGenerator =
                            sp.GetRequiredService<IEmbeddingGenerator>()
                    });
            });

            return services;
        }

    }
}
