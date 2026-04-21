using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DatabaseContext>();

// Настраиваем CORS для работы и с localhost, и с Render
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
    builder =>
    {
        // Для продакшн - разрешаем любые источники (временно)
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
    
    // Оставляем для локальной разработки
    options.AddPolicy("AllowAngularOrigins",
    builder =>
    {
        builder.WithOrigins("http://localhost:4200", "http://localhost:9876")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

var app = builder.Build();

// Настройка статических файлов ДО app.Run()
app.UseStaticFiles(); // Для стандартных статических файлов

// Для загруженных файлов (uploads)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

// Swagger - включаем для всех окружений (временно для отладки)
app.UseSwagger();
app.UseSwaggerUI();

// Для продакшн на Render - отключаем HTTPS редирект (временно)
// app.UseHttpsRedirection(); 

// Выбираем CORS политику в зависимости от окружения
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAngularOrigins");
}
else
{
    app.UseCors("AllowAllOrigins");
}

app.UseAuthorization();
app.MapControllers();

// Простой тестовый эндпоинт для проверки работоспособности
app.MapGet("/", () => new { 
    status = "API is running", 
    environment = app.Environment.EnvironmentName,
    time = DateTime.Now 
});

app.Run();