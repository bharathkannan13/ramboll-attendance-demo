# Document 9: Pilot Testing & Pre-Production Staging Guide (5 to 10 Test Devices)

> **Document ID**: STAGING-2026-PILOT-RUNBOOK-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Horizon**: Pre-Production Staging & Pilot Validation (5–10 Devices)  
> **Target Users**: Pilot Group Staff & Managers (`bharathkannan1154@gmail.com`)

---

1. **Create Entra Pilot Security Group**: Create group `SG-Attendance-Pilot-Users` in `entra.microsoft.com` & add 5–10 test laptops/users.
2. **Enable Pilot Mode**: In `appsettings.json` set `"PilotModeOnly": true` & paste `PilotGroupId`.
3. **Verify 4 Checkpoints**:
   - Check 1: First Seen / Last Seen timestamp precision.
   - Check 2: Office Wi-Fi (`10.100.0.0/16`) vs Remote WFH classification.
   - Check 3: Automated Monday email dispatch + attached `Weekly_Attendance_Report.xlsx`.
   - Check 4: Hyperlink redirection button to `https://ramboll-attendance-portal.azurewebsites.net/Manager`.
4. **Full Production Promotion**: Set `"PilotModeOnly": false` in Azure Portal to expand to all employees!
