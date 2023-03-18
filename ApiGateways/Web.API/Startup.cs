using EmpowerBlog.Web.API.Services.Interfaces;
using EmpowerBlog.Web.API.Services;
using Polly.Extensions.Http;
using Polly;
using System.Threading.RateLimiting;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Configuration;
using EmpowerBlog.Web.API.Infrastructure;

namespace EmpowerBlog.Web.API
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(
                opt =>
                { });

            services.SetupAuthentication(Configuration);
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddTransient<AuthorizationDelegatingHandler>();
            services.AddTransient<IBlogService, BlogService>();
            services.AddTransient<IReviewService, ReviewService>();

            services.AddHttpClient<IBlogService, BlogService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ServiceUrls:Blog"]);
            })
                .AddHttpMessageHandler<AuthorizationDelegatingHandler>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<IReviewService, ReviewService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ServiceUrls:Review"]);
            })
                .AddHttpMessageHandler<AuthorizationDelegatingHandler>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
            {
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .WaitAndRetryAsync(0, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            }

            IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
            {
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                    .CircuitBreakerAsync(6, TimeSpan.FromMinutes(2));
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseExceptionHandler(exceptionHandlerApp =>
                {
                    exceptionHandlerApp.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = Text.Plain;
                        var exception = context.Features.Get<IExceptionHandlerFeature>();
                        await context.Response.WriteAsync($"An exception was thrown. {exception.Error.ToString()}");
                    });
                });
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                // TODO : replace with global exception handler
                app.UseStatusCodePages();
            }
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
            });
        }
    }

}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetupAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"), subscribeToJwtBearerMiddlewareDiagnosticsEvents: true);
                //.EnableTokenAcquisitionToCallDownstreamApi()
                //.AddDownstreamWebApi("MyApi", configuration.GetSection("PostApi"))
                //.AddInMemoryTokenCaches();



        return services;
    }
    public static void AddRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter("common-fixed-limiter", partition =>
                        new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100000,
                            Window = TimeSpan.FromMinutes(1)
                        })),
                PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(httpContext.User?.Identity?.Name ?? "anonymous", partition =>
                        new SlidingWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 4,
                            PermitLimit = 250,

                        })));
        });
    }
}