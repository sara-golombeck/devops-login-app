output "domain_identity_arn" {
  description = "SES Domain Identity ARN"
  value       = aws_ses_domain_identity.main.arn
}

output "domain_identity_verification_token" {
  description = "Domain verification token"
  value       = aws_ses_domain_identity.main.verification_token
}