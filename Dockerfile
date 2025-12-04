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

# Build the UI project (which depends on the API project)
WORKDIR "/src/Tekhnologia.UI"
RUN dotnet build "Tekhnologia.UI.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Tekhnologia.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create directory for uploaded files
RUN mkdir -p /app/wwwroot/digital-resources

# Expose port (Render will set $PORT)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Tekhnologia.UI.dll"]