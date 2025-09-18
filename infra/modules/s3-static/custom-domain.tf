# Custom domain configuration
# Note: Using existing zone from route53-zone module

resource "aws_acm_certificate" "cdn" {
  count             = var.use_custom_domain ? 1 : 0
  provider          = aws.us_east_1
  domain_name       = var.domain_name
  validation_method = "DNS"

  lifecycle {
    create_before_destroy = true
  }

  tags = var.common_tags
}

# Certificate validation records
resource "aws_route53_record" "cert_validation" {
  for_each = var.use_custom_domain ? {
    for dvo in aws_acm_certificate.cdn[0].domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  } : {}

  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = var.zone_id
}

# Certificate validation
resource "aws_acm_certificate_validation" "cdn" {
  count           = var.use_custom_domain ? 1 : 0
  provider        = aws.us_east_1
  certificate_arn = aws_acm_certificate.cdn[0].arn
  validation_record_fqdns = [for record in aws_route53_record.cert_validation : record.fqdn]
}

