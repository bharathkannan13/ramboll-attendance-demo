# Use official .NET 8 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/EnterpriseAttendance.Core/EnterpriseAttendance.Core.csproj", "src/EnterpriseAttendance.Core/"]
COPY ["src/EnterpriseAttendance.Infrastructure/EnterpriseAttendance.Infrastructure.csproj", "src/EnterpriseAttendance.Infrastructure/"]
COPY ["src/EnterpriseAttendance.Services/EnterpriseAttendance.Services.csproj", "src/EnterpriseAttendance.Services/"]
COPY ["src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj", "src/EnterpriseAttendance.Web/"]
RUN dotnet restore "src/EnterpriseAttendance.Web/EnterpriseAttendance.Web.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/src/EnterpriseAttendance.Web"
RUN dotnet publish "EnterpriseAttendance.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "EnterpriseAttendance.Web.dll"]
