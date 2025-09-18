using Prometheus;

namespace EmailWorker.Services;

public static class WorkerMetrics
{
    // Counters
    public static readonly Counter EmailsProcessed = Metrics
        .CreateCounter("email_worker_emails_processed_total", "Total emails processed", new[] { "status" });
    
    public static readonly Counter SqsMessages = Metrics
        .CreateCounter("email_worker_sqs_messages_total", "SQS messages", new[] { "action", "status" });
    
    public static readonly Counter SesOperations = Metrics
        .CreateCounter("email_worker_ses_operations_total", "SES operations", new[] { "status" });

    // Histograms
    public static readonly Histogram EmailProcessingDuration = Metrics
        .CreateHistogram("email_worker_processing_duration_seconds", "Email processing duration");
    
    public static readonly Histogram SqsPollingDuration = Metrics
        .CreateHistogram("email_worker_sqs_polling_duration_seconds", "SQS polling duration");

    // Gauges
    public static readonly Gauge QueueSize = Metrics
        .CreateGauge("email_worker_queue_size", "Current queue size");
    
    public static readonly Gauge WorkerHealth = Metrics
        .CreateGauge("email_worker_health", "Worker health status (1=healthy, 0=unhealthy)");
}