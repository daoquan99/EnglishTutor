#!/usr/bin/env bash
set -e
echo "Starting .NET 10 Backend on http://localhost:5000 ..."
(cd backend && dotnet run --urls "http://localhost:5000") &
BE_PID=$!

echo "Starting React + Vite Frontend on http://localhost:5173 ..."
(cd frontend && npm install && npm run dev)

kill $BE_PID 2>/dev/null || true
