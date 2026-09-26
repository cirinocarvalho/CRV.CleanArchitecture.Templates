# The SPA is a bucket of static files behind CloudFront. The bucket stays fully private:
# CloudFront reaches it through an Origin Access Control, and the bucket policy trusts
# only this distribution.

locals {
  ui_name = "${var.project}-${var.environment}-ui"
}

resource "aws_s3_bucket" "ui" {
  bucket = local.ui_name
}

resource "aws_s3_bucket_public_access_block" "ui" {
  bucket                  = aws_s3_bucket.ui.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_server_side_encryption_configuration" "ui" {
  bucket = aws_s3_bucket.ui.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

# Origin Access Control, not the older Origin Access Identity: OAI is superseded and does
# not support SigV4-only regions.
resource "aws_cloudfront_origin_access_control" "ui" {
  name                              = local.ui_name
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

data "aws_cloudfront_cache_policy" "optimized" {
  name = "Managed-CachingOptimized"
}

resource "aws_cloudfront_distribution" "ui" {
  enabled             = true
  is_ipv6_enabled     = true
  default_root_object = "index.html"
  aliases             = var.ui_aliases
  price_class         = "PriceClass_100"

  origin {
    domain_name              = aws_s3_bucket.ui.bucket_regional_domain_name
    origin_id                = local.ui_name
    origin_access_control_id = aws_cloudfront_origin_access_control.ui.id
  }

  default_cache_behavior {
    allowed_methods        = ["GET", "HEAD"]
    cached_methods         = ["GET", "HEAD"]
    target_origin_id       = local.ui_name
    viewer_protocol_policy = "redirect-to-https"
    compress               = true
    cache_policy_id        = data.aws_cloudfront_cache_policy.optimized.id
  }

  # Vue Router runs in history mode, so /admin is a route rather than an object. A private
  # bucket answers a missing key with 403 (not 404), so both have to be mapped back to the
  # app shell or deep links would break on refresh.
  custom_error_response {
    error_code            = 403
    response_code         = 200
    response_page_path    = "/index.html"
    error_caching_min_ttl = 0
  }

  custom_error_response {
    error_code            = 404
    response_code         = 200
    response_page_path    = "/index.html"
    error_caching_min_ttl = 0
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  # An ACM certificate is only usable once alternate domain names are set, so fall back to
  # the default *.cloudfront.net certificate until both are supplied.
  dynamic "viewer_certificate" {
    for_each = length(var.ui_aliases) > 0 && var.certificate_arn != "" ? [1] : []

    content {
      acm_certificate_arn      = var.certificate_arn
      ssl_support_method       = "sni-only"
      minimum_protocol_version = "TLSv1.2_2021"
    }
  }

  dynamic "viewer_certificate" {
    for_each = length(var.ui_aliases) > 0 && var.certificate_arn != "" ? [] : [1]

    content {
      cloudfront_default_certificate = true
    }
  }
}

data "aws_iam_policy_document" "ui" {
  statement {
    actions   = ["s3:GetObject"]
    resources = ["${aws_s3_bucket.ui.arn}/*"]

    principals {
      type        = "Service"
      identifiers = ["cloudfront.amazonaws.com"]
    }

    # Without this condition the policy would let *any* CloudFront distribution read the
    # bucket, not just ours.
    condition {
      test     = "StringEquals"
      variable = "AWS:SourceArn"
      values   = [aws_cloudfront_distribution.ui.arn]
    }
  }
}

resource "aws_s3_bucket_policy" "ui" {
  bucket = aws_s3_bucket.ui.id
  policy = data.aws_iam_policy_document.ui.json
}

resource "aws_route53_record" "ui" {
  count = var.dns_zone_id_ui == "" || var.ui_domain_name == "" ? 0 : 1

  zone_id = var.dns_zone_id_ui
  name    = var.ui_domain_name
  type    = "A"

  alias {
    name                   = aws_cloudfront_distribution.ui.domain_name
    zone_id                = aws_cloudfront_distribution.ui.hosted_zone_id
    evaluate_target_health = false
  }
}

output "ui_bucket" {
  description = "Bucket the UI workflow syncs the built SPA into."
  value       = aws_s3_bucket.ui.id
}

output "ui_distribution_id" {
  description = "CloudFront distribution to invalidate after a deploy."
  value       = aws_cloudfront_distribution.ui.id
}
