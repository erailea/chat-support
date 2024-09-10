using System.Threading.RateLimiting;
using ChatSupport.Data;
using ChatSupport.Monitor;
using ChatSupport.Queue;
using ChatSupport.Queue.Interfaces;
using ChatSupport.Services;
using ChatSupport.Services.Interfaces;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RabbitMQ.Client;

namespace ChatSupport
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //TODO: get numbers from configuration
            builder.Services.AddRateLimiter(_ => _
                .AddFixedWindowLimiter(policyName: "fixed", options =>
                {
                    options.PermitLimit = 4;
                    options.Window = TimeSpan.FromSeconds(12);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                }));


            var connectionString =
                builder.Configuration.GetConnectionString("ChatSupportDbConnectionString");

            var migrationOptions = new MongoMigrationOptions
            {
                MigrationStrategy = new MigrateMongoMigrationStrategy(),
                BackupStrategy = new CollectionMongoBackupStrategy()
            };

            builder
                .Services.AddHangfire(configuration =>
                    {
                        configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                        configuration.UseSimpleAssemblyNameTypeSerializer();
                        configuration.UseRecommendedSerializerSettings();
                        configuration.UseMongoStorage(connectionString, "Hangfire", new MongoStorageOptions { MigrationOptions = migrationOptions });
                    }
                );

            builder
                .Services.AddHangfireServer();

            builder
                .Services
                .Configure<RabbitMqConfiguration>(builder
                    .Configuration
                    .GetSection("RabbitMqConfiguration"));

            builder
                .Services
                .AddSingleton(sp =>
            {
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("ChatSupportDb");
                return database;
            });

            builder
                .Services
                .AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

            builder
                .Services
                .AddSingleton(sp =>
                {
                    var options =
                        sp
                            .GetRequiredService
                            <IOptions<RabbitMqConfiguration>>();
                    return new ConnectionFactory()
                    {
                        HostName = options.Value.HostName,
                        UserName = options.Value.UserName,
                        Password = options.Value.Password
                    };
                });
            builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();
            builder.Services.AddScoped<IAgentService, AgentService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IAgentChatCoordinatorService, AgentChatCoordinatorService>();
            builder.Services.AddScoped<SeedService>();

            SeedData(builder);

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseRateLimiter();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app
                .MapGet("/",
                () =>
                {
                    return new { Message = "App is running" };
                })
                .WithOpenApi();

            app.MapControllers();

            app.UseHangfireDashboard();

            RecurringJob.AddOrUpdate<ChatSessionMonitor>(
                    "monitor-chat-sessions",
                    monitor => monitor.MonitorChatSessions(),
                    "*/10 * * * * *");

            app.Run();
        }

        public static void SeedData(WebApplicationBuilder builder)
        {
            var seedService = builder.Services.BuildServiceProvider().GetService<SeedService>();
            if (seedService != null)
                seedService.SeedData().Wait();
        }
    }
}
