using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;

namespace RambollPresenceAgent
{
    class Program
    {
        private static string _apiUrl = "http://localhost:5000/api/telemetry/log";
        private static List<string> _allowedSSIDs = new();
        private static int _heartbeatIntervalSeconds = 30;
        private static bool _autoOpenBrowserOnConnect = true;
        private static readonly HttpClient _httpClient = new();
        private static string _macAddress = "00-00-00-00-00-00";
        private static string _hostname = "UNKNOWN-HOST";
        private static string _ssoIdentity = "UNKNOWN-USER";
        private static bool _isRunning = true;

        private static string _lastEvaluatedSSID = "INITIAL_STATE";
        private static DateTime _lastNetworkChangeTime = DateTime.MinValue;
        private static readonly object _networkLock = new();

        static async Task Main(string[] args)
        {
            Console.Title = "Ramboll Smart Attendance Presence Agent (Upgraded)";
            Console.WriteLine("=================================================================");
            Console.WriteLine("        RAMBOLL SMART ATTENDANCE PRESENCE AGENT (POC)           ");
            Console.WriteLine("=================================================================");

            LoadConfiguration();
            InitializeDeviceInfo();

            // Register Windows System Events
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;

            // Subscribe to network connectivity change events for real-time reactivity
            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;

            Console.WriteLine($"[INIT] SSO Identity detected: {_ssoIdentity}");
            Console.WriteLine($"[INIT] MAC Address: {_macAddress}");
            Console.WriteLine($"[INIT] Hostname: {_hostname}");
            Console.WriteLine($"[INIT] Allowed SSIDs: {string.Join(", ", _allowedSSIDs)}");
            Console.WriteLine($"[INIT] Auto-open portal on Ramboll Net: {_autoOpenBrowserOnConnect}");
            Console.WriteLine($"[INIT] Monitoring started. Press Ctrl+C to exit.");
            Console.WriteLine("-----------------------------------------------------------------");

            // Evaluate current network on startup
            _lastEvaluatedSSID = GetActiveWifiSSID();
            await SendTelemetryAsync("WAKE");

            // If starting connected to authorized network, trigger browser launch
            if (IsSSIDAuthorized(_lastEvaluatedSSID) && _autoOpenBrowserOnConnect)
            {
                TriggerPortalBrowser();
            }

            // Main Heartbeat loop
            while (_isRunning)
            {
                string activeSSID = GetActiveWifiSSID();
                bool isNetworkAuthorized = IsSSIDAuthorized(activeSSID);

                if (isNetworkAuthorized)
                {
                    Console.WriteLine($"[STATUS] Connected to Ramboll Hotspot Network: '{activeSSID}'. Sending Heartbeat...");
                    await SendTelemetryAsync("HEARTBEAT");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[STATUS] Suspended: Current SSID '{activeSSID}' is not authorized. Clock paused.");
                    Console.ResetColor();
                }

                // Sleep for the heartbeat interval
                await Task.Delay(_heartbeatIntervalSeconds * 1000);
            }

            // Unsubscribe before exit
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            SystemEvents.SessionSwitch -= OnSessionSwitch;
            NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
        }

        private static void LoadConfiguration()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var configuration = builder.Build();

                _apiUrl = configuration["AttendanceAgent:ApiUrl"] ?? _apiUrl;
                _heartbeatIntervalSeconds = int.TryParse(configuration["AttendanceAgent:HeartbeatIntervalSeconds"], out int val) ? val : _heartbeatIntervalSeconds;
                _autoOpenBrowserOnConnect = bool.TryParse(configuration["AttendanceAgent:AutoOpenBrowserOnConnect"], out bool autoOpen) ? autoOpen : _autoOpenBrowserOnConnect;
                
