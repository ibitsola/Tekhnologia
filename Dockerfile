# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Tekhnologia.sln", "./"]
COPY ["Tekhnologia/Tekhnologia.csproj", "Tekhnologia/"]
COPY ["Tekhnologia.UI/Tekhnologia.UI.csproj", "Tekhnologia.UI/"]
COPY ["Tekhnologia.Tests/Tekhnologia.Tests.csproj", "Tekhnologia.Tests/"]

# Restore dependencies
RUN dotnet restore "Tekhnologia.sln"

# Copy all source code
COPY . .

# Build BOTH projects
WORKDIR "/src/Tekhnologia"
RUN dotnet build "Tekhnologia.csproj" -c Release -o /app/build/api

WORKDIR "/src/Tekhnologia.UI"
RUN dotnet build "Tekhnologia.UI.csproj" -c Release -o /app/build/ui

# Stage 2: Publish
FROM build AS publish
WORKDIR "/src/Tekhnologia"
RUN dotnet publish "Tekhnologia.csproj" -c Release -o /app/publish/api /p:UseAppHost=false

WORKDIR "/src/Tekhnologia.UI"
RUN dotnet publish "Tekhnologia.UI.csproj" -c Release -o /app/publish/ui /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy both published apps
COPY --from=publish /app/publish/api ./api
COPY --from=publish /app/publish/ui ./ui

# Copy startup script
COPY start.sh /app/start.sh
RUN chmod +x /app/start.sh

# Create directory for uploaded files
RUN mkdir -p /app/ui/wwwroot/digital-resources

# Expose port (Render will set $PORT)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["/app/start.sh"]