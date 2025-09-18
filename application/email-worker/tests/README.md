# Email Worker Tests

Professional unit tests for Email Worker service.

## Test Structure

- **EmailServiceTests** - Email sending via AWS SES
- **EmailWorkerServiceTests** - SQS message processing
- **ModelsTests** - Data models and serialization

## Running Tests

```bash
dotnet test
```