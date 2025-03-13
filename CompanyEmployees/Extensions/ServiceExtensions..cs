using Contracts;
using Entities;
using LoggerService;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                                          builder => builder
                                          .AllowAnyOrigin()//.WithOrigins("https://example.com")
                                          .AllowAnyMethod()//.WithMethods("POST", "GET")
                                          .AllowAnyHeader()//.WithHeaders("accept", "content-type")
                                          );
            });
        }
        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options => {
            
            });
        }
        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddScoped<ILoggerManager, LoggerManager>();
        }
        public static void ConfigureSqlContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<RepositoryContext>(opts => opts.UseSqlServer(connectionString, b => b.MigrationsAssembly("CompanyEmployees")));
        }
        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>(); 
        }
    }
}
