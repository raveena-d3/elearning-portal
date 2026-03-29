using E_learning_Portal.Service.Interface;
using ElearningAPI.Data;
using ElearningAPI.Repositories;
using ElearningAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ElearningDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Keycloak JWT Authentication
var keycloak = builder.Configuration.GetSection("Keycloak");
var authority = keycloak["Authority"];
var publicIssuer = keycloak["PublicIssuer"] ?? authority;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = publicIssuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            RoleClaimType = "roles",
            NameClaimType = "preferred_username"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity
                    as System.Security.Claims.ClaimsIdentity;
                if (claimsIdentity == null) return Task.CompletedTask;

                var realmAccess = context.Principal?
                    .FindFirst("realm_access")?.Value;

                if (realmAccess != null)
                {
                    var realmAccessObj = System.Text.Json.JsonDocument.Parse(realmAccess);
                    if (realmAccessObj.RootElement.TryGetProperty("roles", out var rolesElement))
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (roleValue != null)
                            {
                                claimsIdentity.AddClaim(
                                    new System.Security.Claims.Claim(
                                        System.Security.Claims.ClaimTypes.Role, roleValue));
                                claimsIdentity.AddClaim(
                                    new System.Security.Claims.Claim(
                                        "roles", roleValue));
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ Auth failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"❌ Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IDiscussionRepository, DiscussionRepository>();
builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();

// ── Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IDiscussionService, DiscussionService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
builder.Services.AddHttpClient<KeycloakAdminService>();

builder.Services.AddControllers();

// ★ UPDATED CORS — allows both local and Docker
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p =>
        p.WithOrigins(
            "http://localhost:4200",
            "http://localhost:80",
            "http://localhost"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("Accept-Ranges", "Content-Range", "Content-Length")));

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 524288000;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524288000;
});

var app = builder.Build();

// ★ NEW — Auto run migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ElearningDbContext>();
        db.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration failed: {ex.Message}");
    }
}

app.UseCors("AllowAll");

// HTTPS redirection disabled for Docker HTTP setup
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();