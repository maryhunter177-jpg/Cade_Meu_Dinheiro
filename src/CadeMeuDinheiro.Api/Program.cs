using System.Security.Claims;
using System.Text;
using CadeMeuDinheiro.Api;
using CadeMeuDinheiro.Application;
using CadeMeuDinheiro.Domain;
using CadeMeuDinheiro.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<AuthService>(); builder.Services.AddScoped<FinanceService>();
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("auth", limiter => { limiter.PermitLimit = 10; limiter.Window = TimeSpan.FromMinutes(1); limiter.QueueLimit = 0; }));
var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true, ValidIssuer = jwt.Issuer, ValidateAudience = true, ValidAudience = jwt.Audience,
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30), ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey))
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["https://localhost:7240"])
    .AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Cadê Meu Dinheiro? API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new() { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    options.AddSecurityRequirement(new() { [new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = [] });
});

var app = builder.Build();
app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection(); app.UseCors(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization();
app.MapHealthChecks("/health/live"); app.MapHealthChecks("/health/ready");

var auth = app.MapGroup("/api/auth").RequireRateLimiting("auth").WithTags("Authentication");
auth.MapPost("/register", (RegisterRequest request, AuthService service, CancellationToken ct) => service.RegisterAsync(request, ct));
auth.MapPost("/login", (LoginRequest request, AuthService service, CancellationToken ct) => service.LoginAsync(request, ct));

var transactions = app.MapGroup("/api/transactions").RequireAuthorization().WithTags("Transactions");
transactions.MapGet("/", async (HttpContext ctx, FinanceService service, int page = 1, int pageSize = 20, TransactionType? type = null, string? search = null, CancellationToken ct = default) =>
    await service.ListAsync(ctx.UserId(), page, pageSize, type, search, ct));
transactions.MapPost("/", async (HttpContext ctx, TransactionRequest request, FinanceService service, CancellationToken ct) =>
    Results.Created("/api/transactions", await service.CreateAsync(ctx.UserId(), request, ct)));
transactions.MapDelete("/{id:guid}", async (HttpContext ctx, Guid id, FinanceService service, CancellationToken ct) => { await service.DeleteAsync(ctx.UserId(), id, ct); return Results.NoContent(); });
app.MapGet("/api/dashboard", async (HttpContext ctx, FinanceService service, int? year, int? month, CancellationToken ct) =>
{
    var now = DateTimeOffset.UtcNow; return await service.DashboardAsync(ctx.UserId(), year ?? now.Year, month ?? now.Month, ct);
}).RequireAuthorization().WithTags("Dashboard");

app.Run();
public partial class Program;
