# Stage 1 Guide: GitHub Code Transfer, Laptop Setup, & Local Execution

> **Document ID**: STAGE-01-LAPTOP-SETUP-2026  
> **System Name**: Bkran Group Connect (Enterprise Attendance Analytics Platform)  
> **Target Horizon**: Initial Laptop Preparation & Local Verification

---

## 📌 Phase 1: Transfer Code to Ramboll Corporate GitHub

```powershell
# 1. Download full code history
git clone --mirror https://github.com/bharathkannan13/ramboll-attendance-demo.git temp-repo
cd temp-repo

# 2. Push code to Ramboll Corporate GitHub
git push --mirror https://github.com/ramboll/attendance-engine.git

# 3. Clean up temporary folder
cd ..
Remove-Item -Recurse -Force temp-repo
```

---

## 🛠️ Phase 2: Install Software Tools on Official Laptop

1. **VS Code**: [code.visualstudio.com](https://code.visualstudio.com)
2. **.NET 8.0 SDK**: [dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **Git**: [git-scm.com/download/win](https://git-scm.com/download/win)

---

## 💻 Phase 3: Pull Code to Laptop & Run Locally

```powershell
git clone https://github.com/ramboll/attendance-engine.git
cd attendance-engine/EnterpriseAttendance
dotnet run --project src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj
```

Open browser to `http://localhost:5000`.
