output "auth_service_role_arn" {
  description = "Auth service IAM role ARN"
  value       = aws_iam_role.auth_service.arn
}

output "email_service_role_arn" {
  description = "Email service IAM role ARN"  
  value       = aws_iam_role.email_service.arn
}