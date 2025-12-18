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

// Add Razor Pages
builder.Services.AddRazorPages();

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Create database and migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CodingAgentDbContext>();
    dbContext.Database.EnsureCreated();
}

app.MapRazorPages();

app.Run();
