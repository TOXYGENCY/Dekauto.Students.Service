using Dekauto.groups.Service.groups.Service.Infrastructure;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Dekauto.Students.Service.Students.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NuGet.Protocol;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Grafana.Loki;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using Prometheus;


// Настройка логгера Serilog
Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information()
.WriteTo.Console(
    new CompactJsonFormatter()
    //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
    )
.WriteTo.GrafanaLoki(
        "http://loki:3100",
        labels: new List<LokiLabel>
        {
            new LokiLabel { Key = "app", Value = "dekauto-students" },
            new LokiLabel { Key = "app", Value = "dekauto-full" }
        })
.WriteTo.File("logs/Dekauto-Students-.log",
    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
    rollingInterval: RollingInterval.Day,
    rollOnFileSizeLimit: true,
    fileSizeLimitBytes: 10485760, // Ограничение на размер одного лога 10 MB
    retainedFileCountLimit: 31, // может быть 31 файл с последними логами, перед тем, как они будут удаляться  
    encoding: Encoding.UTF8)
.CreateLogger();

try
{
    Console.OutputEncoding = System.Text.Encoding.GetEncoding("utf-8");
    var builder = WebApplication.CreateBuilder(args);
    // Применение конфигов.
    builder.Configuration
        .AddEnvironmentVariables()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.UserName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
        .AddCommandLine(args);

    builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("Jwt__Key");
    var jwtKey = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
    {
        var mes = "Invalid secret key for JWT tokens - needs to be at least 32 characters long.";
        Log.Fatal(mes);
        throw new InvalidOperationException(mes);
    }
    var connectionString = builder.Configuration.GetConnectionString("Main");

    // Получаем список origins из конфигурации
    var allowedOrigins = builder.Configuration
        .GetSection("CorsSettings:AllowedOrigins").Get<string[]>();

    if (allowedOrigins == null || !allowedOrigins.Any())
    {
        var mes =
            "CORS AllowedOrigins are not specified in config (appsettings.json or environment). Can't configure CORS";
        Log.Error(mes);
        throw new InvalidOperationException(mes);
    }

    // Add services to the container.
    if (Boolean.Parse(builder.Configuration["UseEndpointAuth"] ?? "true"))
    {
        // Добавляем JWT сервисы
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero // Нужно для корректного отсчета
                };
            });

            // TODO: настроить связь между латинскими названиями 
    // Политики доступа к эндпоинтам
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("OnlyAdmin", policy => policy.RequireRole("Admin"));

    // Межсервисная авторизация 
    builder.Services.AddHttpClient("ExportService", (provider, client) =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var exportConfig = config.GetSection("Services:Export");
        var clientId = Environment.GetEnvironmentVariable("ClientId");
        var clientSecret = Environment.GetEnvironmentVariable("Services__Export__ClientSecret");

        client.BaseAddress = new Uri(exportConfig["general"]!);
        var authHeader = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")
        );
        Console.WriteLine($"Authorization Header: Basic {authHeader}"); // Ëîãèðóåì çàãîëîâîê
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
    });
    builder.Services.AddHttpClient("ImportService", (provider, client) =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var importConfig = config.GetSection("Services:Import");
        var clientId = Environment.GetEnvironmentVariable("ClientId");
        var clientSecret = Environment.GetEnvironmentVariable("Services__Import__ClientSecret");

        client.BaseAddress = new Uri(importConfig["general"]!);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{clientId}:{clientSecret}"
                )
            )
        );
    });
}
else
{
    // Заглушка политик доступа, если авторизация выключена
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("OnlyAdmin", policy => policy.RequireAssertion(_ => true));
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    // Добавление swagger с авторизацией
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Input JWT token (without 'Bearer')",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
        });
    });
    builder.Services.AddHttpClient();
    builder.Services.AddTransient<IStudentsRepository, StudentsRepository>();
    builder.Services.AddTransient<IGroupsRepository, GroupsRepository>();
    builder.Services.AddTransient<IStudentsService, StudentsService>();
    builder.Services.AddTransient<IGroupsService, GroupsService>();
    builder.Services.AddTransient<IExportProvider, ExportProvider>();
    builder.Services.AddTransient<IImportProvider, ImportProvider>();
    builder.Services.AddSingleton<IRequestMetricsService, RequestMetricsService>();
    builder.Services.AddDbContext<DekautoContext>(options =>
        options.UseNpgsql(connectionString)
        .UseLazyLoadingProxies());
    builder.Services.AddCors(options => options.AddPolicy("AllowMainHosts", policy =>
    {
        policy.WithOrigins(allowedOrigins)
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .WithExposedHeaders("Content-Disposition")
                 .AllowCredentials();
    }));

    Log.Information("Building the application...");
    var app = builder.Build();


    // Configure the HTTP request pipeline.


    // Явно указываем порты (для Docker)
    app.Urls.Add("http://*:5501");

    app.UseCors("AllowMainHosts");

    if (app.Environment.IsDevelopment())
    {
        Log.Warning("Development version of the application is started. Swagger activation...");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Включаем https, если указано в конфиге
    if (Boolean.Parse(app.Configuration["UseHttps"] ?? "false"))
    {
        app.Urls.Add("https://*:5502");
        app.UseHttpsRedirection();
        Log.Information("Enabled HTTPS.");
    }
    else
    {
        Log.Warning("Disabled HTTPS.");
    }

    if (Boolean.Parse(app.Configuration["UseEndpointAuth"] ?? "true"))
    {
        // Аутентификация (JWT, куки и т.д.)
        app.UseAuthentication();

        // Авторизация (защита контроллеров через [Authorize])
        app.UseAuthorization();
    }
    else
    {
        Log.Warning("Disabled all endpoint authorization.");
    }

    app.MapControllers();
    app.UseMetricsMiddleware(); // Метрики

    Log.Information("Application startup...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unexpected Fatal error has occurred in the application.");
}
finally
{
    Log.CloseAndFlush();
}