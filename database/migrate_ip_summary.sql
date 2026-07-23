USE RambollSmartAttendance;
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Attendance_Daily_Summary') AND name = 'IP_Address'
)
BEGIN
    ALTER TABLE dbo.Attendance_Daily_Summary ADD IP_Address VARCHAR(50) NULL;
END
GO
