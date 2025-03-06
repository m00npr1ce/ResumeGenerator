using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ResumeGenerator;
using System.Text;
using ResumeGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������������ ��� JWT
var secretKey = builder.Configuration["Jwt:SecretKey"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500")  // ��������� ����� ��������
              .AllowAnyMethod()  // ��������� ����� HTTP ������ (GET, POST, � �.�.)
              .AllowAnyHeader()
              .AllowCredentials(); // ��������� ����� ���������
    });
});


// ������������ ������
builder.Services.AddSingleton<ITokenService>(new TokenService(secretKey, issuer, audience));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new ArgumentException("JWT Secret Key is missing from configuration.");
        }
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,  // ����� ����� �� ������������
            ValidAudience = audience,  // ����� ����� �� ������������
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))  // ��� ��������� ����
        };
    });

builder.Services.AddDbContext<ResumeGeneratorContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

