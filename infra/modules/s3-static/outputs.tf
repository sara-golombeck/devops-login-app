# S3 Static Module Outputs

output "s3_bucket_name" {
  description = "Name of the S3 bucket"
  value       = aws_s3_bucket.static_files.bucket
}

output "cloudfront_distribution_id" {
  description = "CloudFront Distribution ID"
  value       = aws_cloudfront_distribution.static_files.id
}

output "cloudfront_domain_name" {
  description = "CloudFront Distribution Domain Name"
  value       = aws_cloudfront_distribution.static_files.domain_name
}



output "custom_domain_url" {
  description = "Custom domain URL"
  value       = var.use_custom_domain ? "https://static.${var.domain_name}" : "https://${aws_cloudfront_distribution.static_files.domain_name}"
}

output "jenkins_s3_policy_arn" {
  description = "ARN of IAM policy for Jenkins S3 access"
  value       = aws_iam_policy.jenkins_s3_upload.arn
}

output "cloudfront_zone_id" {
  description = "CloudFront distribution zone ID"
  value       = aws_cloudfront_distribution.static_files.hosted_zone_id
}