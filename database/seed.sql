-- ==========================================
-- Ramboll Smart Attendance Seed Script
-- Populates default test user identities
-- ==========================================

USE RambollSmartAttendance;
GO

-- Clear existing data (in reverse order of dependencies)
DELETE FROM dbo.Attendance_Daily_Summary;
DELETE FROM dbo.Telemetry_Log;
DELETE FROM dbo.Device_Master;
DELETE FROM dbo.Employee_Master;
GO

-- Reset identities
DBCC CHECKIDENT ('dbo.Employee_Master', RESEED, 0);
DBCC CHECKIDENT ('dbo.Device_Master', RESEED, 0);
DBCC CHECKIDENT ('dbo.Attendance_Daily_Summary', RESEED, 0);
GO

-- 1. Insert Mock Employees
-- The first employee represents the active developer machine's Windows user identity
-- to ensure the SSO pipeline resolves instantly during test executions.
INSERT INTO dbo.Employee_Master (SSO_Identity, FullName, Email, Department, Is_Active)
VALUES 
('bharath_kn\bharath', 'Bharath K N', 'bharath.kn@ramboll.com', 'Solutions Architecture', 1),
('RAMBOLL\johndoe', 'John Doe', 'john.doe@ramboll.com', 'DevOps & IT Operations', 1),
('RAMBOLL\janesmith', 'Jane Smith', 'jane.smith@ramboll.com', 'Human Resources', 1);

-- 2. Insert Registered Devices
INSERT INTO dbo.Device_Master (MAC_Address, Hostname, Employee_ID)
VALUES 
('AA-BB-CC-DD-EE-FF', 'BHARATH-LAPTOP1', 1),
('11-22-33-44-55-66', 'BHARATH-LAPTOP2', 1),
('00-11-22-33-44-55', 'JOHN-WORKSTATION', 2);

-- 3. Seed Past Attendance Summary for Testing (Mocking Yesterday's Active Time)
-- Calculates 8.5 hours active for Bharath K N
INSERT INTO dbo.Attendance_Daily_Summary (Employee_ID, Work_Date, First_Seen, Last_Seen, Active_Hours, Status)
VALUES
(1, CAST(DATEADD(day, -1, GETDATE()) AS DATE), 
 DATEADD(hour, 9, CAST(CAST(DATEADD(day, -1, GETDATE()) AS DATE) AS DATETIME)), -- 09:00 AM
 DATEADD(hour, 18, CAST(CAST(DATEADD(day, -1, GETDATE()) AS DATE) AS DATETIME)), -- 06:00 PM
 8.50, 
 'PRESENT');
GO

PRINT 'Database seed populated successfully.';
GO
