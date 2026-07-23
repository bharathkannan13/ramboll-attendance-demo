/* ==========================================================================
   Ramboll Smart Attendance - Shareable Portal Controller
   Vanilla JavaScript - Direct Hotspot Integration & SSO simulation
   ========================================================================== */

let API_BASE = 'http://localhost:5000/api';
let ssoIdentity = '';
let fullName = '';
let mockMac = '';
let mockHostname = '';
let hostIpSetting = '';

let activeView = 'client'; // Default view tab

document.addEventListener('DOMContentLoaded', () => {
    loadSettings();
    initializeUI();
    
    // Check if user identity exists in localStorage. If not, open SSO mock registration modal.
    if (!ssoIdentity || !fullName) {
        document.getElementById('login-modal-overlay').classList.add('active');
    } else {
        startApp();
    }

    // Set up Settings drawer toggles
    document.getElementById('btn-toggle-settings').addEventListener('click', () => {
        document.getElementById('settings-drawer-content').classList.toggle('active');
    });

    document.getElementById('btn-save-settings').addEventListener('click', saveConfigSettings);
    document.getElementById('btn-reset-identity').addEventListener('click', resetIdentity);
    document.getElementById('btn-submit-registration').addEventListener('click', submitRegistrationModal);
    
    // Simulator controls
    document.getElementById('btn-send-sim').addEventListener('click', handleInjectTelemetry);
    document.getElementById('btn-refresh').addEventListener('click', refreshData);
    document.getElementById('btn-export-csv').addEventListener('click', handleExportCSV);
    document.getElementById('btn-clear-db').addEventListener('click', handleClearDatabase);
    document.getElementById('btn-generate-link').addEventListener('click', handleGenerateShareLink);
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

    // Calculate API Base depending on host IP setting
    if (hostIpSetting) {
        API_BASE = `http://${hostIpSetting}:5000/api`;
    } else {
        // Fallback: Default to current browser host
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
    
    // Auto-detect mode. Default is 'admin' (Manager Dashboard) unless 'client' is explicitly specified
    const urlParams = new URLSearchParams(window.location.search);
    const mode = urlParams.get('mode');
    
    if (mode === 'client') {
        // Hide tabs from employees completely
        document.getElementById('tab-client').style.display = 'none';
        document.getElementById('tab-admin').style.display = 'none';
        switchView('client');
    } else {
        // Admin Dashboard is the default main view. Hide tabs.
        document.getElementById('tab-client').style.display = 'none';
        document.getElementById('tab-admin').style.display = 'none';
        switchView('admin');
    }
}

function startApp() {
    document.getElementById('client-welcome-name').innerText = `Welcome, ${fullName}!`;
    document.getElementById('client-sso-label').innerText = `SSO Identity: ${ssoIdentity}`;
    document.getElementById('diag-sso-user').innerText = ssoIdentity;

    refreshData();

    // Start background heartbeats (simulates browser acting as tracking agent on hotspot network)
    // Runs every 10 seconds
    setInterval(sendBrowserAgentHeartbeat, 10000);

    // Dynamic UI polling for updates
    // Runs every 3 seconds for fast synchronization
    setInterval(refreshData, 3000);
}

function switchView(view) {
    activeView = view;
    
    // Toggle active tabs CSS
    document.getElementById('tab-client').className = view === 'client' ? 'btn-primary' : 'btn-secondary';
    document.getElementById('tab-admin').className = view === 'admin' ? 'btn-primary' : 'btn-secondary';

    // Show/hide sections
    document.getElementById('section-client').className = `view-section ${view === 'client' ? 'active' : ''}`;
    document.getElementById('section-admin').className = `view-section ${view === 'admin' ? 'active' : ''}`;
    
    refreshData();
}

// ==========================================================================
// BROWSER-BASED TELEMETRY WORKER
// ==========================================================================

async function sendBrowserAgentHeartbeat() {
    // Only send browser heartbeats in Client view mode
    if (activeView !== 'client') return;

    const payload = {
        Hostname: mockHostname,
        MAC_Address: mockMac,
        Event_Type: 'HEARTBEAT',
        SSID: 'Galaxy S25 Ultra 7A56', // Fixed Ramboll hotspot SSID for demo convenience
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
            console.log("[AGENT] Hotspot Heartbeat sent successfully.");
            updateConnectionStatus(true, 'Galaxy S25 Ultra 7A56');
        } else {
            console.warn("[AGENT] Hotspot Heartbeat failed. Network rejected.");
            updateConnectionStatus(false);
        }
    } catch (err) {
        console.error("[AGENT] Error dispatching browser telemetry heartbeat:", err);
        updateConnectionStatus(false);
    }
}

// ==========================================================================
// CORE API FETCH CALLS
// ==========================================================================

async function refreshData() {
    if (activeView === 'client') {
        await fetchClientDashboard();
    } else {
        await fetchManagerDashboard();
    }
}

async function fetchClientDashboard() {
    if (!ssoIdentity) return;

    try {
        const response = await fetch(`${API_BASE}/reports/dashboard`, {
            headers: { 'X-SSO-Identity': ssoIdentity }
        });

        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const data = await response.json();

        // Render client dashboard metrics
        document.getElementById('client-metric-hours').innerText = `${data.active_Hours.toFixed(2)} hrs`;
        document.getElementById('client-metric-first-seen').innerText = formatTime(data.first_Seen);
        
        // Show status on page
        const statusLabel = document.getElementById('client-connection-status');
        const ssidLabel = document.getElementById('client-network-ssid');
        
        // Simple mock network status check
        if (data.active_Hours > 0 || consecutiveOfflineCount === 0) {
            statusLabel.innerText = "CONNECTED TO RAMBOLL NETWORK";
            statusLabel.style.color = "var(--green-glow)";
            ssidLabel.innerText = "Active SSID: Galaxy S25 Ultra 7A56";
            updateConnectionStatus(true, 'Galaxy S25 Ultra 7A56');
        } else {
            statusLabel.innerText = "CLOCK SUSPENDED - UNTRUSTED SSID";
            statusLabel.style.color = "var(--red-glow)";
            ssidLabel.innerText = "Connect to Galaxy S25 Ultra hotspot to resume attendance tracking.";
            updateConnectionStatus(false);
        }
    } catch (err) {
        console.error("Dashboard sync error:", err);
        updateConnectionStatus(false);
    }
}

async function fetchManagerDashboard() {
    try {
        // Fetch summaries
        const responseSummary = await fetch(`${API_BASE}/reports/summary`);
        if (!responseSummary.ok) throw new Error();
        const summaries = await responseSummary.summary ? await responseSummary.summary : await responseSummary.json();
        
        const summaryTbody = document.getElementById('management-table-body');
        summaryTbody.innerHTML = '';
        
        if (summaries.length === 0) {
            summaryTbody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: var(--text-secondary);">No records logged today.</td></tr>`;
        } else {
            summaries.forEach(s => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td><strong>${s.fullName}</strong></td>
                    <td><code>${s.ssO_Identity}</code></td>
                    <td>${s.department}</td>
                    <td>${formatTime(s.first_Seen)}</td>
                    <td>${formatTime(s.last_Seen || s.last_seen)}</td>
                    <td><strong style="color: var(--brand-cyan);">${s.active_Hours.toFixed(2)} hrs</strong></td>
                    <td><span class="badge ${s.status.toLowerCase()}">${s.status}</span></td>
                `;
                summaryTbody.appendChild(tr);
            });
            document.getElementById('diag-active-count').innerText = `${summaries.length} Active Employees`;
        }

        // Fetch livestream history logs for all employees
        const responseLogs = await fetch(`${API_BASE}/reports/history/all`);
        if (!responseLogs.ok) throw new Error();
        const logs = await responseLogs.json();
        
        const logsTbody = document.getElementById('telemetry-table-body');
        logsTbody.innerHTML = '';
        
        if (logs.length === 0) {
            logsTbody.innerHTML = `<tr><td colspan="7" style="text-align: center; color: var(--text-secondary);">No logs registered.</td></tr>`;
        } else {
            logs.reverse().slice(0, 30).forEach(log => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td><strong>${new Date(log.timestamp).toLocaleTimeString()}</strong></td>
                    <td>${log.fullName || 'Guest User'}</td>
                    <td><code style="color: var(--brand-cyan);">${log.iP_Address || log.ip_Address || 'Unknown'}</code></td>
                    <td>${log.hostname}</td>
                    <td><code>${log.mac_Address}</code></td>
                    <td><span style="font-weight:600; color:${getEventColor(log.event_Type)};">${log.event_Type}</span></td>
                    <td>${log.ssid}</td>
                `;
                logsTbody.appendChild(tr);
            });
        }

    } catch (err) {
        console.error("Manager refresh error:", err);
    }
}

