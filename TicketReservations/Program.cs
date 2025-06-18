using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using TicketReservations.Core.Services;
using TicketReservations.Data;
using TicketReservations.Infrastructure.Middleware;
using TicketReservations.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework with In-Memory Database
builder.Services.AddDbContext<TicketReservationContext>(options =>
    options.UseInMemoryDatabase("TicketReservationDb"));

// Register Service Layer
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<ITicketTypeService, TicketTypeService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// Register Background Services
builder.Services.AddHostedService<ReservationCleanupService>(); 



// Configure API Versioning
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version")
    );
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'V";
    setup.SubstituteApiVersionInUrl = true;
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Concert Ticket Management API",
        Version = "v1",
        Description = "API for managing concert events, venues, and ticket sales"
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TicketReservationContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Concert Ticket Management API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Add Correlation ID middleware early in the pipeline
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow, Version = "1" }))
   .WithName("HealthCheck")
   .WithTags("Health");

app.Run();
