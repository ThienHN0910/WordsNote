using FeatureFusion.Extensions;
using Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddMongoPersistenceServices(builder.Configuration);
builder.Services.AddSupabaseS3Options(builder.Configuration);

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

app.UseHttpsRedirection();
app.UseCors("AllowVue");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
