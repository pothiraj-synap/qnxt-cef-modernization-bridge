using System;
using HotChocolate.Types;

namespace Modern.Core.Csharp.Api.GraphQL.Types
{
    // High-Performance Data Layout Record for Inbound Queries
    public record CefAdjustmentPayload(string ClaimId, string MemberId, decimal AdjustmentAmount, DateTime ProcessedUtc);

    public class CefAdjustmentType : ObjectType<CefAdjustmentPayload>
    {
        protected override void Configure(IObjectTypeDescriptor<CefAdjustmentPayload> descriptor)
        {
            // Explicitly defining clean, strong-typed schemas for downstream screen panels
            descriptor.Field(f => f.ClaimId).Type<NonNullType<StringType>>().Description("The unique identifier for the targeted claim record.");
            descriptor.Field(f => f.MemberId).Type<NonNullType<StringType>>().Description("The patient or member tracking index.");
            descriptor.Field(f => f.AdjustmentAmount).Type<NonNullType<DecimalType>>().Description("The validated CEF financial adjustment allocation.");
            descriptor.Field(f => f.ProcessedUtc).Type<NonNullType<DateTimeType>>().Description("Timestamp verifying back-end transaction completion.");
        }
    }
}
