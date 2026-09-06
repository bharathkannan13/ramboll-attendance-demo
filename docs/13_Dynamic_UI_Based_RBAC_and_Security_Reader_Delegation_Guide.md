# Document 13: Dynamic UI-Based RBAC & Security Reader Delegation Guide

> **Document ID**: TECH-2026-DYNAMIC-RBAC-EXPIRY-001  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Feature**: **Dynamic UI Role Assignment Page, Security Reader Delegation, & 90/365-Day Auto-Expiry**  
> **Target Access Control**: Admin Owner vs Security Reader (Read-Only) vs People Manager

---

## 📌 Executive Summary of RBAC Capability

The Admin Owner can assign temporary read-only or administrative access to colleagues **directly inside the Web Console UI (No C# backend code edits required!)**.

1. **Assign Access in UI Form**: Input corporate email, select role (`Security Reader`), select duration (`90 Days` / `365 Days`).
2. **Security Reader Access**: User can view the entire admin portal, logs, compliance metrics, health status, and telemetry, BUT CANNOT edit, delete, update, or remove configurations/users.
3. **Automatic Expiry**: Once the 90 or 365 days expire, access is revoked automatically.
4. **Extension & Manual Revocation**: Expired users can click **[Request Extension]** in UI; Admin Owner can manually click **[Revoke Access]** at any time.
