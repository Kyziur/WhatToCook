namespace WhatToCook.Web;

public static class ApiHttpClientRegistration
{
    private const string DefaultApiBaseUrl = "https+http://api";

    public static IServiceCollection AddApiHttpClient<TContract, TImplementation>(
        this IServiceCollection services
    )
        where TContract : class
        where TImplementation : class, TContract
    {
        services.AddHttpClient<TContract, TImplementation>(ConfigureApiClient);
        return services;
    }

    public static IServiceCollection AddNamedApiHttpClient(
        this IServiceCollection services,
        string clientName
    )
    {
        services.AddHttpClient(clientName, ConfigureApiClient);
        return services;
    }

    private static void ConfigureApiClient(IServiceProvider serviceProvider, HttpClient client)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var baseUrl = configuration["Api:BaseUrl"] ?? DefaultApiBaseUrl;
        client.BaseAddress = new Uri(baseUrl);
    }
}
