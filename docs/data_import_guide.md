# Employee & Manager Data Import Guide

This guide explains how to prepare, format, and load your organization's employee and manager records into the database to support 19,000+ users.

---

## 1. Database Relational Structure
In the production schema, employee records are tied to their manager profiles to allow hierarchical reporting (e.g., manager dashboards). 

### Updated Database Schema Overview
* **`Employee_Master`**: Contains fields for `Employee_ID`, `SSO_Identity` (Windows/Azure AD User Principal Name), `FullName`, `Email`, `Department`, `Manager_ID`, and `Is_Active`.
* **Manager Relationship**: The `Manager_ID` column in the `Employee_Master` table is a foreign key referencing the `Employee_ID` of their respective manager (a self-referential relationship).

---

## 2. CSV Data Template for HR
Your HR or IT department should export employee data from your HR system of record (e.g., Workday) in the following CSV format.

### Template: `employees_import.csv`
```csv
SSO_Identity,FullName,Email,Department,Manager_SSO,Is_Active
bharath_kn\bharath,Bharath K N,bharath.kn@ramboll.com,Solutions Architecture,,1
RAMBOLL\johndoe,John Doe,john.doe@ramboll.com,DevOps & IT,bharath_kn\bharath,1
RAMBOLL\janesmith,Jane Smith,jane.smith@ramboll.com,Human Resources,bharath_kn\bharath,1
```
* *Note: The `Manager_SSO` column is used during import to resolve and link the correct `Manager_ID` foreign key.*

---

## 3. SQL Data Sync & Import Script
This script can be executed in SQL Server Management Studio (SSMS) or scheduled as a weekly SQL Agent Job to import and sync data from the CSV file.

```sql
-- ===================================================
-- SQL Server Staged CSV Import & Sync Procedure
-- ===================================================
USE RambollSmartAttendance;
GO

-- 1. Create temporary staging table
IF OBJECT_ID('tempdb..#Staging_Employees') IS NOT NULL 
    DROP TABLE #Staging_Employees;

CREATE TABLE #Staging_Employees (
    SSO_Identity VARCHAR(150),
    FullName VARCHAR(150),
    Email VARCHAR(150),
    Department VARCHAR(100),
    Manager_SSO VARCHAR(150),
    Is_Active BIT
);

-- 2. Bulk insert data from CSV file (Path to be updated by IT)
BULK INSERT #Staging_Employees
FROM 'C:\Imports\employees_import.csv'
WITH (
    FIRSTROW = 2,
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    TABLOCK
);

-- 3. Synchronize Employee Master records (Insert new, Update existing)
MERGE INTO dbo.Employee_Master AS target
USING #Staging_Employees AS source
ON target.SSO_Identity = source.SSO_Identity
WHEN MATCHED THEN
    UPDATE SET 
        target.FullName = source.FullName,
        target.Email = source.Email,
        target.Department = source.Department,
        target.Is_Active = source.Is_Active
WHEN NOT MATCHED THEN
    INSERT (SSO_Identity, FullName, Email, Department, Is_Active)
    VALUES (source.SSO_Identity, source.FullName, source.Email, source.Department, source.Is_Active);

-- 4. Update Manager ID mappings based on Manager_SSO matching
UPDATE emp
SET emp.Manager_ID = mgr.Employee_ID
FROM dbo.Employee_Master emp
INNER JOIN #Staging_Employees stage ON emp.SSO_Identity = stage.SSO_Identity
INNER JOIN dbo.Employee_Master mgr ON stage.Manager_SSO = mgr.SSO_Identity
WHERE stage.Manager_SSO IS NOT NULL;
GO
```

---

## 4. Integration with Microsoft Entra ID (Azure AD) Sync
For a fully automated setup, instead of exporting CSV files manually:
1. **Azure Active Directory Connect**: Set up Entra ID SCIM provisioning or a scheduled Azure Logic App.
2. **Logic App flow**:
   * Runs every morning at 01:00.
   * Calls Microsoft Graph API `/users` to list all Ramboll employees, their email addresses, department names, and manager IDs.
   * Feeds the JSON response directly into the Web API sync endpoint: `POST /api/employee/sync`.
