using System;
using FluentAssertions;
using Xunit;
using EnterpriseAttendance.Services.Engine;

namespace EnterpriseAttendance.Tests.EngineTests
{
    public class SubnetCalculatorTests
    {
        [Theory]
        [InlineData("10.100.45.23", "10.100.0.0/16", true)]
        [InlineData("10.100.1.1", "10.100.0.0/16", true)]
        [InlineData("10.101.5.10", "10.100.0.0/16", false)]
        [InlineData("192.168.1.50", "192.168.1.0/24", true)]
        [InlineData("192.168.2.50", "192.168.1.0/24", false)]
        [InlineData("10.5.10.15", "10.0.0.0/8", true)]
        [InlineData("11.5.10.15", "10.0.0.0/8", false)]
        [InlineData("10.100.45.23", "10.100.*", true)]
        [InlineData("10.101.45.23", "10.100.*", false)]
        public void IsIpInSubnet_VariousSubnets_ReturnsExpectedMatch(string ipAddress, string subnet, bool expectedMatch)
        {
            // Act
            bool result = SubnetCalculator.IsIpInSubnet(ipAddress, subnet);

            // Assert
            result.Should().Be(expectedMatch);
        }

        [Fact]
        public void IsIpInSubnet_InvalidInputs_ReturnsFalseSafely()
        {
            SubnetCalculator.IsIpInSubnet("invalid-ip", "10.100.0.0/16").Should().BeFalse();
            SubnetCalculator.IsIpInSubnet("10.100.1.1", "invalid-subnet").Should().BeFalse();
            SubnetCalculator.IsIpInSubnet("", "").Should().BeFalse();
        }
    }
}
