using EmpowerBlog.Services.Post.infrastructure.CQRS;
using EventBus.Interfaces;
using EventBus.ServiceBus;

namespace EmpowerBlog.Services.Post.API.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void SetupAppServices(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus, ServiceBusEventBus>();
            services.AddTransient<IQueryExecuter, QueryExecuter>();
            services.AddTransient<ICommandExecuter, CommandExecuter>();

        }
    }
}
