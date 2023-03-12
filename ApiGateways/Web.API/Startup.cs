using EmpowerBlog.Web.API.Services.Interfaces;
using EmpowerBlog.Web.API.Services;
using Polly.Extensions.Http;
using Polly;
using System.Threading.RateLimiting;

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
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddTransient<IBlogService, BlogService>();
            services.AddTransient<IReviewService, ReviewService>();

            services.AddHttpClient<IBlogService, BlogService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ServiceUrls:Blog"]);
            })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<IReviewService, ReviewService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ServiceUrls:Review"]);
            })
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

            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            //app.UseAuthorization();

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