// ==========================================================================
// SETTINGS & MODAL ACTIONS
// ==========================================================================

function submitRegistrationModal() {
    const nameInput = document.getElementById('reg-name').value.trim();
    if (!nameInput) {
        alert("Please enter your name to authenticate.");
        return;
    }

    // Generate mock details
    const cleanName = nameInput.toLowerCase().replace(/[^a-z0-9]/g, '_');
    const genSSO = `RAMBOLL\\${cleanName}`;
    const genMAC = generateMockMAC();
    const genHostname = `${cleanName.toUpperCase()}-LAPTOP`;

    localStorage.setItem('sso_identity', genSSO);
    localStorage.setItem('full_name', nameInput);
    localStorage.setItem('mock_mac', genMAC);
    localStorage.setItem('mock_hostname', genHostname);

    ssoIdentity = genSSO;
    fullName = nameInput;
    mockMac = genMAC;
    mockHostname = genHostname;

    document.getElementById('login-modal-overlay').classList.remove('active');
    startApp();
}

function saveConfigSettings() {
    const ip = document.getElementById('setting-host-ip').value.trim();
    localStorage.setItem('host_ip', ip);
    alert("Configuration settings updated successfully. Reloading...");
    window.location.reload();
}

function resetIdentity() {
    if (confirm("Reset local SSO identity profile?")) {
        localStorage.clear();
        window.location.reload();
    }
}