                _allowedSSIDs = configuration.GetSection("AttendanceAgent:AllowedSSIDs")
                    .GetChildren()
                    .Select(c => c.Value ?? string.Empty)
                    .Where(val => !string.IsNullOrEmpty(val))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Could not load appsettings.json. Using default settings. Error: {ex.Message}");
            }
        }

        private static void InitializeDeviceInfo()
        {
            _hostname = Environment.MachineName;
            _ssoIdentity = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            try
            {
                _macAddress = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up 
                        && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault(mac => !string.IsNullOrEmpty(mac)) 
                    ?? "00-00-00-00-00-00";

                // Format MAC address as AA:BB:CC:DD:EE:FF for readability
                if (_macAddress != "00-00-00-00-00-00" && _macAddress.Length == 12)
                {
                    _macAddress = string.Join(":", Enumerable.Range(0, 6).Select(i => _macAddress.Substring(i * 2, 2)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Could not determine network interface MAC. Error: {ex.Message}");
            }
        }

        private static async void OnNetworkAddressChanged(object? sender, EventArgs e)
        {
            // Debounce event: Windows network stack triggers this event multiple times rapidly
            lock (_networkLock)
            {
                if ((DateTime.Now - _lastNetworkChangeTime).TotalMilliseconds < 1500)
                {
                    return;
                }
                _lastNetworkChangeTime = DateTime.Now;
            }

            // Small delay to let network interface configure IP settings fully
            await Task.Delay(1000);

            string currentSSID = GetActiveWifiSSID();
            
            if (!string.Equals(currentSSID, _lastEvaluatedSSID, StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[NETWORK] Wi-Fi connection changed: '{_lastEvaluatedSSID}' ──> '{currentSSID}'");
                Console.ResetColor();

                bool isOldAuthorized = IsSSIDAuthorized(_lastEvaluatedSSID);
                bool isNewAuthorized = IsSSIDAuthorized(currentSSID);

                _lastEvaluatedSSID = currentSSID;

                if (isNewAuthorized)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[NETWORK] Connected to authorized Ramboll SSID: '{currentSSID}'. Clock Resumed.");
                    Console.ResetColor();

                    await SendTelemetryAsync("WAKE");

                    if (_autoOpenBrowserOnConnect)
                    {
                        TriggerPortalBrowser();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[NETWORK] Disconnected from Ramboll Hotspot. Connected to: '{currentSSID}'. Clock Paused.");
                    Console.ResetColor();

                    // Send immediate OFFLINE signal so the Web API and Web Dashboard register the switch instantly
                    await SendTelemetryAsync("OFFLINE");
                }
            }
        }

        private static async void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            string eventName = e.Mode == PowerModes.Suspend ? "SLEEP" : (e.Mode == PowerModes.Resume ? "WAKE" : string.Empty);
            if (!string.IsNullOrEmpty(eventName))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[EVENT] Power Mode Changed detected: {eventName}");
                Console.ResetColor();
                await SendTelemetryAsync(eventName);
            }
        }

        private static async void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            string eventName = e.Reason == SessionSwitchReason.SessionLock ? "LOCK" : (e.Reason == SessionSwitchReason.SessionUnlock ? "UNLOCK" : string.Empty);
            if (!string.IsNullOrEmpty(eventName))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[EVENT] Session Switch detected: {eventName}");
                Console.ResetColor();
                await SendTelemetryAsync(eventName);
            }
        }

        private static async Task SendTelemetryAsync(string eventType)
        {
            string activeSSID = GetActiveWifiSSID();

            var payload = new
            {
                Hostname = _hostname,
                MAC_Address = _macAddress,
                Event_Type = eventType,
                SSID = activeSSID,
                Timestamp = DateTime.Now
            };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
                request.Headers.Add("X-SSO-Identity", _ssoIdentity);
                request.Content = JsonContent.Create(payload);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[API] Telemetry sent: {eventType} | SSID: {activeSSID} | Status: Success");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[API] Failed to send telemetry. HTTP Status: {response.StatusCode}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[API] Network error communicating with Web API: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static bool IsSSIDAuthorized(string ssid)
        {
            return _allowedSSIDs.Any(allowed => string.Equals(allowed, ssid, StringComparison.OrdinalIgnoreCase));
        }

        private static void TriggerPortalBrowser()
        {
            try
            {
                string portalPath = FindPortalPath();
                if (!string.IsNullOrEmpty(portalPath))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[AUTO-LAUNCH] Opening Ramboll Smart Attendance Portal at: {portalPath}");
                    Console.ResetColor();

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = portalPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    Console.WriteLine("[WARN] [AUTO-LAUNCH] Could not find the frontend portal file path recursively in parent folders.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] [AUTO-LAUNCH] Failed to open browser: {ex.Message}");
            }
        }

        private static string FindPortalPath()
        {
            string current = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(current))
            {
                string possiblePath = Path.Combine(current, "frontend", "index.html");
                if (File.Exists(possiblePath))
                {
                    return possiblePath;
                }
                
                string possiblePathParent = Path.Combine(current, "RambollAttendancePrototype", "frontend", "index.html");
                if (File.Exists(possiblePathParent))
                {
                    return possiblePathParent;
                }

                var parentDir = Directory.GetParent(current);
                if (parentDir == null || parentDir.FullName == current) break;
                current = parentDir.FullName;
            }
            return string.Empty;
        }

        private static string GetActiveWifiSSID()
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "wlan show interfaces",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("SSID", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Could not parse active WiFi SSID. Error: {ex.Message}");
            }
            return "Disconnected";
        }
    }
}
