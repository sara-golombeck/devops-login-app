
output "cluster_endpoint" {
  description = "Endpoint for EKS control plane"
  value       = module.eks.cluster_endpoint
}

output "cluster_name" {
  description = "EKS cluster name"
  value       = module.eks.cluster_name
}


output "s3_static_bucket_name" {
  description = "Name of the S3 bucket for static files"
  value       = module.s3_static.s3_bucket_name
}

output "cloudfront_distribution_id" {
  description = "CloudFront Distribution ID"
  value       = module.s3_static.cloudfront_distribution_id
}

output "cloudfront_domain_name" {
  description = "CloudFront Distribution Domain Name"
  value       = module.s3_static.cloudfront_domain_name
}

output "route53_nameservers" {
  description = "Route53 nameservers - Configure these at domain registrar"
  value       = module.route53_zone.name_servers
}

output "custom_domain_url" {
  description = "Custom domain URL (after nameserver setup)"
  value       = module.s3_static.custom_domain_url
}


output "addit_zone_id" {
  description = "Route53 Zone ID for myname.click"
  value       = module.route53_zone.zone_id
}