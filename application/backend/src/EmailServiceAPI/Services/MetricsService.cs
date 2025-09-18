using Prometheus;

namespace EmailServiceAPI.Services;

public static class MetricsService
{
    public static readonly Counter LoginAttempts = Metrics
        .CreateCounter("email_service_login_attempts_total", "Total number of login attempts", new[] { "status" });
    
    public static readonly Counter EmailsQueued = Metrics
        .CreateCounter("email_service_emails_queued_total", "Total number of emails queued");
    
    public static readonly Counter DatabaseOperations = Metrics
        .CreateCounter("email_service_database_operations_total", "Total database operations", new[] { "operation", "status" });

    public static readonly Histogram RequestDuration = Metrics
        .CreateHistogram("email_service_request_duration_seconds", "Request duration in seconds", new[] { "endpoint", "method" });
    
    public static readonly Histogram DatabaseQueryDuration = Metrics
        .CreateHistogram("email_service_database_query_duration_seconds", "Database query duration in seconds", new[] { "operation" });
}