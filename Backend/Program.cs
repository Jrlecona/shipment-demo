using System.Net.Mime;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core + SQLite
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=shipments.db"));

// Swagger (optional, nice for demo)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Optional: show XML comments (see step 3)
    var xmlName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlName);
    if (System.IO.File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // Optional: customize document title/version
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Shipments API",
        Version = "v1",
        Description = "Simple demo API for shipments"
    });
});

// CORS for Vite dev server
const string CorsPolicy = "frontend";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p =>
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

// Global error handler -> ProblemDetails
app.UseExceptionHandler(appErr =>
{
    appErr.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        var problem = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Detail = ex?.Message,
            Status = StatusCodes.Status500InternalServerError
        };
        context.Response.StatusCode = problem.Status ?? 500;
        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseCors(CorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Shipments API v1");
        options.RoutePrefix = "swagger"; // so UI is at /swagger
    });
}


// Minimal endpoint: GET /api/shipments?status=Active|Completed
app.MapGet("/api/shipments", async ([FromQuery] string? status, AppDbContext db) =>
{
    IQueryable<Shipment> q = db.Shipments.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(status) &&
        Enum.TryParse<ShipmentStatus>(status, true, out var parsed))
    {
        q = q.Where(s => s.Status == parsed);
    }

    var result = await q.OrderBy(s => s.Id).ToListAsync();
    return Results.Ok(result);
})
.WithName("GetShipments")
.Produces<List<Shipment>>(StatusCodes.Status200OK);

// Ensure DB + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
