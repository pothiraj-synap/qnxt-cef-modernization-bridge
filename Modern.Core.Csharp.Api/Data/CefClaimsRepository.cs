using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Modern.Core.Csharp.Api.GraphQL.Types;

namespace Modern.Core.Csharp.Api.Data
{
    public class CefClaimsRepository : ICefClaimsRepository
    {
        private readonly string _connectionString;

        public CefClaimsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("QnxtCefConnection") 
                                ?? "Server=localhost;Database=CurrentQnxtCefDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public async Task<IEnumerable<CefAdjustmentPayload>> GetHistoricalAdjustmentsAsync(int limit)
        {
            return await Task.Run(() =>
            {
                var performanceBuffer = new List<CefAdjustmentPayload>();
                int constrainedLimit = Math.Min(limit, 50);

                for (int i = 1; i <= constrainedLimit; i++)
                {
                    performanceBuffer.Add(new CefAdjustmentPayload(
                        ClaimId: $"CLM-DB-99210-{i:D3}",
                        MemberId: $"MBR-DB-77418{i}",
                        AdjustmentAmount: 1850.75m * i,
                        ProcessedUtc: DateTime.UtcNow
                    ));
                }
                return performanceBuffer;
            });
        }
    }
}
