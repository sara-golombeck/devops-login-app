# Provider for CloudFront certificates (must be us-east-1)
provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"
}

# S3 bucket for static files
resource "aws_s3_bucket" "static_files" {
  bucket = "${var.project_name}-static-files"
  
  tags = var.common_tags
}

# S3 bucket versioning
resource "aws_s3_bucket_versioning" "static_files" {
  bucket = aws_s3_bucket.static_files.id
  versioning_configuration {
    status = "Enabled"
  }
}

# S3 bucket public access block
resource "aws_s3_bucket_public_access_block" "static_files" {
  bucket = aws_s3_bucket.static_files.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# CloudFront Origin Access Control
resource "aws_cloudfront_origin_access_control" "static_files" {
  name                              = "${var.project_name}-static-files-oac"
  description                       = "OAC for static files bucket"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

# CloudFront Distribution
resource "aws_cloudfront_distribution" "static_files" {
  origin {
    domain_name              = aws_s3_bucket.static_files.bucket_regional_domain_name
    origin_access_control_id = aws_cloudfront_origin_access_control.static_files.id
    origin_id                = "S3-${aws_s3_bucket.static_files.bucket}"
  }



  enabled             = true
  is_ipv6_enabled     = true
  default_root_object = "index.html"

  default_cache_behavior {
    allowed_methods  = ["DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT"]
    cached_methods   = ["GET", "HEAD"]
    target_origin_id = "S3-${aws_s3_bucket.static_files.bucket}"

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 3600
    max_ttl                = 86400
  }

  # Cache behavior for static assets
  ordered_cache_behavior {
    path_pattern     = "/static/*"
    allowed_methods  = ["GET", "HEAD", "OPTIONS"]
    cached_methods   = ["GET", "HEAD", "OPTIONS"]
    target_origin_id = "S3-${aws_s3_bucket.static_files.bucket}"

    forwarded_values {
      query_string = false
      headers      = ["Origin"]
      cookies {
        forward = "none"
      }
    }

    min_ttl                = 0
    default_ttl            = 86400
    max_ttl                = 31536000
    compress               = true
    viewer_protocol_policy = "redirect-to-https"
  }



  price_class = "PriceClass_100"

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    acm_certificate_arn      = var.use_custom_domain ? aws_acm_certificate_validation.cdn[0].certificate_arn : null
    ssl_support_method       = var.use_custom_domain ? "sni-only" : null
    minimum_protocol_version = var.use_custom_domain ? "TLSv1.2_2021" : null
    cloudfront_default_certificate = var.use_custom_domain ? false : true
  }

  aliases = var.use_custom_domain ? [var.domain_name] : []

  web_acl_id = var.enable_waf ? aws_wafv2_web_acl.cloudfront[0].arn : null

  tags = var.common_tags
}



# S3 bucket policy for CloudFront
resource "aws_s3_bucket_policy" "static_files" {
  bucket = aws_s3_bucket.static_files.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "AllowCloudFrontServicePrincipal"
        Effect    = "Allow"
        Principal = {
          Service = "cloudfront.amazonaws.com"
        }
        Action   = "s3:GetObject"
        Resource = "${aws_s3_bucket.static_files.arn}/*"
        Condition = {
          StringEquals = {
            "AWS:SourceArn" = aws_cloudfront_distribution.static_files.arn
          }
        }
      }
    ]
  })
}

resource "aws_iam_policy" "jenkins_s3_upload" {
  name        = "${var.project_name}-jenkins-s3-upload"
  description = "Policy for Jenkins to upload static files to S3"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "s3:PutObject",
          "s3:PutObjectAcl",
          "s3:GetObject",
          "s3:DeleteObject",
          "s3:ListBucket"
        ]
        Resource = [
          aws_s3_bucket.static_files.arn,
          "${aws_s3_bucket.static_files.arn}/*"
        ]
      },
      {
        Effect = "Allow"
        Action = [
          "cloudfront:CreateInvalidation"
        ]
        Resource = aws_cloudfront_distribution.static_files.arn
      }
    ]
  })

  tags = var.common_tags
}