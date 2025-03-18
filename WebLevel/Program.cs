using Bussines.UpdatePsw;
using Inteerfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // <- Регистрируем Serilog
// Добавляем конфигурацию
var configuration = builder.Configuration;

// Настройка JWT (чтобы валидировать входящие токены)
var secret = configuration["JwtSettings:Secret"];
if (string.IsNullOrEmpty(secret))
    throw new InvalidOperationException("JWT Secret is missing in configuration.");

var key = Encoding.ASCII.GetBytes(secret);
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
        provider.GetRequiredService<IConfiguration>()["JwtSettings:Secret"],
        provider.GetRequiredService<ILogger<TokenRepository>>()
    ));
builder.Services.AddLogging();
builder.Services.AddHttpClient();

// Добавляем поддержку Swagger (если нужно)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();