variable "project_name" {
  description = "Name of the project"
  type        = string
}

variable "common_tags" {
  description = "Common tags to apply to all resources"
  type        = map(string)
  default     = {}
}

variable "aws_region" {
  description = "AWS region"
  type        = string
}

variable "use_custom_domain" {
  description = "Whether to use custom domain"
  type        = bool
  default     = false
}

variable "domain_name" {
  description = "Domain name for custom domain"
  type        = string
  default     = null
}

variable "zone_id" {
  description = "Route53 zone ID for DNS records"
  type        = string
  default     = null
}

variable "enable_waf" {
  description = "Whether to enable WAF for CloudFront"
  type        = bool
  default     = true
}



