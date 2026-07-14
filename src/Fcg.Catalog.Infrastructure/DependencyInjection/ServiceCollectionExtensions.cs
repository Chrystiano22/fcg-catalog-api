using Fcg.Catalog.Application.Events;
using Fcg.Catalog.Domain.Games;
using Fcg.Catalog.Domain.Libraries;
using Fcg.Catalog.Domain.Promotions;
using Fcg.Catalog.Infrastructure.Events;
using Fcg.Catalog.Infrastructure.Messaging;
using Fcg.Catalog.Infrastructure.Persistence;
using Fcg.Catalog.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Catalog.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration["Database:ConnectionString"]
            ?? configuration.GetConnectionString("CatalogDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string is required.");
        }

        services.AddDbContext<CatalogDbContext>(options =>
        {
            switch (provider.Trim().ToLowerInvariant())
            {
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Database provider '{provider}' is not supported in this MVP.");
            }
        });

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<ILibraryItemRepository, LibraryItemRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddMessaging(configuration);

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqOptions = RabbitMqOptions.FromConfiguration(configuration);

        if (!rabbitMqOptions.Enabled)
        {
            services.AddScoped<IEventPublisher, LoggingEventPublisher>();

            return services;
        }

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddConsumer<PaymentProcessedConsumer>();

            busConfigurator.UsingRabbitMq((context, rabbitMqConfigurator) =>
            {
                rabbitMqConfigurator.UseRawJsonSerializer(
                    RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders,
                    isDefault: true);

                rabbitMqConfigurator.Host(
                    rabbitMqOptions.Host,
                    rabbitMqOptions.VirtualHost,
                    hostConfigurator =>
                    {
                        hostConfigurator.Username(rabbitMqOptions.Username);
                        hostConfigurator.Password(rabbitMqOptions.Password);
                    });

                rabbitMqConfigurator.Message<OrderPlacedEvent>(messageConfigurator =>
                    messageConfigurator.SetEntityName(EventNames.OrderPlaced));
                rabbitMqConfigurator.Message<PaymentProcessedEvent>(messageConfigurator =>
                    messageConfigurator.SetEntityName(EventNames.PaymentProcessed));

                rabbitMqConfigurator.ReceiveEndpoint("catalog-payment-processed", endpointConfigurator =>
                {
                    endpointConfigurator.UseRawJsonDeserializer(RawSerializerOptions.AnyMessageType, isDefault: true);
                    endpointConfigurator.UseMessageRetry(retryConfigurator =>
                        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5)));
                    endpointConfigurator.ConfigureConsumer<PaymentProcessedConsumer>(context);
                });
            });
        });

        return services;
    }
}
