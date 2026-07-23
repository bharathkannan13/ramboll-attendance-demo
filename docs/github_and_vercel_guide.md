# Beginner's Guide: Pushing to GitHub & Deploying to Vercel

This guide provides simple, step-by-step instructions (with exact clicking actions) to push your code to GitHub and host your frontend on Vercel.

---

## Part 1: How to Push Your Code to GitHub

### Step 1: Create a GitHub Account & New Repository
1. Open your browser and go to: **[github.com](https://github.com)**
2. Click **Sign up** (or **Sign in** if you already have an account) and complete the login.
3. On the top-right corner of the dashboard, click the **`+`** icon and select **New repository**.
4. In the **Repository name** field, type: `ramboll-attendance-demo`
5. Choose **Public** (so Vercel can access it easily).
6. Leave "Add a README", "Add .gitignore", and "Choose a license" **UNCHECKED**.
7. Click the green button: **Create repository**.
8. GitHub will load a page showing a URL. Copy this URL (it looks like `https://github.com/your-username/ramboll-attendance-demo.git`).

---

### Step 2: Push Your Code using VS Code Terminal
1. Open **Visual Studio Code** inside your project folder (`C:\Users\bharath\ram\RambollAttendancePrototype`).
2. Open a new terminal inside VS Code:
   * Go to **Terminal** ──> **New Terminal** (or press `Ctrl+Shift+`` `).
3. Type the following commands, pressing **Enter** after each one:

```bash
# 1. Initialize Git in your folder
git init

# 2. Stage all your files (it will ignore compiled bin/obj folders because we created a .gitignore file)
git add .

# 3. Save your code snapshot locally
git commit -m "Initial commit"

# 4. Rename the default branch to 'main'
git branch -M main

# 5. Link your local project to the GitHub repository you created (Paste the URL you copied in Step 1)
git remote add origin HTTPS_URL_OF_YOUR_REPOSTORY_HERE

# 6. Push your files to GitHub
git push -u origin main
```
*Note: If prompted, log in with your GitHub credentials in the browser popup.*

---

## Part 2: How to Connect Your GitHub Code to Vercel

Once your files are on GitHub, you can host the frontend on Vercel with these click-by-click actions:

### Step 1: Log In to Vercel
1. Open your browser and go to: **[vercel.com](https://vercel.com)**
2. Click **Log In** on the top-right.
3. Click the button: **Continue with GitHub**.
4. Authorize Vercel to access your GitHub repositories if prompted.

---

### Step 2: Import Your Repository
1. On your Vercel Dashboard home screen, click the black button: **Add New...** and select **Project**.
2. Vercel will show a list of your GitHub repositories under the heading **"Import Git Repository"**.
3. Locate **`ramboll-attendance-demo`** in the list.
4. Click the blue button next to it: **Import**.

---

### Step 3: Configure Project Mappings
1. Vercel will show the **"Configure Project"** screen.
2. Under **Project Name**, leave it as `ramboll-attendance-demo`.
3. Under **Framework Preset**, leave it as **"Other"**.
4. Under **Root Directory**, leave it as **`./`** (the root of your project).
   * *Do NOT change anything else. The custom `vercel.json` we wrote at the root will automatically tell Vercel to host your static `frontend/` files!*
5. Click the blue button at the bottom: **Deploy**.

---

### Step 4: Access Your App URL
1. Vercel will show a progress screen saying "Building" and then "Congratulations!".
2. You will see a preview image of your portal. Click on the image or the URL next to it (e.g. `https://ramboll-attendance-demo.vercel.app`).
3. Your frontend is now successfully hosted live in the cloud!
