#!/bin/sh
set -e

echo "Starting Tekhnologia backend API..."
cd /app/api
dotnet Tekhnologia.dll --urls "http://0.0.0.0:7137" &
API_PID=$!
echo "Backend API started with PID $API_PID"

sleep 5

echo "Starting Tekhnologia UI..."
cd /app/ui
exec dotnet Tekhnologia.UI.dll --urls "http://0.0.0.0:${PORT:-8080}"
