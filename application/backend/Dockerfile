# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for dependency caching
COPY src/EmailServiceAPI/*.csproj src/EmailServiceAPI/
COPY tests/EmailServiceAPI.Tests/*.csproj tests/EmailServiceAPI.Tests/
COPY *.sln .

# Restore dependencies
RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish src/EmailServiceAPI -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080
# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/api/health || exit 1

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080


# Start the application
ENTRYPOINT ["dotnet", "EmailServiceAPI.dll"]
