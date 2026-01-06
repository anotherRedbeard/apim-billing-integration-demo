using ApimBilling.Api.Configuration;
using ApimBilling.Api.Endpoints;
using ApimBilling.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from environment variables
var apimSettings = new ApimSettings
{
    ApimName = builder.Configuration["APIM_NAME"] 
        ?? throw new InvalidOperationException("APIM_NAME is required"),
    ResourceGroup = builder.Configuration["APIM_RESOURCE_GROUP"] 
        ?? throw new InvalidOperationException("APIM_RESOURCE_GROUP is required"),
    SubscriptionId = builder.Configuration["AZURE_SUBSCRIPTION_ID"] 
        ?? throw new InvalidOperationException("AZURE_SUBSCRIPTION_ID is required")
};

// Validate configuration
ConfigurationValidator.ValidateApimSettings(apimSettings);

// Register configuration
builder.Services.AddSingleton(apimSettings);

// Add services
builder.Services.AddHttpClient<IApimSubscriptionClient, ApimSubscriptionClient>();
builder.Services.AddScoped<IBillingService, BillingService>();

// Add Application Insights
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Map endpoints
app.MapProductEndpoints();
app.MapSubscriptionEndpoints();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy",
    timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Health");

app.Run();
