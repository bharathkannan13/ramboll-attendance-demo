using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EnterpriseAttendance.Core.Entities;
using EnterpriseAttendance.Core.Enums;
using EnterpriseAttendance.Core.Interfaces;
using EnterpriseAttendance.Infrastructure.Data;

namespace EnterpriseAttendance.Services.Engine
{
    public class NetworkClassifier : INetworkClassifier
    {
        private readonly AttendanceDbContext _context;
        private readonly IConfiguration _configuration;

        public NetworkClassifier(AttendanceDbContext context, IConfiguration? configuration = null)
        {
            _context = context;
            _configuration = configuration ?? new ConfigurationBuilder().Build();
        }

        private class OfficeConfigSubnet
        {
            public string Name { get; set; }
            public string City { get; set; }
            public List<string> Subnets { get; set; }
        }

        public async Task<(NetworkLocationType LocationType, int? OfficeLocationId, int? MatchedNetworkId)> ClassifyNetworkAsync(string ipAddress, string ssid, string subnet)
        {
            if (string.IsNullOrWhiteSpace(ssid) && string.IsNullOrWhiteSpace(ipAddress))
            {
                return (NetworkLocationType.Unknown, null, null);
            }

            var officeNetworks = await _context.OfficeNetworks
                .Include(n => n.OfficeLocation)
                .Where(n => n.IsActive)
                .ToListAsync();

            // 1. Try SSID match first against Corporate SSIDs
            if (!string.IsNullOrWhiteSpace(ssid))
            {
                var ssidMatch = officeNetworks.FirstOrDefault(n =>
                    n.NetworkType == NetworkType.SSID &&
                    n.NetworkValue.Equals(ssid, StringComparison.OrdinalIgnoreCase));

                if (ssidMatch != null)
                {
                    return (NetworkLocationType.CorporateOffice, ssidMatch.OfficeLocationId, ssidMatch.Id);
                }
            }

            // 2. Try Subnet match next against Corporate Subnets
            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                foreach (var net in officeNetworks.Where(n => n.NetworkType == NetworkType.Subnet))
                {
                    if (SubnetCalculator.IsIpInSubnet(ipAddress, net.NetworkValue))
                    {
                        return (NetworkLocationType.CorporateOffice, net.OfficeLocationId, net.Id);
                    }
                }

                var configSubnets = _configuration.GetSection("NetworkConfig:OfficeSubnets").Get<List<OfficeConfigSubnet>>();
                if (configSubnets != null)
                {
                    foreach (var office in configSubnets)
                    {
                        if (office.Subnets != null)
                        {
                            foreach (var sub in office.Subnets)
                            {
                                if (SubnetCalculator.IsIpInSubnet(ipAddress, sub))
                                {
                                    return (NetworkLocationType.CorporateOffice, null, null);
                                }
                            }
                        }
                    }
                }
            }

            // 3. Check if explicitly VPN tunnel
            if ((!string.IsNullOrWhiteSpace(ssid) && ssid.Contains("VPN", StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(ipAddress) && (ipAddress.StartsWith("10.8.") || ipAddress.StartsWith("172.20."))))
            {
                return (NetworkLocationType.VPN, null, null);
            }

            // 4. Default to Remote
            if (!string.IsNullOrWhiteSpace(ssid) || !string.IsNullOrWhiteSpace(ipAddress))
            {
                return (NetworkLocationType.Remote, null, null);
            }

            return (NetworkLocationType.Unknown, null, null);
        }
    }
}
