using EmailServiceAPI.Models;
using EmailServiceAPI.Services;
using EmailServiceAPI.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Amazon.SQS;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IQueueService, SqsQueueService>();


builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSQS>();

var app = builder.Build();


app.UseMetricServer();
app.UseHttpMetrics();


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");


app.MapPost("/api/auth/login", async (
    [FromBody] LoginRequest request,
    IValidator<LoginRequest> validator,
    IQueueService queueService,
    AppDbContext context,
    ILogger<Program> logger) =>
{
    using var timer = MetricsService.RequestDuration.WithLabels("/api/auth/login", "POST").NewTimer();
    
    try
    {

        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            MetricsService.LoginAttempts.WithLabels("invalid").Inc();
            logger.LogWarning("Login attempt with invalid email: {Email}", request.Email);
            return Results.BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid email format",
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            });
        }


        using var dbTimer = MetricsService.DatabaseQueryDuration.WithLabels("user_lookup").NewTimer();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            user = new User 
            { 
                Email = request.Email,
                LoginAttempts = 1,
                LastLoginAttempt = DateTime.UtcNow
            };
            context.Users.Add(user);
            MetricsService.DatabaseOperations.WithLabels("insert", "success").Inc();
        }
        else
        {
            user.LoginAttempts++;
            user.LastLoginAttempt = DateTime.UtcNow;
            MetricsService.DatabaseOperations.WithLabels("update", "success").Inc();
        }
        await context.SaveChangesAsync();
        

        MetricsService.LoginAttempts.WithLabels("success").Inc();
        MetricsService.EmailsQueued.Inc();
        
        logger.LogInformation("User login attempt recorded: {Email}, Total attempts: {Attempts}", 
            request.Email, user.LoginAttempts);


        await queueService.SendToEmailQueueAsync(request.Email);
        
        logger.LogInformation("Email request queued successfully for: {Email}", request.Email);
        return Results.Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Email queued for sending"
        });
    }
    catch (Exception ex)
    {
        MetricsService.LoginAttempts.WithLabels("error").Inc();
        MetricsService.DatabaseOperations.WithLabels("unknown", "error").Inc();
        logger.LogError(ex, "Unexpected error during login process for email: {Email}", request.Email);
        return Results.Problem(
            detail: "An unexpected error occurred",
            statusCode: 500,
            title: "Internal server error"
        );
    }
})
.WithName("Login")
.Produces<ApiResponse<object>>(200)
.Produces<ApiResponse<object>>(400)
.Produces(500);


app.MapGet("/api/health", () =>
{
    using var timer = MetricsService.RequestDuration.WithLabels("/api/health", "GET").NewTimer();
    return Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
})
.WithName("HealthCheck");
app.Run();