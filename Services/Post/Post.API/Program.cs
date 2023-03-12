
using EmpowerBlog.Services.Post.API.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace EmpowerBlog.Services.Post.API;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

        var host = builder.Build();

        MigrateDB(host);

        host.Run();
    }

    static void MigrateDB(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<PostContext>();
        var seeder = services.GetRequiredService<PostContextSeeder>();

        Policy.Handle<SqlException>()
            .WaitAndRetryAsync(retryCount: 10,
            (retryAttemp) => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)),
            onRetry: (exception, timeSpan, retry, ctx) =>
            {
                logger.LogWarning(exception, "[prefix] Exception {ExceptionMessage} occured on attempt {retry} of {retries}]", nameof(PostContext), exception.Message, retry, 10);
            })
            .ExecuteAsync(async () =>
            {
                context.Database.Migrate();
                await seeder.SeedAsync();
            });
    }
}
