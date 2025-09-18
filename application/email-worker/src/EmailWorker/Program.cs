using Amazon.SQS;
using Amazon.SimpleEmail;
using EmailWorker.Services;
using Prometheus;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddAWSService<IAmazonSimpleEmailService>();


builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<EmailWorkerService>();


builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var host = builder.Build();


var metricServer = new MetricServer(hostname: "*", port: 8080);
metricServer.Start();


WorkerMetrics.WorkerHealth.Set(1);

try
{

    await host.RunAsync();
}
finally
{
    WorkerMetrics.WorkerHealth.Set(0);
    metricServer.Stop();
}