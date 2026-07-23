# Ramboll Smart Attendance Prototype (POC) Setup Guide

This document guides you through setting up the local development environment for testing the Smart Presence and Network Detection prototype.

---

## 1. Directory Structure

```
RambollAttendancePrototype/
├── assets/          - Image assets and logos
├── backend/         - .NET 8 Web API & Presence Agent Source Code
├── database/        - SQL Server schema.sql & seed.sql scripts
├── docs/            - System documentation and guides
├── frontend/        - HTML, CSS, JavaScript Dashboard UI
├── reports/         - Generated CSV/Excel report storage
└── screenshots/     - Progress checkpoints
```

---

## 2. Requirements & Prerequisites

To run this prototype locally on your Windows 10 laptop, ensure the following free software is installed:
1. **.NET 8.0 SDK** (Software Development Kit)
2. **SQL Server Express or Developer Edition** (Local instance)
3. **SQL Server Management Studio (SSMS)**
4. **Visual Studio Code** (or Visual Studio 2022)
5. A web browser (Edge/Chrome/Firefox)

---

## 3. Configuration Parameters

The POC is configured to detect connectivity to your mobile hotspot.
- **SSID Targeted**: `Galaxy S25 Ultra 7A56`
- **Simulated SSO ID**: Matches your active Windows login name (`System.Environment.UserName`).

---

## 4. Next Step
Proceed to **Phase 2: Database Schema & Seeding** to initialize tables in your local SQL Server instance.
