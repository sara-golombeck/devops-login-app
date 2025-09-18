# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first for better caching
COPY src/EmailWorker/EmailWorker.csproj .
RUN dotnet restore "EmailWorker.csproj"

# Copy source and build
COPY src/EmailWorker/ .
RUN dotnet publish "EmailWorker.csproj" -c Release -o /app/publish --no-restore

# Runtime stage  
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published app
COPY --from=build /app/publish .

# Change ownership and switch to non-root user
RUN chown -R appuser:appuser /app
USER appuser
EXPOSE 8080

# Set environment
ENV DOTNET_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "EmailWorker.dll"]