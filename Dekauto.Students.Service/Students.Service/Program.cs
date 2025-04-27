using Dekauto.groups.Service.groups.Service.Infrastructure;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Dekauto.Students.Service.Students.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;


// ��������� ������� Serilog
Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information()
.WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
.WriteTo.File("logs/Dekauto-Students-.log", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", rollingInterval: RollingInterval.Day,
    rollOnFileSizeLimit: true,
    fileSizeLimitBytes: 10485760, // ����������� �� ������ ������ ���� 10 MB
    retainedFileCountLimit: 31) // ����� ���� 31 ���� � ���������� ������, ����� ���, ��� ��� ����� ���������
.CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    // ���������� ��������.
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
        var mes = "�������� ��������� ���� ��� JWT ������� - ���������� �� ����� 32 ��������.";
        Log.Fatal(mes);
        throw new InvalidOperationException(mes);
    }
    var connectionString = builder.Configuration.GetConnectionString("Main");

    // Add services to the container.
    // ��������� JWT �������
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
                ClockSkew = TimeSpan.Zero // ����� ��� ������ �������� �������

            };
        });

    // TODO: ���������������� ����
    // �������� ������� � ����������
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("OnlyAdmin", policy => policy.RequireRole("�������������"));

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    // ���������� swagger � ������������
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "������� JWT ����� (��� ����� 'Bearer')",
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
    builder.Services.AddCors(options => options.AddPolicy("AngularLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // ����� Angular-����������
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .WithExposedHeaders("Content-Disposition")
                 .AllowCredentials();
    }));

    var app = builder.Build();


    // Configure the HTTP request pipeline.


    // ���� ��������� ����� (��� Docker)
    app.Urls.Add("http://*:5501");

    if (app.Environment.IsDevelopment())
    {
        Log.Warning("����������� Development-������ ����������. ��������� Swagger...");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // ���������� https, ���� ��� ������� � �������
    if (Boolean.Parse(app.Configuration["UseHttps"]))
    {
        app.Urls.Add("https://*:5508");
        app.UseHttpsRedirection();
        Log.Information("������� HTTPS.");
    }

    // �������������� (JWT, ���� � �.�.)
    app.UseAuthentication();

    // ����������� (�������� ��������� [Authorize])
    app.UseAuthorization();

    app.MapControllers();

    app.UseMetricsMiddleware(); // �������

    Log.Information("����������� Development-������ ����������. ��������� Swagger...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "� ���������� �������� �������������� ����������� ������.");
}
finally
{
    Log.CloseAndFlush();
}