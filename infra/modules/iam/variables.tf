variable "project_name" {
  description = "Project name"
  type        = string
}

variable "cluster_oidc_issuer_arn" {
  description = "EKS cluster OIDC issuer ARN"
  type        = string
}

variable "cluster_oidc_issuer_url" {
  description = "EKS cluster OIDC issuer URL"
  type        = string
}

variable "namespace" {
  description = "Kubernetes namespace"
  type        = string
}

variable "sqs_queue_arn" {
  description = "SQS queue ARN"
  type        = string
}

variable "ses_domain_arn" {
  description = "SES domain ARN"
  type        = string
}

variable "common_tags" {
  description = "Common tags"
  type        = map(string)
  default     = {}
}