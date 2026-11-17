-- ============================================================================
-- 1. Create Database (Run this block separately if connected to 'postgres' db)
-- ============================================================================
CREATE DATABASE qnxt_cef_modernization;
\c qnxt_cef_modernization;

-- ============================================================================
-- 2. Member Table (With ACTIVE, INACTIVE, INCOMPLETE constraints)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.member (
    member_id VARCHAR(30) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    date_of_birth DATE NOT NULL,
    gender CHAR(1) NOT NULL,
    member_status VARCHAR(20) NOT NULL DEFAULT 'INCOMPLETE',
    created_utc TIMESTAMP NOT NULL DEFAULT clock_timestamp(),
    CONSTRAINT pk_member PRIMARY KEY (member_id),
    CONSTRAINT chk_member_status CHECK (
        member_status IN ('ACTIVE', 'INACTIVE', 'INCOMPLETE')
    )
);

-- ============================================================================
-- 3. Claim Table (With full QNXT CEF status constraints & OPEN default)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.claim (
    claim_id VARCHAR(50) NOT NULL,
    member_id VARCHAR(30) NOT NULL,
    provider_id VARCHAR(30) NOT NULL,
    total_billed_amount NUMERIC(18, 4) NOT NULL DEFAULT 0.0000,
    claim_status VARCHAR(15) NOT NULL DEFAULT 'OPEN',
    received_utc TIMESTAMP NOT NULL DEFAULT clock_timestamp(),
    CONSTRAINT pk_claim PRIMARY KEY (claim_id),
    CONSTRAINT fk_claim_member FOREIGN KEY (member_id) 
        REFERENCES public.member(member_id) ON DELETE RESTRICT,
    CONSTRAINT chk_claim_status CHECK (
        claim_status IN (
            'OPEN', 'PAY', 'WAITPAY', 'LOCK', 'HOLD', 
            'DENY', 'DENIED', 'PEND', 'ADJUDICATED', 'PAID', 'VOID'
        )
    )
);

-- ============================================================================
-- 4. Claim Detail Line-Item Table
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.claimdetail (
    claim_detail_id BIGSERIAL NOT NULL,
    claim_id VARCHAR(50) NOT NULL,
    line_number INT NOT NULL,
    procedure_code VARCHAR(10) NOT NULL,
    revenue_code VARCHAR(10) NULL,
    charge_amount NUMERIC(18, 4) NOT NULL DEFAULT 0.0000,
    allowed_amount NUMERIC(18, 4) NOT NULL DEFAULT 0.0000,
    CONSTRAINT pk_claimdetail PRIMARY KEY (claim_detail_id),
    CONSTRAINT fk_claimdetail_claim FOREIGN KEY (claim_id) 
        REFERENCES public.claim(claim_id) ON DELETE CASCADE,
    CONSTRAINT uq_claim_line UNIQUE (claim_id, line_number)
);

-- ============================================================================
-- 5. Outbox Audit Log Header & Detail Tables
-- ============================================================================
CREATE TABLE IF NOT EXISTS public.logheader (
    log_header_id BIGSERIAL NOT NULL,
    transaction_guid UUID NOT NULL DEFAULT gen_random_uuid(),
    source_channel VARCHAR(30) NOT NULL DEFAULT 'ANGULAR_UI',
    execution_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    started_utc TIMESTAMP NOT NULL DEFAULT clock_timestamp(),
    completed_utc TIMESTAMP NULL,
    CONSTRAINT pk_logheader PRIMARY KEY (log_header_id),
    CONSTRAINT uq_log_guid UNIQUE (transaction_guid)
);

CREATE TABLE IF NOT EXISTS public.logdetail (
    log_detail_id BIGSERIAL NOT NULL,
    log_header_id BIGINT NOT NULL,
    claim_id VARCHAR(50) NULL,
    log_level VARCHAR(10) NOT NULL DEFAULT 'INFO',
    message_payload TEXT NOT NULL,
    timestamp_utc TIMESTAMP NOT NULL DEFAULT clock_timestamp(),
    CONSTRAINT pk_logdetail PRIMARY KEY (log_detail_id),
    CONSTRAINT fk_logdetail_header FOREIGN KEY (log_header_id) 
        REFERENCES public.logheader(log_header_id) ON DELETE CASCADE
);

-- ============================================================================
-- 6. MVCC Performance Indexes
-- ============================================================================
CREATE INDEX IF NOT EXISTS ix_claim_member ON public.claim (member_id);
CREATE INDEX IF NOT EXISTS ix_claim_received ON public.claim (received_utc DESC);
CREATE INDEX IF NOT EXISTS ix_claimdetail_search ON public.claimdetail (claim_id);
CREATE INDEX IF NOT EXISTS ix_logheader_status ON public.logheader (execution_status) 
    WHERE execution_status = 'PENDING';

-- ============================================================================
-- 7. Seed Initial Test Records (So Foreign Keys work immediately on Insert)
-- ============================================================================
INSERT INTO public.member (member_id, first_name, last_name, date_of_birth, gender, member_status)
VALUES 
    ('MBR-99810-TN', 'Pothiraj', 'R', '1982-10-21', 'M', 'ACTIVE'),
    ('MBR-10022-CEF', 'Anand', 'Kumar', '1985-05-14', 'M', 'INCOMPLETE')
ON CONFLICT (member_id) DO NOTHING;

INSERT INTO public.claim (claim_id, member_id, provider_id, total_billed_amount, claim_status)
VALUES 
    ('CLM-PG-2026-A01', 'MBR-99810-TN', 'PROV-COIMB-01', 4500.5000, 'OPEN'),
    ('CLM-PG-2026-A02', 'MBR-10022-CEF', 'PROV-MADURAI-02', 1250.7500, 'WAITPAY')
ON CONFLICT (claim_id) DO NOTHING;
