# Automarkly Email Service Platform

A production-ready, cloud-native email service platform built on AWS EKS with GitOps deployment patterns. This platform provides a scalable, secure, and observable email processing system using microservices architecture.

## 🏗️ Architecture Overview

The platform consists of three main components:
- **Frontend**: React-based web application for email submission
- **Backend API**: .NET 8 REST API for email processing and queuing
- **Email Worker**: Background service for email delivery via AWS SES

### Infrastructure Components

- **AWS EKS**: Kubernetes cluster for container orchestration
- **AWS SQS**: Message queuing for reliable email processing
- **AWS SES**: Email delivery service
- **AWS S3 + CloudFront**: Static website hosting with CDN
- **PostgreSQL**: Database for user management and audit trails
- **ArgoCD**: GitOps continuous deployment
- **Prometheus + Grafana**: Monitoring and observability

## 🚀 Quick Start

### Prerequisites

- AWS CLI configured with appropriate permissions
- Terraform >= 1.0
- kubectl
- Docker
- Helm 3.x

### Infrastructure Deployment

1. **Deploy AWS Infrastructure**:
   ```bash
   cd infra
   terraform init
   terraform plan
   terraform apply
   ```

2. **Configure kubectl**:
   ```bash
   aws eks update-kubeconfig --region <region> --name <cluster-name>
   ```

3. **Deploy Applications via GitOps**:
   ```bash
   cd gitops
   kubectl apply -f argocd/
   ```

### Local Development

1. **Backend Development**:
   ```bash
   cd application/backend
   docker-compose up -d  # Start PostgreSQL
   dotnet run --project src/EmailServiceAPI
   ```

2. **Frontend Development**:
   ```bash
   cd application/frontend
   npm install
   npm start
   ```

3. **Worker Development**:
   ```bash
   cd application/email-worker
   dotnet run --project src/EmailWorker
   ```

## 📁 Project Structure

```
automarkly/
├── application/           # Application source code
│   ├── backend/          # .NET 8 Web API
│   ├── frontend/         # React application
│   ├── email-worker/     # Background email processor
│   └── e2e-email-service/ # End-to-end tests
├── infra/                # Terraform infrastructure code
│   └── modules/          # Reusable Terraform modules
├── gitops/               # GitOps configuration
│   ├── argocd/          # ArgoCD applications
│   └── email-service/   # Helm charts
└── scripts/             # Utility scripts
```

## 🔄 CI/CD Pipeline

The platform implements a comprehensive CI/CD pipeline with:

- **Automated Testing**: Unit tests, integration tests, and E2E tests
- **Multi-Environment**: Staging and production environments
- **GitOps Deployment**: ArgoCD for automated deployments
- **Security Scanning**: Container and dependency vulnerability scanning
- **Monitoring**: Prometheus metrics and Grafana dashboards

### Pipeline Features

- ✅ Automated unit and integration testing
- ✅ Docker image building and scanning
- ✅ Multi-stage deployments (staging → production)
- ✅ GitOps-based deployment with ArgoCD
- ✅ Automated rollbacks on failure
- ✅ Comprehensive monitoring and alerting

## 🛡️ Security Features

- **IRSA (IAM Roles for Service Accounts)**: Fine-grained AWS permissions
- **Network Policies**: Kubernetes network segmentation
- **Secrets Management**: AWS Secrets Manager integration
- **TLS Encryption**: End-to-end encryption with cert-manager
- **Container Security**: Distroless images and security scanning
- **WAF Protection**: AWS WAF for frontend protection

## 📊 Monitoring & Observability

- **Metrics**: Prometheus for metrics collection
- **Visualization**: Grafana dashboards
- **Logging**: Centralized logging with Fluent Bit and Elasticsearch
- **Tracing**: Application performance monitoring
- **Alerting**: Automated alerts for critical issues

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `AWS_REGION` | AWS region | `us-east-1` |
| `CLUSTER_NAME` | EKS cluster name | `automarkly-cluster` |
| `DOMAIN_NAME` | Primary domain | - |
| `DATABASE_URL` | PostgreSQL connection string | - |
| `SQS_QUEUE_URL` | SQS queue URL | - |

### Terraform Variables

Key variables in `infra/terraform.tfvars`:
- `cluster_name`: EKS cluster name
- `vpc_cidr`: VPC CIDR block
- `domain_name`: Domain for SES
- `admin_user_arn`: Admin user ARN for EKS access

## 🧪 Testing

### Running Tests

```bash
# Backend tests
cd application/backend
dotnet test

# Frontend tests
cd application/frontend
npm test

# Worker tests
cd application/email-worker
dotnet test

# E2E tests
cd application/e2e-email-service
python -m pytest
```

### Test Coverage

- Unit tests for all business logic
- Integration tests for API endpoints
- E2E tests for complete user workflows
- Load testing for performance validation

## 📈 Performance

- **Horizontal Scaling**: Auto-scaling based on CPU/memory usage
- **Queue Processing**: Asynchronous email processing with SQS
- **CDN**: CloudFront for global content delivery
- **Database Optimization**: Connection pooling and query optimization
- **Caching**: Redis for session and data caching

## 🚀 Deployment

### Production Deployment

1. **Infrastructure**: Deployed via Terraform
2. **Applications**: Deployed via ArgoCD GitOps
3. **Monitoring**: Automatic setup of observability stack
4. **Security**: Automated certificate management

### Rollback Strategy

- **Blue-Green Deployments**: Zero-downtime deployments
- **Automated Rollbacks**: On health check failures
- **Database Migrations**: Backward-compatible migrations
- **Feature Flags**: Gradual feature rollouts

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

### Development Guidelines

- Follow conventional commit messages
- Maintain test coverage above 80%
- Update documentation for new features
- Follow security best practices

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Check the documentation in each component's README
- Review the troubleshooting guide in the wiki

## 🔗 Related Documentation

- [Backend API Documentation](application/backend/README.md)
- [Frontend Documentation](application/frontend/README.md)
- [Email Worker Documentation](application/email-worker/README.md)
- [Infrastructure Documentation](infra/README.md)
- [GitOps Documentation](gitops/README.md)

---

**Built with ❤️ for scalable email processing**