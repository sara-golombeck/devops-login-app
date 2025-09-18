resource "aws_sqs_queue" "email_queue" {
  name                       = "${var.project_name}-email-queue"
  visibility_timeout_seconds = 300
  message_retention_seconds  = 1209600  # 14 days
  receive_wait_time_seconds  = 20       # Long polling

  tags = var.common_tags
}