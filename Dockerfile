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

# Copy both published apps
COPY --from=publish /app/publish/api ./api
COPY --from=publish /app/publish/ui ./ui

# Create directory for uploaded files
RUN mkdir -p /app/ui/wwwroot/digital-resources

# Install supervisor to manage multiple processes
RUN apt-get update && \
    apt-get install -y supervisor && \
    rm -rf /var/lib/apt/lists/*

# Create supervisor config to run both API and UI
RUN printf '[supervisord]\n\
nodaemon=true\n\
user=root\n\
loglevel=info\n\
\n\
[program:api]\n\
command=dotnet /app/api/Tekhnologia.dll --urls http://0.0.0.0:7137\n\
directory=/app/api\n\
autostart=true\n\
autorestart=true\n\
stdout_logfile=/dev/stdout\n\
stdout_logfile_maxbytes=0\n\
stderr_logfile=/dev/stderr\n\
stderr_logfile_maxbytes=0\n\
priority=100\n\
startsecs=10\n\
\n\
# Use a shell wrapper for the UI so the runtime $PORT is expanded by the shell\n\
[program:ui]\n\
command=/bin/sh -c "dotnet /app/ui/Tekhnologia.UI.dll --urls http://0.0.0.0:$PORT"\n\
directory=/app/ui\n\
autostart=true\n\
autorestart=true\n\
stdout_logfile=/dev/stdout\n\
stdout_logfile_maxbytes=0\n\
stderr_logfile=/dev/stderr\n\
stderr_logfile_maxbytes=0\n\
priority=200\n\
startsecs=15\n\
' > /etc/supervisor/conf.d/supervisord.conf

# Expose port (Render will set $PORT)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]