using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WordsNote.Application.Extensions;
using WordsNote.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- Cấu hình Swagger ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WordsNote API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// --- Cấu hình Supabase Auth ---
var supabaseAuthSettings = builder.Configuration.GetSection("SupabaseAuth");
var isSupabaseAuthEnabled = bool.TryParse(supabaseAuthSettings["Enabled"], out var enabled) && enabled;

if (!isSupabaseAuthEnabled)
{
    throw new InvalidOperationException("SupabaseAuth:Enabled must be true in appsettings.json.");
}

var authority = supabaseAuthSettings["Authority"]?.TrimEnd('/') 
                ?? throw new InvalidOperationException("SupabaseAuth:Authority is missing.");
var audience = supabaseAuthSettings["Audience"] ?? "authenticated";
var jwtSecret = supabaseAuthSettings["JwtSecret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set true in production
    options.SaveToken = true;

    // Supabase commonly signs access tokens via JWKS. Use Authority metadata as the primary source.
    options.Authority = authority;
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidIssuer = authority,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Loại bỏ thời gian trễ 5 phút mặc định
    };

    // Optional legacy fallback for projects still using HS256 with shared JWT secret.
    if (!string.IsNullOrWhiteSpace(jwtSecret))
    {
        options.TokenValidationParameters.IssuerSigningKey =
            new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret));
    }

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("JwtBearer");

            logger.LogWarning(context.Exception,
                "JWT authentication failed for {Path}",
                context.Request.Path);

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// --- Cấu hình CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- Các dịch vụ tầng Application & Infrastructure ---
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// --- Pipeline xử lý Request ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();