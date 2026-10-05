using System;
using System.Collections.Generic;
using Modern.Core.Csharp.Api.GraphQL.Types;
using System.Threading.Tasks;
using HotChocolate;
using Modern.Core.Csharp.Api.Data;
namespace Modern.Core.Csharp.Api.GraphQL.Queries
{
    public class CefAdjustmentQuery
    {
        /// <summary>
        /// Senior Architectural Design: Resolves active pending financial adjustments.
        /// Corresponds to: query { getPendingAdjustments(limit: 5) { claimId adjustmentAmount } }
        /// </summary>
        public IEnumerable<CefAdjustmentPayload> GetPendingAdjustments(int limit)
        {
            var activeDataStream = new List<CefAdjustmentPayload>();
            
            // Constraining loop counts to safe production baselines
            int safetyLimit = Math.Min(limit, 50);

            for (int i = 1; i <= safetyLimit; i++)
            {
                activeDataStream.Add(new CefAdjustmentPayload(
                    ClaimId: $"CLM-99210-CEF0{i}",
                    MemberId: $"MBR-77418{i}",
                    AdjustmentAmount: 1250.50m * i,
                    ProcessedUtc: DateTime.UtcNow
                ));
            }

            return activeDataStream;
        }
         public async Task<IEnumerable<CefAdjustmentPayload>> GetPendingAdjustmentsAsync(
            int limit,
            [Service] ICefRepository cefRepository)
        {
            return await cefRepository.GetPendingAdjustmentsFromLedgerAsync(limit);
        }
        public async Task<IEnumerable<CefAdjustmentPayload>> GetClaimsPendingAdjustments(
            int limit,
            [Service] ICefClaimsRepository repository)
        {
            return await repository.GetHistoricalAdjustmentsAsync(limit);
        }
    }
}
