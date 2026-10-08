import React, { useState, useEffect } from 'react';

interface CefRecord {
  claimId: string;
  memberId: string;
  adjustmentAmount: number;
  processedUtc: string;
}

export const CefLiveViewer: React.FC = () => {
  const [records, setRecords] = useState<CefRecord[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchGraphQLData = async () => {
    const queryPayload = {
      query: `
        query {
          pendingAdjustments(limit: 10) {
            claimId
            memberId
            adjustmentAmount
            processedUtc
          }
        }
      `
    };

    try {
      setLoading(true);
      const response = await fetch('/graphql', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(queryPayload),
      });
      
      const result = await response.json();
      
      if (result.errors) {
        setError(result.errors[0].message);
      } else {
        setRecords(result.data.pendingAdjustments);
        setError(null);
      }
    } catch (err) {
      setError("Failed to bridge network data from .NET Core cloud container.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchGraphQLData();
    const dataPoller = setInterval(fetchGraphQLData, 5000);
    return () => clearInterval(dataPoller);
  }, []);

  return (
    <div style={{ padding: '32px', fontFamily: 'Segoe UI, sans-serif', backgroundColor: '#f8fafc', minHeight: '100vh' }}>
      <div style={{ maxWidth: '1200px', margin: '0 auto' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderBottom: '2px solid #e2e8f0', paddingBottom: '16px', marginBottom: '24px' }}>
          <h2 style={{ color: '#0f172a', margin: 0, fontSize: '24px', fontWeight: 700 }}>
            📊 QNXT CEF Enterprise Modernization Gateway
          </h2>
          <span style={{ padding: '6px 12px', backgroundColor: '#dcfce7', color: '#15803d', borderRadius: '9999px', fontSize: '13px', fontWeight: 'bold' }}>
            🟢 Live PostgreSQL Stream Active
          </span>
        </div>

        {error && (
          <div style={{ padding: '16px', backgroundColor: '#fee2e2', color: '#991b1b', borderRadius: '6px', marginBottom: '16px', borderLeft: '4px solid #dc2626' }}>
            <strong>System Error Log:</strong> {error}
          </div>
        )}

        {loading && records.length === 0 ? (
          <div style={{ color: '#64748b', fontSize: '16px' }}>Polling Dapper repositories inside cloud container...</div>
        ) : (
          <div style={{ overflowX: 'auto', borderRadius: '8px', backgroundColor: '#ffffff' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
              <thead>
                <tr style={{ backgroundColor: '#1e293b', color: '#ffffff' }}>
                  <th style={{ padding: '16px', fontWeight: '600' }}>Claim Reference ID</th>
                  <th style={{ padding: '16px', fontWeight: '600' }}>Member Tracking ID</th>
                  <th style={{ padding: '16px', fontWeight: '600', textAlign: 'right' }}>Adjustment Amount</th>
                  <th style={{ padding: '16px', fontWeight: '600' }}>Processed Time (UTC)</th>
                </tr>
              </thead>
              <tbody>
                {records.map((row, idx) => (
                  <tr key={idx} style={{ borderBottom: '1px solid #f1f5f9' }}>
                    <td style={{ padding: '16px', fontWeight: 'bold', color: '#2563eb' }}>{row.claimId}</td>
                    <td style={{ padding: '16px', color: '#334155' }}>{row.memberId}</td>
                    <td style={{ padding: '16px', color: '#16a34a', fontWeight: 'bold', textAlign: 'right' }}>
                      ${row.adjustmentAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}
                    </td>
                    <td style={{ padding: '16px', color: '#64748b', fontSize: '14px' }}>{row.processedUtc}</td>
                  </tr>
                ))}
              </tbody>            
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
