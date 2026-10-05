using System.Collections.Generic;
using System.Threading.Tasks;
using Modern.Core.Csharp.Api.GraphQL.Types;

namespace Modern.Core.Csharp.Api.Data
{
    public interface ICefRepository
    {
        Task<IEnumerable<CefAdjustmentPayload>> GetPendingAdjustmentsFromLedgerAsync(int limit);
    }
}
