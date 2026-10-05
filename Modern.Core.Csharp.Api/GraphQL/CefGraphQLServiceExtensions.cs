using Microsoft.Extensions.DependencyInjection;
using Modern.Core.Csharp.Api.GraphQL.Queries;
using Modern.Core.Csharp.Api.GraphQL.Types;
using Modern.Core.Csharp.Api.GraphQL.Mutations;
namespace Modern.Core.Csharp.Api.GraphQL
{
    /// <summary>
    /// Senior Clean Code Architecture Pattern: Encapsulates all GraphQL 
    /// bootstrapping container logic away from the primary Program.cs file.
    /// </summary>
    public static class CefGraphQLServiceExtensions
    {
        public static IServiceCollection AddCefGraphQLInfrastructure(this IServiceCollection services)
        {
            // Bind and package the complete Hot Chocolate server ecosystem cleanly
            services
                .AddGraphQLServer()
                .AddQueryType<CefAdjustmentQuery>() // Attaches Read query operations
                .AddMutationType<CefAdjustmentMutation>()// <--- Register the mutation layer here
                .AddType<CefAdjustmentType>();      // Attaches explicit schema contracts
                
            return services;
        }
    }
}
