using Dekauto.groups.Service.groups.Service.Infrastructure;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Dekauto.Students.Service.Students.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// ���������� ��������.
builder.Configuration
    .AddEnvironmentVariables()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.UserName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
    .AddCommandLine(args);

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
    Console.WriteLine($"Authorization Header: Basic {authHeader}"); // �������� ���������
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

builder.Services.AddHttpClient();
builder.Services.AddTransient<IStudentsRepository, StudentsRepository>();
builder.Services.AddTransient<IGroupsRepository, GroupsRepository>();
builder.Services.AddTransient<IStudentsService, StudentsService>();
builder.Services.AddTransient<IGroupsService, GroupsService>();
builder.Services.AddTransient<IExportProvider, ExportProvider>();
builder.Services.AddTransient<IImportProvider, ImportProvider>();
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
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("AngularLocalhost");

} else
{
    app.Urls.Add("https://*:5502");
    app.UseHttpsRedirection(); // ��� https ��������� � dev-������
}


// 1. �������������� (JWT, ���� � �.�.)
app.UseAuthentication();

// 2. ����������� (�������� ��������� [Authorize])
app.UseAuthorization();


app.MapControllers();

app.Run();
