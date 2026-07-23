# Vercel Deployment Guide (Frontend Cloud Hosting)

This guide walks you through deploying the static frontend portal to **Vercel** so you can share it with your friends via a public web link.

---

## 1. Prerequisites
1. A free **GitHub** account (https://github.com)
2. A free **Vercel** account (https://vercel.com)
3. Your local git workspace pushed to GitHub.

---

## 2. Deploying to Vercel (Step-by-Step)

### Step 1: Create a GitHub Repository
1. Log in to GitHub and create a new repository (e.g. `ramboll-attendance-demo`).
2. Push your local project folder to this GitHub repository.

### Step 2: Import Project to Vercel
1. Log in to **Vercel** using your GitHub account.
2. Click **"Add New..."** ──> **"Project"**.
3. Under "Import Git Repository", find and import your `ramboll-attendance-demo` repository.

### Step 3: Configure Project Settings
Vercel will detect the project structure. Configure the settings as follows:
* **Framework Preset**: Other (Static HTML/CSS/JS)
* **Build and Output Settings**:
  * *We have already added the `vercel.json` file in the root folder, which tells Vercel to route traffic automatically. You do not need to configure anything else here!*
* Click **"Deploy"**.

Once deployed, Vercel will give you a public URL (e.g., `https://ramboll-attendance-demo.vercel.app`).

---

## 3. How to Run the Demo with Friends

1. **Start Your Local Server**:
   * Open your host laptop, connect it to your Samsung S25 Ultra hotspot, and run the backend Web API:
     ```powershell
     cd backend\RambollAttendanceAPI
     dotnet run --urls http://0.0.0.0:5000
     ```
2. **Retrieve Your Laptop's Local IP**:
   * Open a command prompt and run `ipconfig`.
   * Find your IPv4 address under your Wi-Fi interface (e.g. `192.168.43.10`).
3. **Open Your Admin Dashboard**:
   * Open the Vercel app URL in your browser with the admin parameter:
     `https://ramboll-attendance-demo.vercel.app/index.html?mode=admin`
   * Open the Settings Cog in the bottom right, enter your local IP `192.168.43.10`, and save.
4. **Generate and Share the Link**:
   * Click **"Generate Link"** in the Admin panel.
   * It will output a customized URL containing the IP:
     `https://ramboll-attendance-demo.vercel.app/index.html?mode=client&host_ip=192.168.43.10`
   * Share this link with your 5 friends.
5. **Friends Connect**:
   * Your friends connect to your Samsung S25 Ultra hotspot.
   * They click the generated link.
   * Their browsers automatically hook into your local backend API over Wi-Fi, prompt them to enter their names, and start transmitting heartbeats, registering them on your Admin Dashboard live!
