# Multi-stage build for HomeContentLock
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

# Copy solution and project files
COPY *.slnx ./
COPY src/ ./src/
COPY tests/ ./tests/

# Restore and build
RUN dotnet restore
RUN dotnet build -c Release --no-restore

# Run tests
RUN dotnet test -c Release --no-build --logger="console;verbosity=minimal"

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine

WORKDIR /app

# Copy built binaries from builder
COPY --from=builder /src/src/HomeContentLock.Presentation/bin/Release/net10.0/ ./

# Set entrypoint
ENTRYPOINT ["dotnet", "HomeContentLock.Presentation.dll"]
CMD ["--help"]