// ==========================================================================
// MOCK SIMULATOR ACTIONS
// ==========================================================================

async function handleInjectTelemetry() {
    const targetSSO = document.getElementById('sim-sso').value.trim();
    const targetName = document.getElementById('sim-name').value.trim();
    const eventType = document.getElementById('sim-event').value;

    const payload = {
        Hostname: `${targetName.toUpperCase().replace(/\s+/g, '_')}-LAPTOP`,
        MAC_Address: generateMockMAC(),
        Event_Type: eventType,
        SSID: 'Galaxy S25 Ultra 7A56',
        Timestamp: new Date()
    };

    try {
        const response = await fetch(`${API_BASE}/telemetry/log`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-SSO-Identity': targetSSO,
                'X-Employee-Name': targetName
            },
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            alert(`Injected ${eventType} event successfully for ${targetName}!`);
            refreshData();
        } else {
            alert("Injection failed. Server rejected request.");
        }
    } catch (err) {
        alert("API connection failure: " + err.message);
    }
}

function handleExportCSV() {
    window.open(`${API_BASE}/reports/export`, '_blank');
}

async function handleClearDatabase() {
    if (!confirm("Are you sure you want to clear all telemetry data and reset the daily summaries for the demo?")) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/reports/reset`, {
            method: 'POST'
        });

        if (response.ok) {
            alert("Database cleared successfully. Starting freshly!");
            refreshData();
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

function formatTime(isoString) {
    if (!isoString) return '--:--';
    return new Date(isoString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

function generateMockMAC() {
    const hexDigits = "0123456789ABCDEF";
    let mac = "00:15:5D:"; // Microsoft OU prefix
    for (let i = 0; i < 3; i++) {
        mac += hexDigits.charAt(Math.floor(Math.random() * 16));
        mac += hexDigits.charAt(Math.floor(Math.random() * 16));
        if (i < 2) mac += ":";
    }
    return mac;
}

function getEventColor(type) {
    switch (type.toUpperCase()) {
        case 'WAKE':
        case 'UNLOCK':
            return 'var(--green-glow)';
        case 'HEARTBEAT':
            return 'var(--brand-cyan)';
        case 'LOCK':
        case 'SLEEP':
            return 'var(--yellow-glow)';
        default:
            return 'var(--red-glow)';
    }
}

let consecutiveOfflineCount = 0;

function updateConnectionStatus(active, ssid = '') {
    const dot = document.getElementById('nav-connection-dot');
    const label = document.getElementById('nav-connection-text');

    if (active) {
        consecutiveOfflineCount = 0;
        dot.classList.add('active');
        dot.style.backgroundColor = 'var(--green-glow)';
        label.innerText = `HOTSPOT ON (${ssid})`;
        document.getElementById('network-warning-overlay').classList.remove('active');
    } else {
        dot.classList.remove('active');
        dot.style.backgroundColor = 'var(--red-glow)';
        label.innerText = 'OFF-HOTSPOT DISCONNECTED';

        // Display the incorrect network warning overlay popup in client mode
        if (activeView === 'client') {
            document.getElementById('network-warning-overlay').classList.add('active');
        }
    }
}

function handleGenerateShareLink() {
    // Build share link dynamically containing local server host IP parameter
    const origin = window.location.origin;
    const path = window.location.pathname;
    const hostIp = hostIpSetting || 'localhost';
    const shareLink = `${origin}${path}?mode=client&host_ip=${hostIp}`;
    
    const input = document.getElementById('generated-share-link');
    input.value = shareLink;
    
    // Copy automatically to clipboard
    navigator.clipboard.writeText(shareLink).then(() => {
        alert("Share link generated and copied to clipboard!");
    }).catch(err => {
        console.warn("Clipboard copy blocked. Please copy manually from the input field.");
    });
}

// Expose copy and close functions to window object for HTML onclick bindings
window.copyShareLink = function() {
    const input = document.getElementById('generated-share-link');
    if (input.value) {
        navigator.clipboard.writeText(input.value);
        alert("Link copied to clipboard!");
    }
};

window.closeNetworkWarning = function() {
    document.getElementById('network-warning-overlay').classList.remove('active');
};
