using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qnxt.Cef.Legacy; // Binds directly to your core VB.NET proxy wrapper

namespace Modern.Core.Csharp.Api
{
    public record OutboxMessage(string MessageId, string ClaimId, decimal Amount, DateTime CreatedAt);

    public class CefOutboxProcessor : BackgroundService
    {
        private readonly ILogger<CefOutboxProcessor> _logger;
        private readonly FinancialComponentProxy _legacyCefProxy;
        private static readonly ConcurrentQueue<OutboxMessage> OutboxQueue = new();

        public CefOutboxProcessor(ILogger<CefOutboxProcessor> logger)
        {
            _logger = logger;
            _legacyCefProxy = new FinancialComponentProxy();
        }

        public static void EnqueueAdjustment(string claimId, decimal amount)
        {
            var message = new OutboxMessage(Guid.NewGuid().ToString("N"), claimId, amount, DateTime.UtcNow);
            OutboxQueue.Enqueue(message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 QNXT CEF Outbox Background Worker started successfully.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (OutboxQueue.TryDequeue(out var pendingMessage))
                {
                    try
                    {
                        _logger.LogInformation("Processing Outbox ID: {Id} for Claim: {ClaimId}", pendingMessage.MessageId, pendingMessage.ClaimId);

                        // Running the synchronous legacy VB.NET ASC module inside an unblocked worker thread
                        string rawResult = await Task.Run(() => 
                            _legacyCefProxy.ProcessEncounterAdjustment(pendingMessage.ClaimId, pendingMessage.Amount), 
                            stoppingToken
                        );

                        _logger.LogInformation("Successfully integrated CEF Adjustment. Legacy Payload: {Result}", rawResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Critical Exception inside legacy interop boundary for Claim: {ClaimId}", pendingMessage.ClaimId);
                    }
                }
                else
                {
                    await Task.Delay(2500, stoppingToken); // Sleep briefly when queue is idle
                }
            }
        }
    }
}