# Local Deployment Guide (VS Code)

This guide provides step-by-step instructions on how to open, build, run, and verify the Ramboll Smart Attendance Prototype locally using VS Code.

---

## 1. Opening the Project in VS Code
1. Open **Visual Studio Code**.
2. Click **File** ──> **Open Folder...** (or press `Ctrl+K Ctrl+O`).
3. Browse and select the parent folder:
   `C:\Users\bharath\ram\RambollAttendancePrototype`
4. Click **Select Folder**.

---

## 2. Launching the Backend Web API
The backend handles the SQL Server connection, telemetry logging, and time aggregation.

1. Open a new terminal in VS Code:
   * Go to **Terminal** ──> **New Terminal** (or press `Ctrl+Shift+`` `).
2. Navigate to the Web API folder:
   ```powershell
   cd backend\RambollAttendanceAPI
   ```
3. Build the API project:
   ```powershell
   dotnet build
   ```
4. Run the Web API on a local server (listening on port 5000):
   ```powershell
   dotnet run --urls http://localhost:5000
   ```
   * *Keep this terminal tab open and running.*

---

## 3. Running the C# Presence Agent
The Presence Agent runs silently on the user's laptop, monitoring power events and network changes.

1. Open a **second terminal tab** in VS Code (click the **`+`** icon on the top right of the terminal panel).
2. Navigate to the Presence Agent folder:
   ```powershell
   cd backend\RambollPresenceAgent
   ```
3. Run the Presence Agent background worker:
   ```powershell
   dotnet run
   ```
   * *The agent will start up, detect your current SSID, perform Windows SSO login, and automatically launch the dashboard.*

---

## 4. Running the Frontend Portal
Since the frontend consists of static HTML, CSS, and JS, you have two options to run it:

### Option A: Open File Directly (Standard)
1. Double-click `frontend/index.html` in the VS Code File Explorer.
2. It will open in your default browser and automatically connect to your running local Web API server (`http://localhost:5000`).

### Option B: Local Web Server (Recommended for Sharing)
If you want to host it as a local web server (instead of opening it as a `file:///` path) or share it with others on your Wi-Fi:
1. In VS Code, install the extension **"Live Server"** (by Ritwick Dey).
2. Right-click `frontend/index.html` and select **"Open with Live Server"**.
3. It will host the frontend at `http://127.0.0.1:5500/frontend/index.html`.

---

## 5. Sharing the Demo Over Wi-Fi
If you want others connected to your hotspot (`Galaxy S25 Ultra 7A56`) to access the dashboard:
1. Find your laptop's Wi-Fi IP address by running `ipconfig` in the command prompt (e.g., `192.168.43.10`).
2. Open `frontend/js/app.js` in VS Code and update the backend target:
   ```javascript
   // Change from localhost to your laptop's actual IP
   const API_BASE = 'http://192.168.43.10:5000/api'; 
   ```
3. Now, other users connected to your mobile hotspot can open the dashboard in their browsers by typing:
   `http://192.168.43.10:5500/frontend/index.html` (if using Live Server).
