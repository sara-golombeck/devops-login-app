# modules/eks/variables.tf

variable "cluster_name" {
  description = "EKS cluster name"
  type        = string
}

variable "cluster_version" {
  description = "EKS cluster Kubernetes version"
  type        = string
  default     = "1.28"
}

variable "vpc_id" {
  description = "VPC ID where EKS cluster will be created"
  type        = string
}

variable "private_subnet" {
  description = "Private subnet objects with IDs"
  type = list(object({
    id                = string
    cidr_block        = string
    availability_zone = string
  }))
}

variable "private_subnet_cidrs" {
  description = "CIDR blocks of private subnets for security group rules"
  type        = list(string)
}

variable "public_access_cidrs" {
  description = "CIDR blocks that can access the public API server endpoint"
  type        = list(string)
  default     = ["212.76.117.214/32"]
}

variable "admin_user_arn" {
  description = "ARN of the IAM user for EKS admin access"
  type        = string
}

variable "node_group_config" {
  description = "EKS node group configuration"
  type = object({
    instance_types = list(string)
    capacity_type  = string
    disk_size      = number
    min_size       = number
    max_size       = number
    desired_size   = number
  })
  default = {
    instance_types = ["t3a.medium"]
    capacity_type  = "ON_DEMAND"
    disk_size      = 50
    min_size       = 2
    max_size       = 3
    desired_size   = 2
  }
}

variable "cluster_addons" {
  description = "EKS cluster addons"
  type        = set(string)
  default = [
    "coredns",
    "kube-proxy",
    "vpc-cni"
  ]
}

variable "common_tags" {
  description = "Common tags to apply to all resources"
  type        = map(string)
  default     = {}
}