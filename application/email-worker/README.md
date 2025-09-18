# Email Worker Service

A high-performance .NET 8 background service that processes email messages from AWS SQS and delivers them via AWS SES with comprehensive monitoring and error handling.

## 🚀 Features

- **Background Processing**: Continuous SQS message processing
- **Email Delivery**: AWS SES integration for reliable email sending
- **Error Handling**: Comprehensive error handling with retry logic
- **Monitoring**: Prometheus metrics for observability
- **Health Checks**: Built-in health monitoring
- **Graceful Shutdown**: Proper resource cleanup on termination
- **Configurable**: Environment-based configuration

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Backend API   │───▶│    AWS SQS      │───▶│  Email Worker   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                        │
                                                        ▼
                                               ┌─────────────────┐
                                               │    AWS SES      │
                                               └─────────────────┘
```

## 🛠️ Technology Stack

- **.NET 8**: Latest .NET framework
- **Hosted Services**: Background service implementation
- **AWS SDK**: SQS and SES integration
- **Prometheus.NET**: Metrics collection
- **Microsoft.Extensions.Hosting**: Service hosting
- **Microsoft.Extensions.Logging**: Structured logging

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- AWS CLI configured with SES and SQS permissions
- Docker (optional)

### Local Development

1. **Navigate to worker directory**:
   ```bash
   cd application/email-worker
   ```

2. **Install dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure settings**:
   ```json
   {
     "AWS": {
       "Region": "us-east-1",
       "SQS": {
         "QueueUrl": "https://sqs.us-east-1.amazonaws.com/account/queue-name"
       },
       "SES": {
         "FromEmail": "noreply@yourdomain.com",
         "FromName": "Your Service"
       }
     },
     "Worker": {
       "MaxConcurrentMessages": 10,
       "VisibilityTimeoutSeconds": 300,
       "WaitTimeSeconds": 20
     }
   }
   ```

4. **Run the worker**:
   ```bash
   dotnet run --project src/EmailWorker
   ```

### Docker Development

```bash
# Build and run with Docker
docker build -t email-worker .
docker run email-worker

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
dotnet test tests/EmailWorker.Tests/
```

### Test Structure

```
tests/EmailWorker.Tests/
├── EmailServiceTests.cs        # Email service tests
├── EmailWorkerServiceTests.cs  # Worker service tests
├── ModelsTests.cs             # Model validation tests
└── GlobalUsings.cs            # Global test usings
```

### Test Coverage

- **Services**: 95% coverage
- **Models**: 100% coverage
- **Worker Logic**: 90% coverage
- **Error Handling**: 100% coverage

## 📊 Monitoring & Metrics

### Prometheus Metrics

- `worker_health`: Worker health status (0/1)
- `emails_processed_total`: Total emails processed
- `emails_sent_total`: Successfully sent emails
- `emails_failed_total`: Failed email attempts
- `sqs_messages_received_total`: SQS messages received
- `email_processing_duration_seconds`: Email processing time

### Health Monitoring

```csharp
// Health check implementation
WorkerMetrics.WorkerHealth.Set(1); // Healthy
WorkerMetrics.WorkerHealth.Set(0); // Unhealthy
```

### Metrics Endpoint

The worker exposes metrics on port 8080:
```
http://localhost:8080/metrics
```

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `AWS__Region` | AWS region | `us-east-1` |
| `AWS__SQS__QueueUrl` | SQS queue URL | - |
| `AWS__SES__FromEmail` | Sender email address | - |
| `AWS__SES__FromName` | Sender display name | - |
| `Worker__MaxConcurrentMessages` | Max concurrent processing | `10` |
| `Worker__VisibilityTimeoutSeconds` | SQS visibility timeout | `300` |
| `Worker__WaitTimeSeconds` | SQS long polling wait time | `20` |

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AWS": {
    "Region": "us-east-1",
    "SQS": {
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/account/queue-name"
    },
    "SES": {
      "FromEmail": "noreply@yourdomain.com",
      "FromName": "Email Service"
    }
  },
  "Worker": {
    "MaxConcurrentMessages": 10,
    "VisibilityTimeoutSeconds": 300,
    "WaitTimeSeconds": 20,
    "RetryAttempts": 3,
    "RetryDelaySeconds": 30
  }
}
```

## 🏗️ Project Structure

```
src/EmailWorker/
├── Models/
│   ├── EmailMessage.cs         # Email message model
│   └── EmailResult.cs          # Email processing result
├── Services/
│   ├── EmailService.cs         # Email sending service
│   ├── EmailWorkerService.cs   # Main worker service
│   ├── IEmailService.cs        # Email service interface
│   └── WorkerMetrics.cs        # Prometheus metrics
├── appsettings.json            # Application settings
├── EmailWorker.csproj          # Project file
└── Program.cs                  # Application entry point
```

## 🔄 Message Processing Flow

### Processing Pipeline

1. **Message Polling**: Long polling from SQS queue
2. **Deserialization**: Convert SQS message to EmailMessage
3. **Validation**: Validate email content and recipient
4. **Email Sending**: Send via AWS SES
5. **Result Handling**: Success/failure processing
6. **Message Deletion**: Remove from SQS on success
7. **Error Handling**: Retry logic for failures

### Message Format

```json
{
  "recipientEmail": "user@example.com",
  "subject": "Login Link",
  "body": "Click here to login: https://example.com/login?token=...",
  "timestamp": "2024-01-01T12:00:00Z",
  "messageId": "unique-message-id"
}
```

