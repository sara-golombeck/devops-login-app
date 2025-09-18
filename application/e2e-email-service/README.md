# End-to-End Testing Service

Python-based end-to-end testing framework for comprehensive email service validation.

## Features

- **Full Workflow Testing**: Complete user journey validation
- **API Integration Testing**: Backend endpoint verification
- **Frontend Testing**: UI interaction and response validation
- **Email Delivery Testing**: SES integration verification
- **Cross-Service Testing**: Multi-component workflow validation

## Quick Start

```bash
cd application/e2e-email-service
pip install -r requirements.txt
python -m pytest
```

## Test Structure

- **Frontend Tests**: User interface interaction testing
- **Integration Tests**: API endpoint and service integration
- **Email Flow Tests**: Complete email processing workflow

## External Repository Integration

This E2E testing service is designed as a reusable component that can be integrated into multiple projects. The test framework is maintained as an external repository and imported into various email service implementations for consistent testing across different environments.

## Configuration

```python
# Test configuration
BASE_URL = "https://api.myname.click"
FRONTEND_URL = "https://myname.click"
TEST_EMAIL = "test@example.com"
```

## System Integration

1. **Frontend Testing**: Validates React application functionality
2. **API Testing**: Tests Backend API endpoints and responses
3. **Workflow Testing**: End-to-end email processing validation
4. **Cross-Service**: Multi-component integration verification

---

**Developed by Sara**