using ApimBilling.Api.Configuration;
using ApimBilling.Api.Endpoints;
using ApimBilling.Api.Filters;
using ApimBilling.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Note: APIM configuration must be provided via required HTTP headers (X-APIM-ServiceName, X-APIM-ResourceGroup)
// This allows dynamic APIM instance selection per request.

// Add HttpContextAccessor for accessing request headers
builder.Services.AddHttpContextAccessor();

// Register APIM configuration provider (reads from required headers)
builder.Services.AddScoped<IApimConfigurationProvider, ApimConfigurationProvider>();

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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "APIM Billing API",
        Version = "v1",
        Description = @"API for managing Azure APIM subscriptions and billing. 

**Required Headers:**
All requests must include these headers:
- `X-APIM-ServiceName`: Azure APIM service name (e.g., 'my-apim-instance')
- `X-APIM-ResourceGroup`: Azure resource group name (e.g., 'my-resource-group')"
    });

    // Add operation filter to include APIM headers in Swagger UI
    options.OperationFilter<ApimHeadersOperationFilter>();
});

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
