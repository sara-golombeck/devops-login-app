output "queue_url" {
  description = "SQS Queue URL"
  value       = aws_sqs_queue.email_queue.url
}

output "queue_arn" {
  description = "SQS Queue ARN"
  value       = aws_sqs_queue.email_queue.arn
}