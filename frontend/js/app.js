/* ==========================================================================
   Ramboll Smart Attendance - Secure Portal Controller
   Vanilla JavaScript - Strict Role-Based Access Control (RBAC)
   ========================================================================== */

let API_BASE = 'http://localhost:5000/api';
let ssoIdentity = '';
let fullName = '';
let mockMac = '';
let mockHostname = '';
let hostIpSetting = '';

let adminToken = '';
let activeView = 'admin'; // Default view is admin dashboard
let heartbeatIntervalId = null;
let pollIntervalId = null;

document.addEventListener('DOMContentLoaded', () => {
    loadSettings();
    initializeUI();
});

function loadSettings() {
    // 1. Check URL parameters first for auto-configuration from shared link
    const urlParams = new URLSearchParams(window.location.search);
    const hostIpParam = urlParams.get('host_ip');
    if (hostIpParam) {
        localStorage.setItem('host_ip', hostIpParam);
    }

    ssoIdentity = localStorage.getItem('sso_identity') || '';
    fullName = localStorage.getItem('full_name') || '';
    mockMac = localStorage.getItem('mock_mac') || '';
    mockHostname = localStorage.getItem('mock_hostname') || '';
    hostIpSetting = localStorage.getItem('host_ip') || '';
    adminToken = localStorage.getItem('admin_token') || '';

    // Calculate API Base depending on host IP setting
    if (hostIpSetting) {
        API_BASE = `http://${hostIpSetting}:5000/api`;
    } else {
        const browserHost = window.location.hostname;
        if (browserHost && browserHost !== 'localhost' && browserHost !== '127.0.0.1' && !browserHost.includes('github.io')) {
            API_BASE = `http://${browserHost}:5000/api`;
            localStorage.setItem('host_ip', browserHost);
            hostIpSetting = browserHost;
        }
    }
}

function initializeUI() {
    document.getElementById('setting-host-ip').value = hostIpSetting;
    document.getElementById('diag-host-ip').innerText = hostIpSetting || 'localhost';

    const urlParams = new URLSearchParams(window.location.search);
    const path = window.location.pathname;
    const mode = urlParams.get('mode');
    const join = urlParams.get('join');

    // Detect if page should render in Client view (Employee)
    const isClientMode = (mode === 'client') || (join !== null) || path.includes('/join/');

    if (isClientMode) {
        activeView = 'client';
        // 1. Enforce Employee Mode: Hide all navigation, statistics, settings, and dashboard sections
        document.getElementById('nav-header').style.display = 'none';
        document.getElementById('admin-settings-drawer').style.display = 'none';
        document.getElementById('section-admin').style.display = 'none';
        document.getElementById('admin-login-overlay').classList.remove('active');
        document.getElementById('section-client').classList.add('active');

        // Bind registration buttons
        document.getElementById('btn-submit-registration').addEventListener('click', handleEmployeeLogin);

        // If credentials exist, transition to Welcome page immediately and start tracking
        if (fullName && ssoIdentity) {
            transitionToWelcome();
        } else {
            // Show only login page
            document.getElementById('client-login-view').classList.add('active');
            document.getElementById('client-welcome-view').classList.remove('active');
        }
    } else {
        activeView = 'admin';
        // 2. Enforce Admin Mode: Hide employee views
        document.getElementById('section-client').style.display = 'none';
        document.getElementById('section-admin').classList.add('active');

        // Bind Admin Actions
        document.getElementById('btn-submit-admin-login').addEventListener('click', handleAdminLogin);
        document.getElementById('btn-logout-admin').addEventListener('click', handleAdminLogout);
        document.getElementById('btn-toggle-settings').addEventListener('click', () => {
            document.getElementById('settings-drawer-content').classList.toggle('active');
        });
        document.getElementById('btn-save-settings').addEventListener('click', saveConfigSettings);
        document.getElementById('btn-clear-db').addEventListener('click', handleClearDatabase);
        document.getElementById('btn-export-csv').addEventListener('click', handleExportCSV);
        document.getElementById('btn-generate-link').addEventListener('click', handleGenerateShareLink);

        // Enforce Login Lock
        if (adminToken === 'RambollAdminSecret2026') {
            document.getElementById('admin-login-overlay').classList.remove('active');
            document.getElementById('nav-header').style.display = 'block';
            document.getElementById('admin-settings-drawer').style.display = 'block';
            startAdminDashboard();
        } else {
            // Force Lock screen visible
            document.getElementById('admin-login-overlay').classList.add('active');
            document.getElementById('nav-header').style.display = 'none';
            document.getElementById('admin-settings-drawer').style.display = 'none';
        }
    }
}

