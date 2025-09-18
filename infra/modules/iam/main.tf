data "aws_iam_policy_document" "auth_service_assume_role" {
  statement {
    effect = "Allow"
    
    principals {
      type        = "Federated"
      identifiers = [var.cluster_oidc_issuer_arn]
    }
    
    actions = ["sts:AssumeRoleWithWebIdentity"]
    
    condition {
      test     = "StringEquals"
      variable = "${replace(var.cluster_oidc_issuer_url, "https://", "")}:sub"
      values   = ["system:serviceaccount:${var.namespace}:auth-service"]
    }
  }
}

resource "aws_iam_role" "auth_service" {
  name               = "${var.project_name}-auth-service-role"
  assume_role_policy = data.aws_iam_policy_document.auth_service_assume_role.json
  tags               = var.common_tags
}

data "aws_iam_policy_document" "auth_service_policy" {
  statement {
    effect = "Allow"
    actions = [
      "sqs:SendMessage",
      "sqs:GetQueueAttributes"
    ]
    resources = [var.sqs_queue_arn]
  }
}

resource "aws_iam_role_policy" "auth_service" {
  name   = "${var.project_name}-auth-service-policy"
  role   = aws_iam_role.auth_service.id
  policy = data.aws_iam_policy_document.auth_service_policy.json
}

# Email Service Role
data "aws_iam_policy_document" "email_service_assume_role" {
  statement {
    effect = "Allow"
    principals {
      type        = "Federated"
      identifiers = [var.cluster_oidc_issuer_arn]
    }
    
    actions = ["sts:AssumeRoleWithWebIdentity"]
    
    condition {
      test     = "StringEquals"
      variable = "${replace(var.cluster_oidc_issuer_url, "https://", "")}:sub"
      values   = ["system:serviceaccount:${var.namespace}:email-service"]
    }
  }
}
resource "aws_iam_role" "email_service" {
  name               = "${var.project_name}-email-service-role"
  assume_role_policy = data.aws_iam_policy_document.email_service_assume_role.json
  tags               = var.common_tags
}

data "aws_iam_policy_document" "email_service_policy" {
  statement {
    effect = "Allow"
    actions = [
      "sqs:ReceiveMessage",
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes"
    ]
    resources = [var.sqs_queue_arn]
  }
  
  statement {
    effect = "Allow"
    actions = [
      "ses:SendEmail",
      "ses:SendRawEmail"
    ]
    resources = [
      var.ses_domain_arn,
      "arn:aws:ses:ap-south-1:980921758549:identity/myname.click"
    ]
  }
}

resource "aws_iam_role_policy" "email_service" {
  name   = "${var.project_name}-email-service-policy"
  role   = aws_iam_role.email_service.id
  policy = data.aws_iam_policy_document.email_service_policy.json
}