using EmpowerBlog.Services.Review.API.Infrastructure;
using EmpowerBlog.Services.Review.API.IntegrationEvents;
using EmpowerBlog.Services.Review.API.IntegrationEvents.Handlers;
using EventBus;
using EventBus.Interfaces;
using EventBus.ServiceBus;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Polly;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(
    opt =>
    {
        opt.Filters.Add(typeof(GlobalExceptionFilter));
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"), subscribeToJwtBearerMiddlewareDiagnosticsEvents: true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.SetupAppServices();

builder.Services.AddDbContext<ReviewContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Review.API"));
});

builder.Services.AddTransient<ReviewContextSeeder>();

builder.Services.SetUpIntegrationEvents();

builder.Services.AddEventBus(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MigrateDB();

app.ConfigureIntegrationEvents();

app.Run();

public static class WebApplicationExtension
{
    public static void ConfigureIntegrationEvents(this IApplicationBuilder app)
    {
        // TODO : Subscribe with reflection
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        // Subscribe integration events
        eventBus.Subscribe<BlogDeletedIntegrationEvent, BlogDeletedIntegrationEventHandler>();
    }

    public static void MigrateDB(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<ReviewContext>();
        var seeder = services.GetRequiredService<ReviewContextSeeder>();

        Policy.Handle<SqlException>()
            .WaitAndRetryAsync(retryCount: 10,
            (retryAttemp) => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)),
            onRetry: (exception, timeSpan, retry, ctx) =>
            {
                logger.LogWarning(exception, "[prefix] Exception {ExceptionMessage} occured on attempt {retry} of {retries}]", nameof(ReviewContext), exception.Message, retry, 10);


            })
            .ExecuteAsync(async () =>
            {
                context.Database.Migrate();
                await seeder.SeedAsync();
            });
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetUpIntegrationEvents(this IServiceCollection services)
    {
        services.AddTransient<IIntegrationEventHandler<BlogDeletedIntegrationEvent>, BlogDeletedIntegrationEventHandler>();

        return services;
    }

    public static IServiceCollection AddEventBus(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddSingleton<IServiceBusPersisterConnection>(sp =>
        {
            var serviceBusConnection = configuration.GetValue<string>("EventBus:ConnectionString");
            return new DefaultServiceBusPersisterConnection(serviceBusConnection);
        });

        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        services.AddSingleton<IEventBus, ServiceBusEventBus>();

        return services;
    }
}
