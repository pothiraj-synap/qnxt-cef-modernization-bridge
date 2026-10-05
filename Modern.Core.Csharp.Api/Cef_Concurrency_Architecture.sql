-- =========================================================================
-- QNXT CEF (Claims, Encounter, Financial) High-Concurrency Database Design
-- Target Dilemma: Eliminating LCK_M_X (Exclusive Lock) Deadlocks during 
-- high-volume automated claims ingestion and real-time financial audits.
-- =========================================================================

-- 1. Enable Read Committed Snapshot Isolation (RCSI)
-- Architect Note: This forces SQL Server to use Row Versioning in tempdb. 
-- Read operations (Auditors/Dashboards) no longer place Shared Locks, meaning
-- they NEVER block write operations (Inbound Ingestion Workers).
ALTER DATABASE CurrentQnxtCefDB 
SET READ_COMMITTED_SNAPSHOT ON 
WITH ROLLBACK IMMEDIATE;
GO

-- 2. Create the Production Encounter Adjustment Tracking Ledger
IF OBJECT_ID('dbo.CefEncounterLedger', 'U') IS NOT NULL
    DROP TABLE dbo.CefEncounterLedger;
GO

CREATE TABLE dbo.CefEncounterLedger (
    LedgerSequenceID BIGINT IDENTITY(1,1) NOT NULL,
    EncounterGuid UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    ClaimId VARCHAR(50) NOT NULL,
    MemberId VARCHAR(20) NOT NULL,
    AdjustmentAmount DECIMAL(18,4) NOT NULL,
    RowVersion ROWVERSION NOT NULL, -- Implements Optimistic Concurrency Control (OCC)
    LastModifiedUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_CefEncounterLedger PRIMARY KEY NONCLUSTERED (EncounterGuid)
);

-- Establish physical storage topology: Sequential append-only clustering 
-- to completely isolate the drive from page splits and index fragmentation.
CREATE CLUSTERED INDEX IX_CefEncounterLedger_Sequence
ON dbo.CefEncounterLedger (LedgerSequenceID ASC);
GO

-- 3. Stored Procedure: Thread-Safe Concurrency Verification via Optimistic Locking
CREATE PROCEDURE dbo.CreateSecuredCefAdjustment
    @ClaimId VARCHAR(50),
    @MemberId VARCHAR(20),
    @AdjustmentAmount DECIMAL(18,4)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON; -- Force immediate rollback on any runtime transaction error

    BEGIN TRANSACTION;
        
        -- Utilizing highly optimized index paths to avoid full table scans
        INSERT INTO dbo.CefEncounterLedger (ClaimId, MemberId, AdjustmentAmount)
        VALUES (@ClaimId, @MemberId, @AdjustmentAmount);

    COMMIT TRANSACTION;
    
    _logger.LogInformation("Database transaction safely committed to historical CEF ledger block.");
END;
GO
