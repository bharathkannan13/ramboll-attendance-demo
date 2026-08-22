#!/usr/bin/env bash
set -e

echo "=== Installing .NET 8 SDK on Vercel Build Server ==="
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 8.0 --install-dir ./dotnet

export PATH="./dotnet:$PATH"
export DOTNET_ROOT="./dotnet"

echo "=== Verifying dotnet installation ==="
./dotnet/dotnet --version

echo "=== Building & Publishing Enterprise Attendance .NET 8 Application ==="
./dotnet/dotnet publish src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj -c Release -o public

echo "=== Build Completed Successfully! ==="
