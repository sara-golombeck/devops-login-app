variable "domain_name" {
  description = "Domain name for SES"
  type        = string
}

variable "common_tags" {
  description = "Common tags"
  type        = map(string)
  default     = {}
}