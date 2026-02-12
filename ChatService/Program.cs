using Asp.Versioning;
using ChatService.Infrastructure;
using ChatService.Infrastructure.Messaging;
using ChatService.Interfaces;
using ChatService.Messaging;
using ChatService.Messaging.Consumer;
using ChatService.Messaging.Publisher;
using ChatService.Middleware;
using ChatService.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// API Versioning
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScoped<IChatService, ChatService.Service.ChatService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();

builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();


builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

builder.Services.AddHostedService<AIResponseGeneratedConsumer>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: "Bearer", securityScheme: new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name ="Authorization",
        Description = "Enter the Bearer Authorization string as Follow: Bearer {token}",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        
    });
    option.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, Array.Empty<string>()
        }
    });
});

builder.AddAppAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

//ApplyMigration();

app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        _db.Database.ExecuteSqlRaw("CREATE SCHEMA IF NOT EXISTS ers");
        
        

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}