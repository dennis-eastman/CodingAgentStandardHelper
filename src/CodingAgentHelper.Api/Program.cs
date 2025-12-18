using CodingAgentHelper.Core.Application.Services;
using CodingAgentHelper.Core.Domain.Repositories;
using CodingAgentHelper.Core.Infrastructure.Data;
using CodingAgentHelper.Core.Infrastructure.VectorStore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<CodingAgentDbContext>(options =>
    options.UseSqlite("Data Source=./data/cah_standards.db"));

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

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Create database and migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CodingAgentDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.Run();
