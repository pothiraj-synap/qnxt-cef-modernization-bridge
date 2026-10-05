using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Modern.Core.Csharp.Api.GraphQL.Types;

namespace Modern.Core.Csharp.Api.Data
{
    public class CefRepository : ICefRepository
    {
        private readonly string _connectionString;

        public CefRepository(IConfiguration configuration)
        {
            // Pulls the secure encrypted database gateway string from configuration maps
            _connectionString = configuration.GetConnectionString("QnxtCefDatabase") 
                                ?? "Server=tcp:mock-qnxt-cef.database.windows.net;Database=CurrentQnxtCefDB;";
        }

        public async Task<IEnumerable<CefAdjustmentPayload>> GetPendingAdjustmentsFromLedgerAsync(int limit)
        {
            // Utilizing a clean parameterized raw SQL execution path with Top-N filters 
            // targeting our optimized non-clustered filtered indexes directly.
            const string sqlQuery = @"
                SELECT TOP (@RowLimit) 
                    ClaimId, 
                    MemberId, 
                    AdjustmentAmount, 
                    LastModifiedUtc AS ProcessedUtc
                FROM dbo.CefEncounterLedger
                ORDER BY LedgerSequenceID DESC;";

            using (IDbConnection databaseConnection = new SqlConnection(_connectionString))
            {
                // Dapper maps the raw database record attributes instantly to your high-performance C# Records in memory
                return await databaseConnection.QueryAsync<CefAdjustmentPayload>(
                    sqlQuery, 
                    new { RowLimit = limit }
                );
            }
        }
    }
}
