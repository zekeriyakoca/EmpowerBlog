using EmpowerBlog.Services.Review.API.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(
    opt =>
    {
        opt.Filters.Add(typeof(GlobalExceptionFilter));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.SetupAppServices();

builder.Services.AddDbContext<ReviewContext>(options =>
{
    options.UseInMemoryDatabase("ReviewDB");
    //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddTransient<ReviewContextSeeder>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MigrateDB();

app.Run();

public static class WebApplicationExtension
{
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

