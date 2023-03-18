using EmpowerBlog.Services.Post.API.Application.Commands;
using EmpowerBlog.Services.Post.API.Application.IntegrationEvents;
using EmpowerBlog.Services.Post.API.Dtos;
using EmpowerBlog.Services.Post.API.Infrastructure;
using EmpowerBlog.Services.Post.Domain.Events;
using EmpowerBlog.Services.Post.Domain;
using EmpowerBlog.Services.Post.infrastructure.CQRS;
using EventBus.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Windows.Input;
using EmpowerBlog.Services.Post.API.Application.DomainEventHandlers;
using EventBus.ServiceBus;
using Microsoft.Extensions.Options;
using EventBus;
using Microsoft.AspNetCore.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace EmpowerBlog.Services.Post.API
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
                {
                    opt.Filters.Add(typeof(GlobalExceptionFilter));
                });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"), subscribeToJwtBearerMiddlewareDiagnosticsEvents: true);

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddHttpContextAccessor();

            services.SetupAppServices();

            services.AddDbContext<PostContext>(options =>
            {
                //options.UseInMemoryDatabase("PostDB");
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Post.API"));
            });
            services.AddDbContext<PostQueryContext>(options =>
            {
                //options.UseInMemoryDatabase("PostDB");
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("Post.API"));

            });

            services.AddTransient<PostContextSeeder>();

            services.AddHandlers();

            services.AddEventBus(Configuration);
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

            app.ConfigureIntegrationEvents();
        }

    }

    public static class ApplicationBuilderExtensions
    {
        public static void ConfigureIntegrationEvents(this IApplicationBuilder app)
        {
            // TODO : Subscribe with reflection
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            // Subscribe to IntegrationEvents

        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            var currentAssembly = Assembly.GetAssembly(typeof(Startup));

            services.AddTransient<ICommandHandler<UpdateBlogCommand, bool>, UpdateBlogCommanddHandler>();
            services.AddTransient<ICommandHandler<DeleteBlogCommand, bool>, DeleteBlogCommandHandler>();
            services.AddTransient<ICommandHandler<InsertBlogCommand, bool>, InsertBlogCommandHandler>();

            services.AddTransient<IQueryHandler<GetBlogQuery, BlogDto>, GetBlogQueryHandler>();
            services.AddTransient<IQueryHandler<PaginatedBlogsQuery, IEnumerable<BlogDto>>, PaginatedBlogsQueryHandler>();

            services.AddTransient<IHandler<BlogDeletedDomainEvent>, BlogDeletedEventHandler>();
            return services;
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
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
}