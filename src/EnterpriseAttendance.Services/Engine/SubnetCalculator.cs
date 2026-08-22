using System;
using System.Net;

namespace EnterpriseAttendance.Services.Engine
{
    public static class SubnetCalculator
    {
        /// <summary>
        /// Checks if an IP address falls within a CIDR subnet range.
        /// Supports formats: "10.100.0.0/16", "10.100.0.0/24", "10.100.0.0/8"
        /// Also supports wildcard format: "10.100.*" (legacy compatibility)
        /// </summary>
        public static bool IsIpInSubnet(string ipAddress, string subnetCidr)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(subnetCidr))
                return false;

            try
            {
                if (subnetCidr.Contains("*"))
                {
                    // Handle wildcard like "10.100.*"
                    var prefix = subnetCidr.Substring(0, subnetCidr.IndexOf('*'));
                    return ipAddress.StartsWith(prefix);
                }

                if (!subnetCidr.Contains("/"))
                {
                    // Exact match
                    return ipAddress == subnetCidr;
                }

                var parts = subnetCidr.Split('/');
                if (parts.Length != 2) return false;

                var baseIp = parts[0];
                if (!int.TryParse(parts[1], out int maskLength) || maskLength < 0 || maskLength > 32)
                    return false;

                uint ipUint = IpToUint(ipAddress);
                uint baseIpUint = IpToUint(baseIp);

                if (ipAddress != "0.0.0.0" && ipUint == 0) return false;
                if (baseIp != "0.0.0.0" && baseIpUint == 0) return false;

                // Create mask handling 0 mask length edge case
                uint mask = maskLength == 0 ? 0 : uint.MaxValue << (32 - maskLength);
                return (ipUint & mask) == (baseIpUint & mask);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parses an IP address string into a uint for bitwise comparison.
        /// </summary>
        private static uint IpToUint(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out IPAddress parsedIp))
                return 0;

            byte[] bytes = parsedIp.GetAddressBytes();
            if (bytes.Length != 4)
                return 0; // Only support IPv4

            return (uint)((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]);
        }
    }
}