// ==========================================================================
// EMPLOYEE CLIENT HANDLERS
// ==========================================================================

async function handleEmployeeLogin() {
    const usernameInput = document.getElementById('reg-name').value.trim();
    if (!usernameInput) {
        alert("Please enter a username to continue.");
        return;
    }

    // Generate mock details
    const cleanName = usernameInput.toLowerCase().replace(/[^a-z0-9]/g, '_');
    const genSSO = `RAMBOLL\\${cleanName}`;
    const genMAC = generateMockMAC();
    const genHostname = `${cleanName.toUpperCase()}-LAPTOP`;

    // Try a test network ping to see if we can reach the Web API
    try {
        const pingPayload = {
            Hostname: genHostname,
            MAC_Address: genMAC,
            Event_Type: 'WAKE',
            SSID: 'Samsung S20 Ultra',
            Timestamp: new Date()
        };

        const response = await fetch(`${API_BASE}/telemetry/log`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-SSO-Identity': genSSO,
                'X-Employee-Name': usernameInput
            },
            body: JSON.stringify(pingPayload)
        });

        if (response.ok) {
            // Connected to correct network. Persist and enter welcome view
            localStorage.setItem('sso_identity', genSSO);
            localStorage.setItem('full_name', usernameInput);
            localStorage.setItem('mock_mac', genMAC);
            localStorage.setItem('mock_hostname', genHostname);

            ssoIdentity = genSSO;
            fullName = usernameInput;
            mockMac = genMAC;
            mockHostname = genHostname;

            transitionToWelcome();
        } else {
            // Server returned error (e.g. untrusted subnet)
            showAccessDenied();
        }
    } catch (err) {
        // Fetch failed entirely (indicating client is on a different Wi-Fi subnet)
        showAccessDenied();
    }
}

function transitionToWelcome() {
    document.getElementById('client-login-view').classList.remove('active');
    document.getElementById('client-welcome-view').classList.add('active');
    document.getElementById('display-client-name').innerText = fullName;
    document.getElementById('network-warning-overlay').classList.remove('active');

    // Run telemetry worker heartbeats immediately, then every 10 seconds
    sendBrowserAgentHeartbeat();
    if (heartbeatIntervalId) clearInterval(heartbeatIntervalId);
    heartbeatIntervalId = setInterval(sendBrowserAgentHeartbeat, 10000);
}

async function sendBrowserAgentHeartbeat() {
    if (activeView !== 'client') return;

    const payload = {
        Hostname: mockHostname,
        MAC_Address: mockMac,
        Event_Type: 'HEARTBEAT',
        SSID: 'Samsung S20 Ultra', // Fixed Ramboll hotspot SSID for demo POC
        Timestamp: new Date()
    };

    try {
        const response = await fetch(`${API_BASE}/telemetry/log`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-SSO-Identity': ssoIdentity,
                'X-Employee-Name': fullName
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            document.getElementById('network-warning-overlay').classList.remove('active');
        } else {
            showAccessDenied();
        }
    } catch (err) {
        showAccessDenied();
    }
}

function showAccessDenied() {
    document.getElementById('network-warning-overlay').classList.add('active');
    if (heartbeatIntervalId) {
        clearInterval(heartbeatIntervalId);
        heartbeatIntervalId = null;
    }
}

// ==========================================================================
// ADMIN DASHBOARD ACTIONS
// ==========================================================================

function handleAdminLogin() {
    const password = document.getElementById('admin-password').value;
    if (password === 'RambollAdmin2026') {
        localStorage.setItem('admin_token', 'RambollAdminSecret2026');
        window.location.reload();
    } else {
        alert("Access Denied: Invalid credentials.");
    }
}

function handleAdminLogout() {
    localStorage.removeItem('admin_token');
    window.location.reload();
}

function startAdminDashboard() {
    refreshAdminData();
    if (pollIntervalId) clearInterval(pollIntervalId);
    pollIntervalId = setInterval(refreshAdminData, 3000);
}

