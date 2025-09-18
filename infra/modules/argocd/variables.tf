variable "argocd_namespace" {
  description = "Namespace for ArgoCD installation"
  type        = string
  default     = "argocd"
}

variable "gitops_repo_url" {
  description = "SSH URL for GitOps repository"
  type        = string
}

variable "github_ssh_secret_name" {
  description = "Name of AWS Secrets Manager secret containing GitHub SSH key"
  type        = string
  default     = "email-service-github-ssh"
}
variable "infra_path" {
  description = "Path to infrastructure applications in GitOps repo"
  type        = string
  default     = "infra-apps"
}

variable "eks_cluster_name" {
  description = "EKS cluster name"
  type        = string
}

variable "aws_region" {
  description = "AWS region"
  type        = string
}