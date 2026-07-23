USE RambollSmartAttendance;
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Telemetry_Log') AND name = 'IP_Address'
)
BEGIN
    ALTER TABLE dbo.Telemetry_Log ADD IP_Address VARCHAR(50) NULL;
END
GO
