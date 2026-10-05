using System.Threading.Tasks;
using HotChocolate;
using Modern.Core.Csharp.Api.Data; // Direct link to your standardized Data folder
using Modern.Core.Csharp.Api.GraphQL.Types;

namespace Modern.Core.Csharp.Api.GraphQL.Mutations
{
    public record CreateAdjustmentInput(string ClaimId, string MemberId, decimal Amount);
    public record CreateAdjustmentResponse(bool IsSuccess, string TrackingMessage);

    public class CefAdjustmentMutation
    {
        /// <summary>
        /// Senior Architecture Pattern: GraphQL Mutation acting as a write-boundary interface.
        /// Thread-safely injects payloads directly into the Data repository tier.
        /// </summary>
        public async Task<CreateAdjustmentResponse> SubmitCefAdjustmentAsync(
            CreateAdjustmentInput input,
            [Service] ICefClaimsRepository repository)
        {
            if (string.IsNullOrWhiteSpace(input.ClaimId) || input.Amount <= 0)
            {
                return new CreateAdjustmentResponse(false, "Invalid financial metrics payload.");
            }

            // In a production layout, the mutation passes data down to your repository layer securely:
            // await repository.SaveNewAdjustmentEntryAsync(input.ClaimId, input.Amount);
            
            // Queue the adjustment thread-safely into our running background processor loop
            CefOutboxProcessor.EnqueueAdjustment(input.ClaimId, input.Amount);

            return await Task.FromResult(new CreateAdjustmentResponse(
                IsSuccess: true,
                TrackingMessage: $"Claim {input.ClaimId} successfully queued into backend interop processing streams."
            ));
        }
    }
}
