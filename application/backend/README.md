# Email Service Backend API

.NET 8 Minimal API for email processing with PostgreSQL and AWS SQS integration.

![Backend & Worker CI](../../diagrams/back_and_worker_ci.png)

## Features

- RESTful API with PostgreSQL persistence
- AWS SQS message queuing
- Prometheus metrics integration
- FluentValidation request validation
- Health checks and monitoring
- Jenkins CI/CD pipeline integration
- Docker containerization with multi-stage builds

## Quick Start

```bash
cd application/backend
dotnet restore
dotnet run --project src/EmailServiceAPI
```

## API Endpoints

- `POST /api/auth/login` - Submit email for login
- `GET /api/health` - Health check
- `GET /metrics` - Prometheus metrics

## Configuration

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

## Testing

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
```

## System Integration

1. **Request Processing**: Validates email requests from Frontend
2. **Database Operations**: Stores user data in PostgreSQL
3. **Queue Publishing**: Sends messages to AWS SQS for worker processing
4. **Metrics Export**: Exposes Prometheus metrics for monitoring

---

**Developed by Sara**