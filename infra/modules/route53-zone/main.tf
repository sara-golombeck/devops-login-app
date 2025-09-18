# Route53 Hosted Zone (not records - External-DNS manages those)
resource "aws_route53_zone" "main" {
  name = var.domain_name
}