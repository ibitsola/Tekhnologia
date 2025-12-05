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

# Stage 3: Runtime - single ASP.NET process (API) that also serves the UI static files
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy API published app
COPY --from=publish /app/publish/api ./api

# Copy UI published static files into the API's wwwroot so API serves the UI
RUN mkdir -p /app/api/wwwroot
COPY --from=publish /app/publish/ui/wwwroot/ ./api/wwwroot/

# Create directory for uploaded files inside API wwwroot
RUN mkdir -p /app/api/wwwroot/digital-resources

# Expose port; Render will provide $PORT at runtime
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Start only the API process; use shell so $PORT is expanded by the shell
CMD ["/bin/sh", "-c", "dotnet /app/api/Tekhnologia.dll --urls http://0.0.0.0:$PORT"]