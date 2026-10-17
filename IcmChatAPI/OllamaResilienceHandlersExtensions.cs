namespace IcmChatAPI
{
    public static class OllamaResilienceHandlersExtensions
    {
        public static IServiceCollection AddOllamaResilienceHandlers(this IServiceCollection services)
        {
            services.ConfigureHttpClientDefaults(httpClientBuilder =>
            {
#pragma warning disable EXTEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                httpClientBuilder.RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                httpClientBuilder.AddStandardResilienceHandler(config =>
                {
                    config.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5); // Timeout for each attempt
                    config.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(10);
                    config.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
                });
            });

            return services;
        }
    }
}
