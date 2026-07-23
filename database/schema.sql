-- ==========================================
-- Ramboll Smart Attendance Prototype Schema
-- Enterprise-grade POC relational schema
-- Designed to run on SQL Server Express / Developer
-- ==========================================

-- Create database if it does not exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'RambollSmartAttendance')
BEGIN
    CREATE DATABASE RambollSmartAttendance;
END
GO

USE RambollSmartAttendance;
GO

-- 1. EMPLOYEE MASTER TABLE
-- Stores primary employee profiles and corporate identities.
IF OBJECT_ID('dbo.Employee_Master', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employee_Master (
        Employee_ID INT IDENTITY(1,1) PRIMARY KEY,
        SSO_Identity VARCHAR(150) UNIQUE NOT NULL, -- e.g., 'bharath_kn\bharath' or domain login
        FullName VARCHAR(150) NOT NULL,
        Email VARCHAR(150) NOT NULL,
        Department VARCHAR(100) NOT NULL,
        Is_Active BIT DEFAULT 1 NOT NULL,
        Created_At DATETIME DEFAULT GETDATE() NOT NULL
    );

    -- Indexing for performance at scale (19,000+ users lookup by SSO login)
    CREATE UNIQUE INDEX IX_Employee_SSO ON dbo.Employee_Master(SSO_Identity);
END
GO

-- 2. DEVICE MASTER TABLE
-- Tracks devices authorized or used by employees.
IF OBJECT_ID('dbo.Device_Master', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Device_Master (
        Device_ID INT IDENTITY(1,1) PRIMARY KEY,
        MAC_Address VARCHAR(50) UNIQUE NOT NULL,
        Hostname VARCHAR(100) NOT NULL,
        Employee_ID INT NOT NULL,
        Last_Seen DATETIME DEFAULT GETDATE() NOT NULL,
        Created_At DATETIME DEFAULT GETDATE() NOT NULL,
        CONSTRAINT FK_Device_Employee FOREIGN KEY (Employee_ID) REFERENCES dbo.Employee_Master(Employee_ID) ON DELETE CASCADE
    );

    CREATE INDEX IX_Device_Employee ON dbo.Device_Master(Employee_ID);
END
GO

-- 3. TELEMETRY LOG TABLE
-- High-throughput time-series logging for presence heartbeats & power events.
IF OBJECT_ID('dbo.Telemetry_Log', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Telemetry_Log (
        Telemetry_ID BIGINT IDENTITY(1,1) PRIMARY KEY,
        Employee_ID INT NOT NULL,
        MAC_Address VARCHAR(50) NOT NULL,
        Hostname VARCHAR(100) NOT NULL,
        Event_Type VARCHAR(50) NOT NULL, -- 'WAKE', 'SLEEP', 'LOCK', 'UNLOCK', 'HEARTBEAT', 'DISCONNECT'
        Timestamp DATETIME NOT NULL,
        SSID VARCHAR(100) NOT NULL,      -- SSID connected to (e.g. Galaxy S25 Ultra 7A56)
        Created_At DATETIME DEFAULT GETDATE() NOT NULL,
        CONSTRAINT FK_Telemetry_Employee FOREIGN KEY (Employee_ID) REFERENCES dbo.Employee_Master(Employee_ID) ON DELETE CASCADE
    );

    -- Composite index to optimize hourly/daily aggregation queries
    CREATE INDEX IX_Telemetry_Aggregate ON dbo.Telemetry_Log(Employee_ID, Timestamp);
END
GO

-- 4. ATTENDANCE DAILY SUMMARY TABLE
-- Stores pre-calculated, aggregated work shifts for dashboards & rapid reporting.
IF OBJECT_ID('dbo.Attendance_Daily_Summary', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Attendance_Daily_Summary (
        Summary_ID INT IDENTITY(1,1) PRIMARY KEY,
        Employee_ID INT NOT NULL,
        Work_Date DATE NOT NULL,
        First_Seen DATETIME NULL,
        Last_Seen DATETIME NULL,
        Active_Hours DECIMAL(5, 2) DEFAULT 0.00 NOT NULL,
        Status VARCHAR(50) DEFAULT 'ABSENT' NOT NULL, -- 'PRESENT', 'ABSENT', 'INCOMPLETE'
        Last_Updated DATETIME DEFAULT GETDATE() NOT NULL,
        CONSTRAINT FK_Summary_Employee FOREIGN KEY (Employee_ID) REFERENCES dbo.Employee_Master(Employee_ID) ON DELETE CASCADE,
        CONSTRAINT UQ_Employee_Date UNIQUE (Employee_ID, Work_Date)
    );

    CREATE INDEX IX_Summary_Employee_Date ON dbo.Attendance_Daily_Summary(Employee_ID, Work_Date);
END
GO

-- 5. ATTENDANCE AUDIT TABLE
-- Tracks system overrides or manual HR updates for cybersecurity compliance.
IF OBJECT_ID('dbo.Attendance_Audit', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Attendance_Audit (
        Audit_ID INT IDENTITY(1,1) PRIMARY KEY,
        Employee_ID INT NOT NULL,
        Action VARCHAR(100) NOT NULL,       -- e.g., 'MANUAL_OVERRIDE', 'SETTINGS_CHANGE'
        Details VARCHAR(MAX) NOT NULL,
        Updated_By VARCHAR(150) NOT NULL,   -- HR manager login
        Timestamp DATETIME DEFAULT GETDATE() NOT NULL,
        CONSTRAINT FK_Audit_Employee FOREIGN KEY (Employee_ID) REFERENCES dbo.Employee_Master(Employee_ID)
    );
END
GO
