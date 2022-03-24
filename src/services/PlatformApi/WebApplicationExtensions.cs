﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace PlatformApi
{
    public static class WebApplicationExtensions
    {
        public static WebApplication MigrateDbContext<TContext>(this WebApplication app, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetRequiredService<TContext>();

                try
                {
                    logger.LogInformation("Migration database associated with context {DbContextName}", typeof(TContext).Name);

                    var retries = 10;
                    var retry = Policy.Handle<SqlException>()
                        .WaitAndRetry(
                        retryCount: retries,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)),
                        onRetry: (exception, timeSpan, retry, ctx) =>
                        {
                            logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", nameof(TContext), exception.GetType().Name, exception.Message, retry, retries);
                        });

                    retry.Execute(() => InvokeSeeder(seeder, context, services));
                    logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                }
            }

            return app;
        }

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services)
        where TContext : DbContext
        {
            if (context.Database.IsRelational())
            {
                context.Database.EnsureCreated();
                
            }
            seeder(context,services);
        }
    }
}
