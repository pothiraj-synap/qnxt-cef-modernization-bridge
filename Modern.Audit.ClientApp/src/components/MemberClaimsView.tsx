import React, { useEffect, useState, useCallback } from 'react';

interface Member {
  memberId: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  memberStatus: 'ACTIVE' | 'INACTIVE' | 'INCOMPLETE';
}

interface Claim {
  claimId: string;
  memberId: string;
  providerId: string;
  totalBilledAmount: number;
  claimStatus: string;
  receivedUtc: string;
}

export const MemberClaimsView: React.FC = () => {
  const [members, setMembers] = useState<Member[]>([]);
  const [selectedMember, setSelectedMember] = useState<Member | null>(null);
  const [claims, setClaims] = useState<Claim[]>([]);
  const [loadingMembers, setLoadingMembers] = useState(true);
  const [loadingClaims, setLoadingClaims] = useState(false);

  const API_BASE = 'http://localhost:5141/api';

 

  // 2. Fetch linked claims when a member is selected
  const handleSelectMember = useCallback((member: Member) => {
    setSelectedMember(member);
    setLoadingClaims(true);

    fetch(`${API_BASE}/claims?memberId=${encodeURIComponent(member.memberId)}`)
      .then((res) => res.json())
      .then((data: Claim[]) => {
        // Filter client-side as fallback if endpoint returns all claims
        const memberClaims = data.filter((c) => c.memberId === member.memberId);
        setClaims(memberClaims);
        setLoadingClaims(false);
      })
      .catch(() => {
        setClaims([]);
        setLoadingClaims(false);
      });
  }, [API_BASE]);
 // 1. Load all members on mount
  useEffect(() => {
    fetch(`${API_BASE}/members`)
      .then((res) => res.json())
      .then((data: Member[]) => {
        setMembers(data);
        setLoadingMembers(false);
        if (data.length > 0) {
          handleSelectMember(data[0]);
        }
      })
      .catch(() => setLoadingMembers(false));
  }, [API_BASE, handleSelectMember]);
  const getStatusBadgeColor = (status: string) => {
    switch (status) {
      case 'ACTIVE':
        return '#198754'; // Green
      case 'INACTIVE':
        return '#dc3545'; // Red
      default:
        return '#fd7e14'; // Orange for INCOMPLETE
    }
  };

  return (
    <div style={{ maxWidth: '1000px', margin: '2rem auto', fontFamily: 'sans-serif' }}>
      <h2>QNXT Member Directory</h2>

      {loadingMembers ? (
        <p>Loading members from PostgreSQL...</p>
      ) : (
        <table style={{ width: '100%', borderCollapse: 'collapse', marginBottom: '2rem' }}>
          <thead>
            <tr style={{ backgroundColor: '#f4f6f8', textAlign: 'left', borderBottom: '2px solid #dee2e6' }}>
              <th style={{ padding: '10px' }}>Member ID</th>
              <th style={{ padding: '10px' }}>Full Name</th>
              <th style={{ padding: '10px' }}>DOB</th>
              <th style={{ padding: '10px' }}>Gender</th>
              <th style={{ padding: '10px' }}>Status</th>
              <th style={{ padding: '10px' }}>Action</th>
            </tr>
          </thead>
          <tbody>
            {members.map((m) => {
              const isSelected = selectedMember?.memberId === m.memberId;
              return (
                <tr
                  key={m.memberId}
                  onClick={() => handleSelectMember(m)}
                  style={{
                    cursor: 'pointer',
                    backgroundColor: isSelected ? '#e8f4fd' : '#fff',
                    borderBottom: '1px solid #dee2e6',
                  }}
                >
                  <td style={{ padding: '10px', fontWeight: 'bold' }}>{m.memberId}</td>
                  <td style={{ padding: '10px' }}>{m.firstName} {m.lastName}</td>
                  <td style={{ padding: '10px' }}>{m.dateOfBirth?.split('T')[0]}</td>
                  <td style={{ padding: '10px' }}>{m.gender}</td>
                  <td style={{ padding: '10px' }}>
                    <span
                      style={{
                        backgroundColor: getStatusBadgeColor(m.memberStatus),
                        color: '#fff',
                        padding: '3px 8px',
                        borderRadius: '4px',
                        fontSize: '0.85rem',
                      }}
                    >
                      {m.memberStatus}
                    </span>
                  </td>
                  <td style={{ padding: '10px' }}>
                    <button type="button" style={{ padding: '4px 10px', cursor: 'pointer' }}>
                      {isSelected ? 'Viewing Claims' : 'View Claims'}
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}

      {/* Connected Claims Panel */}
      {selectedMember && (
        <div style={{ padding: '1.5rem', border: '1px solid #dee2e6', borderRadius: '6px', backgroundColor: '#fafafa' }}>
          <h3>
            Linked Claims for {selectedMember.firstName} {selectedMember.lastName} ({selectedMember.memberId})
          </h3>

          {loadingClaims ? (
            <p>Loading linked claims...</p>
          ) : claims.length === 0 ? (
            <p style={{ color: '#6c757d' }}>No claims recorded for this member yet.</p>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', backgroundColor: '#fff' }}>
              <thead>
                <tr style={{ backgroundColor: '#e9ecef', textAlign: 'left' }}>
                  <th style={{ padding: '8px' }}>Claim ID</th>
                  <th style={{ padding: '8px' }}>Provider ID</th>
                  <th style={{ padding: '8px' }}>Billed Amount</th>
                  <th style={{ padding: '8px' }}>Claim Status</th>
                  <th style={{ padding: '8px' }}>Received UTC</th>
                </tr>
              </thead>
              <tbody>
                {claims.map((c) => (
                  <tr key={c.claimId} style={{ borderBottom: '1px solid #dee2e6' }}>
                    <td style={{ padding: '8px', fontWeight: 'bold' }}>{c.claimId}</td>
                    <td style={{ padding: '8px' }}>{c.providerId}</td>
                    <td style={{ padding: '8px' }}>${c.totalBilledAmount.toFixed(2)}</td>
                    <td style={{ padding: '8px' }}>{c.claimStatus}</td>
                    <td style={{ padding: '8px' }}>{new Date(c.receivedUtc).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  );
};
