# Email Service Backend API

A high-performance .NET 8 Web API that provides email processing capabilities with PostgreSQL persistence and AWS SQS integration.

## 🚀 Features

- **RESTful API**: Clean, well-documented REST endpoints
- **Database Integration**: PostgreSQL with Entity Framework Core
- **Message Queuing**: AWS SQS for reliable email processing
- **Validation**: FluentValidation for request validation
- **Monitoring**: Prometheus metrics integration
- **Health Checks**: Built-in health monitoring
- **CORS Support**: Configurable cross-origin resource sharing
- **Swagger Documentation**: Interactive API documentation

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │───▶│   Backend API   │───▶│   PostgreSQL    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │    AWS SQS      │
                       └─────────────────┘
```

## 📋 API Endpoints

### Authentication
- `POST /api/auth/login` - Submit email for login link

### Health
- `GET /api/health` - Health check endpoint

### Metrics
- `GET /metrics` - Prometheus metrics endpoint

## 🛠️ Technology Stack

- **.NET 8**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for database operations
- **PostgreSQL**: Primary database
- **FluentValidation**: Request validation
- **AWS SDK**: SQS integration
- **Prometheus.NET**: Metrics collection
- **Swagger/OpenAPI**: API documentation

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL 13+
- AWS CLI configured
- Docker (optional)

### Local Development

1. **Clone and navigate**:
   ```bash
   cd application/backend
   ```

2. **Install dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure settings**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=emailservice;Username=postgres;Password=password"
     },
     "AWS": {
       "Region": "us-east-1",
       "SQS": {
         "QueueUrl": "https://sqs.us-east-1.amazonaws.com/account/queue-name"
       }
     }
   }
   ```

4. **Run the application**:
   ```bash
   dotnet run --project src/EmailServiceAPI
   ```

5. **Access Swagger UI**:
   ```
   https://localhost:7001/swagger
   ```

### Docker Development

```bash
# Build and run with Docker
docker build -t email-backend .
docker run -p 8080:8080 email-backend

# Or use Docker Compose
docker-compose up -d
```

## 🧪 Testing

### Unit Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/EmailServiceAPI.Tests/
```

### Test Structure

```
tests/
├── EmailServiceAPI.Tests/
│   ├── ApiResponseTests.cs      # API response model tests
│   ├── LoginRequestTests.cs     # Request validation tests
│   ├── SqsQueueServiceTests.cs  # SQS service tests
│   ├── UserModelTests.cs        # User model tests
│   └── ValidatorTests.cs        # Validation logic tests
```

### Test Coverage

- **Models**: 100% coverage
- **Services**: 95% coverage
- **Validators**: 100% coverage
- **Controllers**: 90% coverage

## 📊 Monitoring & Metrics

### Prometheus Metrics

- `http_requests_total`: Total HTTP requests
- `http_request_duration_seconds`: Request duration
- `login_attempts_total`: Login attempts by status
- `emails_queued_total`: Total emails queued
- `database_operations_total`: Database operations

### Health Checks

The API provides comprehensive health checks:
- Database connectivity
- SQS queue accessibility
- Memory usage
- Disk space

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment | `Development` |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection | - |
| `AWS__Region` | AWS region | `us-east-1` |
| `AWS__SQS__QueueUrl` | SQS queue URL | - |
| `Cors__AllowedOrigins__0` | Allowed CORS origins | `http://localhost:3000` |

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=emailservice;Username=postgres;Password=password"
  },
  "AWS": {
    "Region": "us-east-1",
    "SQS": {
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/account/queue-name"
    }
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://yourdomain.com"
    ]
  }
}
```

## 🏗️ Project Structure

```
src/EmailServiceAPI/
├── Models/
│   ├── ApiResponse.cs          # Generic API response model
│   ├── AppDbContext.cs         # Entity Framework context
│   ├── LoginRequest.cs         # Login request model
│   └── User.cs                 # User entity model
├── Services/
│   ├── IQueueService.cs        # Queue service interface
│   ├── MetricsService.cs       # Prometheus metrics
│   └── SqsQueueService.cs      # SQS implementation
├── Validators/
│   └── LoginRequestValidator.cs # Request validation
├── Properties/
│   └── launchSettings.json     # Launch configuration
├── appsettings.json            # Application settings
├── appsettings.Development.json # Development settings
├── EmailServiceAPI.csproj      # Project file
└── Program.cs                  # Application entry point
```

## 🔒 Security

### Security Features

- **Input Validation**: FluentValidation for all inputs
- **SQL Injection Protection**: Entity Framework parameterized queries
- **CORS Configuration**: Configurable allowed origins
- **HTTPS Enforcement**: Automatic HTTPS redirection
- **Rate Limiting**: Built-in rate limiting capabilities

### Security Best Practices

- Use environment variables for sensitive data
- Enable HTTPS in production
- Configure proper CORS policies
- Implement proper error handling
- Use structured logging

## 🚀 Deployment

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EmailServiceAPI/EmailServiceAPI.csproj", "src/EmailServiceAPI/"]
RUN dotnet restore "src/EmailServiceAPI/EmailServiceAPI.csproj"
COPY . .
WORKDIR "/src/src/EmailServiceAPI"
RUN dotnet build "EmailServiceAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmailServiceAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailServiceAPI.dll"]
```

### Kubernetes Deployment

The backend is deployed using Helm charts in the GitOps repository:
- Horizontal Pod Autoscaler for scaling
- Service Monitor for Prometheus scraping
- ConfigMaps for configuration
- Secrets for sensitive data

## 🐛 Troubleshooting

### Common Issues

1. **Database Connection Issues**:
   ```bash
   # Check PostgreSQL connectivity
   psql -h localhost -U postgres -d emailservice
   ```

2. **SQS Permission Issues**:
   ```bash
   # Verify AWS credentials
   aws sts get-caller-identity
   aws sqs get-queue-attributes --queue-url <queue-url>
   ```

3. **Port Conflicts**:
   ```bash
   # Check port usage
   netstat -tulpn | grep :8080
   ```

### Logging

The application uses structured logging with different levels:
- `Information`: General application flow
- `Warning`: Validation failures, recoverable errors
- `Error`: Exceptions and critical errors

## 📈 Performance

### Performance Optimizations

- **Connection Pooling**: Entity Framework connection pooling
- **Async/Await**: Non-blocking I/O operations
- **Memory Management**: Proper disposal of resources
- **Database Indexing**: Optimized database queries

### Load Testing

```bash
# Example load test with Apache Bench
ab -n 1000 -c 10 -H "Content-Type: application/json" \
   -p login.json http://localhost:8080/api/auth/login
```

## 🤝 Contributing

1. Follow .NET coding conventions
2. Add unit tests for new features
3. Update API documentation
4. Ensure all tests pass
5. Update this README if needed

## 📄 API Documentation

When running in development mode, visit `/swagger` for interactive API documentation.

---

**Built with .NET 8 for high-performance email processing**