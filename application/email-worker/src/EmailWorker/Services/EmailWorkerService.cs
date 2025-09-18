using Amazon.SQS;
using Amazon.SQS.Model;
using EmailWorker.Models;
using EmailWorker.Services;
using System.Text.Json;
using System.Diagnostics;

namespace EmailWorker.Services
{
    public class EmailWorkerService : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailWorkerService> _logger;
        private readonly string _queueUrl;

        public EmailWorkerService(
            IAmazonSQS sqsClient,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<EmailWorkerService> logger)
        {
            _sqsClient = sqsClient;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _queueUrl = _configuration["AWS:SQS:QueueUrl"] ?? throw new InvalidOperationException("SQS Queue URL not configured");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Worker Service started. Queue URL: {QueueUrl}", _queueUrl);

            while (!stoppingToken.IsCancellationRequested)
            {
                var pollingStopwatch = Stopwatch.StartNew();
                bool shouldMeasurePolling = true;

                try
                {
                    // Read messages from Queue
                    var request = new ReceiveMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 20
                    };

                    var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);
                    
                    WorkerMetrics.QueueSize.Set(response.Messages.Count);
                    WorkerMetrics.SqsMessages.WithLabels("receive", "success").Inc(response.Messages.Count);

                    if (response.Messages.Count > 0)
                    {
                        _logger.LogInformation("Received {Count} messages from queue", response.Messages.Count);

                        foreach (var message in response.Messages)
                        {
                            await ProcessMessageAsync(message, stoppingToken);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("No messages received, waiting...");
                    }
                }
                catch (OperationCanceledException)
                {
                    shouldMeasurePolling = false;
                    _logger.LogInformation("Email Worker Service is stopping...");
                    break;
                }
                catch (Exception ex)
                {
                    WorkerMetrics.SqsMessages.WithLabels("receive", "error").Inc();
                    WorkerMetrics.WorkerHealth.Set(0);
                    _logger.LogError(ex, "Error in Email Worker Service main loop");
                    await Task.Delay(5000, stoppingToken);
                    WorkerMetrics.WorkerHealth.Set(1);
                }
                finally
                {

                    if (shouldMeasurePolling)
                    {
                        WorkerMetrics.SqsPollingDuration.Observe(pollingStopwatch.Elapsed.TotalSeconds);
                    }
                }
            }

            _logger.LogInformation("Email Worker Service stopped");
        }

        private async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {

                var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message.Body);
                
                if (emailMessage == null || string.IsNullOrEmpty(emailMessage.Email))
                {
                    WorkerMetrics.EmailsProcessed.WithLabels("invalid").Inc();
                    _logger.LogWarning("Invalid message format: {MessageBody}", message.Body);
                    await DeleteMessageAsync(message);
                    return;
                }


                var processingStopwatch = Stopwatch.StartNew();
                
                try
                {
                    _logger.LogInformation("Processing email for: {Email}", emailMessage.Email);

                    var result = await _emailService.SendLoginEmailAsync(emailMessage.Email);

                    if (result.Success)
                    {
                        WorkerMetrics.EmailsProcessed.WithLabels("success").Inc();
                        WorkerMetrics.SesOperations.WithLabels("success").Inc();
                        _logger.LogInformation("Email sent successfully to: {Email}, MessageId: {MessageId}", 
                            emailMessage.Email, result.MessageId);
                        

                        await DeleteMessageAsync(message);
                    }
                    else
                    {
                        WorkerMetrics.EmailsProcessed.WithLabels("failed").Inc();
                        WorkerMetrics.SesOperations.WithLabels("failed").Inc();
                        _logger.LogError("Failed to send email to: {Email}, Error: {Error}", 
                            emailMessage.Email, result.ErrorMessage);
                        

                        await DeleteMessageAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    WorkerMetrics.EmailsProcessed.WithLabels("error").Inc();
                    _logger.LogError(ex, "Error processing email for message: {MessageId}", message.MessageId);
                    throw;
                }
                finally
                {

                    WorkerMetrics.EmailProcessingDuration.Observe(processingStopwatch.Elapsed.TotalSeconds);
                }
            }
            catch (JsonException jsonEx)
            {
                WorkerMetrics.EmailsProcessed.WithLabels("invalid").Inc();
                _logger.LogError(jsonEx, "Invalid JSON format in message: {MessageId}", message.MessageId);
                await DeleteMessageAsync(message);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Unexpected error processing message: {MessageId}", message.MessageId);
            }
        }

        private async Task DeleteMessageAsync(Message message)
        {
            try
            {
                await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
                WorkerMetrics.SqsMessages.WithLabels("delete", "success").Inc();
                _logger.LogDebug("Message deleted from queue: {MessageId}", message.MessageId);
            }
            catch (Exception ex)
            {
                WorkerMetrics.SqsMessages.WithLabels("delete", "error").Inc();
                _logger.LogError(ex, "Failed to delete message: {MessageId}", message.MessageId);
            }
        }
    }
}