# Master Pre-Pilot Deployment Playbook (Dynamic Single Manager Input & Cross-Border Architecture)

> **Document ID**: PLAYBOOK-2026-CROSSBORDER-OPTIMIZED  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Special Architecture**: **Global Managers (Denmark/Germany) Managing India Staff**  
> **Telemetry Rule**: **Filter telemetry at Employee level (India Hubs Only); Allow Global Managers to receive emails & access SSO dashboards.**

---

## 🌍 Global Manager (Denmark / Germany) Cross-Border Architecture

1. **Ingestion Filter**: Applied at Employee (Subordinate) level (`officeLocation IN ('Chennai', 'Noida', 'Hyderabad', 'Gurugram', 'Bangalore', 'Mumbai', 'Pune', 'Delhi')`). Danish/German employees' laptop telemetry is **100% EXCLUDED** for EU GDPR compliance.
2. **Email Delivery**: Danish/German managers (`lars.jensen@ramboll.dk`) **DO receive weekly automated emails** containing `Weekly_Attendance_Report.xlsx` for their **India-based subordinates only**.
3. **SSO Dashboard Access**: Danish/German managers sign in via Entra ID SSO (`@ramboll.dk`) and inspect their **India team's attendance heatmaps and timelines** in the Manager Console.
