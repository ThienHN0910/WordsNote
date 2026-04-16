using FeatureFusion.Extensions;
using Infrastructure.Configuration;

using MongoDB.Driver;

using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.ApplyFlatEnvironmentOverrides();

// Register services
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddMongoPersistenceServices(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        var statusCode = exception switch
        {
            MongoException => StatusCodes.Status503ServiceUnavailable,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError,
        };

        var message = statusCode switch
        {
            StatusCodes.Status503ServiceUnavailable => "Dependent data service is unavailable. Please try again later.",
            StatusCodes.Status401Unauthorized => "Unauthorized.",
            _ => "Unexpected server error.",
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            Error = message,
            TraceId = context.TraceIdentifier,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    });
});

app.UseHttpsRedirection();
app.UseCors("AllowVue");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
