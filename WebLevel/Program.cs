using Bussines.UpdatePsw;
using Inteerfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using WebApplication1;
using WebLevel.Example;

var builder = WebApplication.CreateBuilder(args);

// Настройка логирования
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Регистрируем Serilog

string jwtSecret;

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5055);
    });

    jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret is missing in configuration");
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
}
else
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(8080);
    });
    jwtSecret = Environment.GetEnvironmentVariable("JwtSecret") ?? throw new InvalidOperationException("JWT Secret is missing in environment variables");

    builder.Services.Configure<SmtpSettings>(options =>
    {
        options.SmtpServer = Environment.GetEnvironmentVariable("SmtpServer") ?? throw new Exception("SmtpServer is missing");
        Log.Information($"SmtpServer { options.SmtpServer}");

        if (!int.TryParse(Environment.GetEnvironmentVariable("Port"), out int port))
            throw new Exception("Port is missing or invalid");

        options.Port = port;
        Log.Information($"Port { options.Port}");

        options.Password = Environment.GetEnvironmentVariable("Password") ?? throw new Exception("Password is missing");
        Log.Information($"Password { options.Password}");

        options.FromEmail = Environment.GetEnvironmentVariable("FromEmail") ?? throw new Exception("FromEmail is missing");
        Log.Information($"FromEmail { options.FromEmail}");
    });
}

// Настройка JWT
var key = Encoding.ASCII.GetBytes(jwtSecret);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// Добавляем контроллеры
builder.Services.AddControllers();

// Регистрируем зависимости
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IMail, MailRepository>();
builder.Services.AddScoped<IService, PasswordRecoveryService>();
builder.Services.AddSingleton<TokenRepository>(provider =>
    new TokenRepository(
        jwtSecret,
        provider.GetRequiredService<ILogger<TokenRepository>>()
    ));
builder.Services.AddLogging();
builder.Services.AddHttpClient();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PasswordRecovery API", Version = "v1" });
    c.EnableAnnotations();
    c.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<RequestResetPasswordExample>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<ResetPasswordExample>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("Starting web application");

app.Run();
