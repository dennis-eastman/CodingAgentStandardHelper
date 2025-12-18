using CodingAgentHelper.Core.Application.Services;
using CodingAgentHelper.Core.Domain.Repositories;
using CodingAgentHelper.Core.Infrastructure.Data;
using CodingAgentHelper.Core.Infrastructure.VectorStore;
using CodingAgentHelper.Api.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database - use appropriate provider based on environment
if (builder.Environment.IsEnvironment("Test"))
{
    // Use in-memory for testing
    builder.Services.AddDbContext<CodingAgentDbContext>(options =>
        options.UseInMemoryDatabase("test-db"));
}
else
{
    // Use SQLite for production/development
    builder.Services.AddDbContext<CodingAgentDbContext>(options =>
        options.UseSqlite("Data Source=./data/cah_standards.db"));
}

// Services
builder.Services.AddScoped<IStandardRepository, StandardRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IStandardService, StandardService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Vector Store
var chromaConfig = new ChromaConfiguration
{
    Host = builder.Configuration["Chroma:Host"] ?? "http://localhost",
    Port = int.Parse(builder.Configuration["Chroma:Port"] ?? "8601")
};
builder.Services.AddSingleton(chromaConfig);
builder.Services.AddSingleton<IChromaClient>(sp => new MockChromaClient(chromaConfig, sp.GetRequiredService<ILogger<MockChromaClient>>()));
builder.Services.AddSingleton<IEmbeddingService, MockEmbeddingService>();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Enable XML documentation for Swagger
    var xmlFile = "CodingAgentHelper.Api.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CodingAgentHelper API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Global exception handling
app.UseExceptionHandling();

// Create database and migrations (skip in Test environment)
if (!app.Environment.IsEnvironment("Test"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CodingAgentDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Make Program accessible for WebApplicationFactory in tests
public partial class Program { }
