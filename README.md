# RAMBoll Smart Attendance & Network Presence System

An enterprise-grade, secure, and production-ready Attendance Tracking & Network Presence System built with **Next.js 15**, **TypeScript**, **Tailwind CSS**, **Prisma ORM**, and **PostgreSQL (Neon)**.

---

## 🌟 Key Features

- **Cloud-Native & Serverless**: 100% hosted on Vercel with PostgreSQL database on Neon. No local server or hardware required.
- **Enterprise Security & Role-Based Access Control (RBAC)**:
  - Administrator access protected by JWT authentication stored in HTTP-Only, Secure cookies.
  - Strict middleware guards protecting `/admin/*` and API routes (`/api/sessions`, `/api/attendance`).
  - Employees have **zero access** to administrative functions, records, endpoints, or hidden routes.
- **Real-Time Attendance Monitoring**:
  - Live Admin Dashboard updating automatically.
  - Displays: **Username**, **Date**, **First Seen**, **Last Seen**, **Total Hours**, **IP Address**, and **Online/Offline Status**.
  - Server-Sent Events (SSE) stream live attendance changes directly to the dashboard.
- **Employee Session Tracking**:
  - Unique session link generation (e.g. `https://your-app.vercel.app/join/ABCD1234`).
  - Minimalistic employee interface showing **only** the welcome status:
    > **Welcome to RAMBoll Attendance Portal**  
    > Welcome, `<Username>`  
    > ✅ Attendance Started Successfully  
    > You are connected to the authorized network. Please keep this page open while working.
- **Heartbeat & Disconnect Engine**:
  - Sends heartbeats every 30 seconds.
  - Auto-calculates active hours (`Last Seen - First Seen`).
  - Automatically marks users **Offline** if no heartbeat is received within 90 seconds.
- **Excel Export**:
  - Generates downloadable `.xlsx` spreadsheets containing detailed daily attendance logs.

---

## 🏗️ Architecture Stack

| Layer | Technology |
|---|---|
| **Framework** | Next.js 15 (App Router) |
| **Language** | TypeScript |
| **Styling** | Tailwind CSS (Glassmorphic Dark Theme) |
| **Database** | PostgreSQL (Neon / Managed Cloud) |
| **ORM** | Prisma ORM |
| **Authentication** | JWT (`jose`) + Bcryptjs + HTTP-Only Cookies |
| **Validation** | Zod |
| **Export** | ExcelJS |
| **Hosting** | Vercel |

---

## 🚀 Quick Setup & Local Development

### 1. Environment Configuration
Copy the `.env.example` file to `.env`:

```bash
cp .env.example .env
```

Set your configuration parameters:

```env
# Managed PostgreSQL Connection String (Neon / Supabase)
DATABASE_URL="postgresql://user:password@ep-example.neon.tech/ramboll_attendance?sslmode=require"

# JWT Secret & Admin Credentials
JWT_SECRET="your-random-256-bit-secret-key"
ADMIN_USERNAME="admin"
ADMIN_PASSWORD="RambollAdmin2026"

# Network Settings
AUTHORIZED_SSID="Galaxy S25 Ultra 7A56"
HEARTBEAT_TIMEOUT_MS=90000
```

### 2. Database Migration & Seeding
Push the Prisma schema to your PostgreSQL database and seed the default administrator account:

```bash
# Push schema to database
npx prisma db push

# Seed initial admin account
npx prisma db seed
```

### 3. Run Development Server
Start the local server:

```bash
npm run dev
```

Open `http://localhost:3000` in your browser.

---

## ☁️ Deployment on Vercel

1. Push your repository to GitHub.
2. Go to [Vercel Dashboard](https://vercel.com/dashboard) -> **Add New Project**.
3. Import your GitHub repository (`ramboll-attendance-demo`).
4. In **Environment Variables**, add:
   - `DATABASE_URL` (From your Neon PostgreSQL dashboard)
   - `JWT_SECRET`
   - `ADMIN_USERNAME`
   - `ADMIN_PASSWORD`
   - `AUTHORIZED_SSID`
5. Click **Deploy**. Vercel will automatically build and publish the app!

---

## 🔒 Security Summary

- **401 Unauthorized**: Returned for unauthenticated requests.
- **403 Forbidden**: Returned for employees attempting to access admin APIs.
- **Sanitized SQL**: Handled via Prisma parameterized queries.
- **Security Headers**: Standard security headers (X-Frame-Options, CSP, X-XSS-Protection) configured in `next.config.ts`.