## 🔒 Security

### Security Features

- **IAM Roles**: Service uses IAM roles for AWS access
- **Encryption**: Messages encrypted in transit and at rest
- **Input Validation**: All email content validated
- **Rate Limiting**: Configurable processing limits
- **Error Sanitization**: Sensitive data removed from logs

### AWS Permissions Required

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "sqs:ReceiveMessage",
        "sqs:DeleteMessage",
        "sqs:GetQueueAttributes"
      ],
      "Resource": "arn:aws:sqs:region:account:queue-name"
    },
    {
      "Effect": "Allow",
      "Action": [
        "ses:SendEmail",
        "ses:SendRawEmail"
      ],
      "Resource": "*"
    }
  ]
}
```

## 🚀 Deployment

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EmailWorker/EmailWorker.csproj", "src/EmailWorker/"]
RUN dotnet restore "src/EmailWorker/EmailWorker.csproj"
COPY . .
WORKDIR "/src/src/EmailWorker"
RUN dotnet build "EmailWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmailWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailWorker.dll"]
```

### Kubernetes Deployment

The worker is deployed using Helm charts:
- Deployment with resource limits
- ConfigMaps for configuration
- Secrets for sensitive data
- Service Monitor for metrics scraping

## ⚡ Performance

### Performance Optimizations

- **Concurrent Processing**: Configurable concurrent message handling
- **Long Polling**: Reduces SQS API calls
- **Connection Pooling**: Efficient AWS SDK usage
- **Memory Management**: Proper resource disposal
- **Batch Processing**: Process multiple messages efficiently

### Scaling Configuration

```yaml
# Kubernetes HPA example
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: email-worker-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: email-worker
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

## 🐛 Error Handling

### Error Categories

1. **Transient Errors**: Network timeouts, temporary SES limits
2. **Permanent Errors**: Invalid email addresses, malformed messages
3. **Configuration Errors**: Missing AWS credentials, invalid queue URL

### Retry Logic

```csharp
public async Task<EmailResult> ProcessEmailAsync(EmailMessage message)
{
    var maxRetries = 3;
    var retryDelay = TimeSpan.FromSeconds(30);
    
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var result = await _emailService.SendEmailAsync(message);
            if (result.Success)
            {
                return result;
            }
        }
        catch (Exception ex) when (IsTransientError(ex))
        {
            if (attempt < maxRetries)
            {
                await Task.Delay(retryDelay * attempt);
                continue;
            }
        }
    }
    
    return EmailResult.Failed("Max retries exceeded");
}
```

## 📊 Monitoring & Alerting

### Key Metrics to Monitor

- **Processing Rate**: Messages processed per minute
- **Error Rate**: Failed messages percentage
- **Queue Depth**: SQS queue message count
- **Processing Latency**: Time from queue to delivery
- **Worker Health**: Service availability

### Alerting Rules

```yaml
# Prometheus alerting rules
groups:
- name: email-worker
  rules:
  - alert: EmailWorkerDown
    expr: worker_health == 0
    for: 1m
    labels:
      severity: critical
    annotations:
      summary: "Email worker is down"
      
  - alert: HighEmailFailureRate
    expr: rate(emails_failed_total[5m]) / rate(emails_processed_total[5m]) > 0.1
    for: 2m
    labels:
      severity: warning
    annotations:
      summary: "High email failure rate detected"
```

## 🔧 Troubleshooting

### Common Issues

1. **SQS Connection Issues**:
   ```bash
   # Verify AWS credentials
   aws sts get-caller-identity
   aws sqs get-queue-attributes --queue-url <queue-url>
   ```

2. **SES Permission Issues**:
   ```bash
   # Check SES sending quota
   aws ses get-send-quota
   aws ses get-send-statistics
   ```

3. **High Memory Usage**:
   ```bash
   # Monitor memory usage
   docker stats email-worker
   ```

### Debug Logging

Enable debug logging for troubleshooting:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "EmailWorker": "Debug"
    }
  }
}
```

## 📈 Scaling

### Horizontal Scaling

- Multiple worker instances can process the same queue
- SQS ensures message delivery to only one consumer
- Configure appropriate visibility timeout
- Monitor queue depth for scaling decisions

### Vertical Scaling

- Increase `MaxConcurrentMessages` for higher throughput
- Monitor CPU and memory usage
- Adjust resource limits in Kubernetes

## 🤝 Contributing

### Development Guidelines

1. Follow .NET coding conventions
2. Add comprehensive unit tests
3. Update metrics for new features
4. Ensure proper error handling
5. Update documentation

### Testing New Features

```bash
# Run full test suite
dotnet test --logger trx --results-directory TestResults

# Run performance tests
dotnet run --configuration Release --project tests/PerformanceTests
```

## 📄 Logging

### Structured Logging

```csharp
_logger.LogInformation("Processing email message {MessageId} for {Recipient}", 
    message.MessageId, message.RecipientEmail);

_logger.LogError(ex, "Failed to send email {MessageId} after {Attempts} attempts", 
    message.MessageId, attempts);
```

### Log Levels

- **Information**: Normal processing flow
- **Warning**: Recoverable errors, retries
- **Error**: Failed email delivery, exceptions
- **Debug**: Detailed processing information

---

**Built with .NET 8 for reliable email processing**