async function refreshAdminData() {
    try {
        // 1. Fetch summaries with Admin Authentication token
        const responseSummary = await fetch(`${API_BASE}/reports/summary`, {
            headers: { 'X-Admin-Token': adminToken }
        });
        
        if (responseSummary.status === 403) {
            handleAdminLogout();
            return;
        }
        
        if (!responseSummary.ok) throw new Error();
        const summaries = await responseSummary.json();
        
        const summaryTbody = document.getElementById('management-table-body');
        summaryTbody.innerHTML = '';
        
        if (summaries.length === 0) {
            summaryTbody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: var(--text-secondary);">No records logged today.</td></tr>`;
        } else {
            summaries.forEach(s => {
                const tr = document.createElement('tr');
                
                // Calculate online/offline status badge
                const timeDiffSeconds = (new Date() - new Date(s.last_Seen)) / 1000;
                // If last heartbeat was within 45 seconds, status is Active, else Offline
                const isOnline = timeDiffSeconds < 45 && s.status !== 'ABSENT';
                const statusBadge = isOnline 
                    ? '<span class="badge present">Active</span>' 
                    : '<span class="badge absent">Offline</span>';

                tr.innerHTML = `
                    <td><strong>${s.fullName}</strong></td>
                    <td><code>${new Date(s.work_Date).toLocaleDateString()}</code></td>
                    <td>${formatTime(s.first_Seen)}</td>
                    <td>${formatTime(s.last_Seen)}</td>
                    <td><code style="color: var(--brand-cyan);">${s.iP_Address || 'Unknown'}</code></td>
                    <td>${statusBadge}</td>
                    <td><strong>${s.active_Hours.toFixed(2)} hrs</strong></td>
                `;
                summaryTbody.appendChild(tr);
            });
            document.getElementById('diag-active-count').innerText = `${summaries.length} Tracked Employees`;
        }

    } catch (err) {
        console.error("Admin refresh error:", err);
    }
}

function handleGenerateShareLink() {
    const origin = window.location.origin;
    const hostIp = hostIpSetting || 'localhost';
    
    // Generates share link matching specifications (Auto-embeds client and host IP parameters)
    const shareLink = `${origin}/join/ABCD1234?host_ip=${hostIp}`;
    
    const input = document.getElementById('generated-share-link');
    input.value = shareLink;
    
    // Copy automatically to clipboard
    navigator.clipboard.writeText(shareLink).then(() => {
        alert("Attendance link generated and copied to clipboard!");
    }).catch(err => {
        console.warn("Clipboard copy blocked. Please copy manually from the input field.");
    });
}

function handleExportCSV() {
    window.open(`${API_BASE}/reports/export?admin_token=${adminToken}`, '_blank');
}

async function handleClearDatabase() {
    if (!confirm("Are you sure you want to clear all telemetry data and reset the daily summaries for the demo?")) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/reports/reset`, {
            method: 'POST',
            headers: { 'X-Admin-Token': adminToken }
        });

        if (response.ok) {
            alert("Database cleared successfully. Starting freshly!");
            refreshAdminData();
        } else {
            alert("Failed to clear database.");
        }
    } catch (err) {
        alert("API connection failure: " + err.message);
    }
}

// ==========================================================================
// UTILITIES
// ==========================================================================

function saveConfigSettings() {
    const ip = document.getElementById('setting-host-ip').value.trim();
    localStorage.setItem('host_ip', ip);
    alert("Configuration settings updated successfully. Reloading...");
    window.location.reload();
}

function formatTime(isoString) {
    if (!isoString) return '--:--';
    return new Date(isoString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

function generateMockMAC() {
    const hexDigits = "0123456789ABCDEF";
    let mac = "00:15:5D:"; 
    for (let i = 0; i < 3; i++) {
        mac += hexDigits.charAt(Math.floor(Math.random() * 16));
        mac += hexDigits.charAt(Math.floor(Math.random() * 16));
        if (i < 2) mac += ":";
    }
    return mac;
}

window.copyShareLink = function() {
    const input = document.getElementById('generated-share-link');
    if (input.value) {
        navigator.clipboard.writeText(input.value);
        alert("Link copied to clipboard!");
    }
};
