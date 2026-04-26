using Autofac;
using Autofac.Extensions.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using ShareFlow.Api.Filters;
using ShareFlow.Api.Middleware;
using ShareFlow.Application.Auth;
using ShareFlow.Application.Auth.Interfaces;
using ShareFlow.Application.Common.Interfaces;
using ShareFlow.Application;
using ShareFlow.Infrastructure;
using ShareFlow.Infrastructure.Jobs;
using ShareFlow.Infrastructure.Persistence;
using System.Reflection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day));

// Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(cb =>
{
    cb.RegisterModule<ApplicationModule>();
    cb.RegisterModule<InfrastructureModule>();
});

// EF Core
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddHttpContextAccessor();

// JWT
var jwtKey = builder.Configuration["Jwt:SecretKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
        opt.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
                if (string.IsNullOrWhiteSpace(jti))
                {
                    return;
                }

                var redisService = context.HttpContext.RequestServices.GetRequiredService<IRedisService>();
                var exists = await redisService.KeyExistsAsync($"token:blacklist:{jti}");
                if (exists)
                {
                    context.Fail("Token has been revoked.");
                }
            }
        };
    });

// Mapster
var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Scan(
    Assembly.GetExecutingAssembly(),
    typeof(ApplicationModule).Assembly);
builder.Services.AddSingleton(mapsterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationModule).Assembly);

builder.Services.AddShareFlowJobScheduling();

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ShareFlow API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<IIdentitySeedService>();
    await seedService.SeedAsync();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", region = app.Configuration["REGION"] }));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
