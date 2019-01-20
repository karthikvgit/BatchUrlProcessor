using Common.Utilities.Handlers;
using Common.Utilities.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Net.Http;

namespace Common.Utilities.Extensions
{
    public static class BaseExtenstions
    {
        public static IServiceCollection AddBase(this IServiceCollection services)
        {
            services.RegisterDependencies();

            services.AddHttpClientFactory();

            return services;
        }

        public static IServiceCollection AddLoneStarDependencies(this IServiceCollection services)
        {
            services.RegisterDependencies();

            return services;
        }

        #region UseLoneStarBase

        public static IApplicationBuilder UseBase(this IApplicationBuilder app)
        {
            app.UseCustomExceptionHandlerMiddleware(); 

            return app;
        }


        #endregion UseLoneStarBase

        public static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IHttpClientHelper, HttpClientHelper>();

            return services;
        }

        public static IServiceCollection AddHttpClientFactory(this IServiceCollection services)
        {
            services.AddTransient<CorrelationIdHeaderHandler>(); 

            services.AddHttpClient();

            services.AddHttpClient("ServiceCall", c =>
            {
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls | System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12
            })
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5)))
            .AddHttpMessageHandler<CorrelationIdHeaderHandler>();

            services.AddHttpClient("ServiceCallWithSsl", c =>
            {
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                SslProtocols = System.Security.Authentication.SslProtocols.Ssl2 | System.Security.Authentication.SslProtocols.Ssl3
            })
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5)))
            .AddHttpMessageHandler<CorrelationIdHeaderHandler>();

            return services;
        }

    }
}