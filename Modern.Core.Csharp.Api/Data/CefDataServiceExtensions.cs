using Microsoft.Extensions.DependencyInjection;

namespace Modern.Core.Csharp.Api.Data
{
    public static class CefDataServiceExtensions
    {
        public static IServiceCollection AddCefDataInfrastructure(this IServiceCollection services)
        {
            // Registering the claims repository cleanly inside the Data ecosystem
            services.AddTransient<ICefClaimsRepository, CefClaimsRepository>();
            
            return services;
        }
    }
}
