using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models; // Добавьте этот using

var builder = WebApplication.CreateBuilder(args);

// Добавьте сервис CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyOrigin",
        builder =>
        {
            builder.WithOrigins("http://127.0.0.1:5500") // Замените на фактический origin вашего HTML
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddControllers();

// Добавьте Swagger / OpenAPI
builder.Services.AddSwaggerGen(c =>  // Изменено
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Resume API", Version = "v1" }); // Изменено
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Изменено
    app.UseSwaggerUI(c => // Изменено
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resume API V1"); // Изменено
    });
}

app.UseHttpRedirection();

// Включите CORS middleware
app.UseCors("AllowMyOrigin");

app.UseAuthorization();

app.MapControllers();

app.Run();