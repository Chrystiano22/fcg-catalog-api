using Microsoft.Extensions.Configuration;

namespace Fcg.Catalog.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public bool Enabled { get; init; }

    public string Host { get; init; } = "localhost";

    public string VirtualHost { get; init; } = "/";

    public string Username { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public static RabbitMqOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMq");

        return new RabbitMqOptions
        {
            Enabled = bool.TryParse(section["Enabled"], out var enabled) && enabled,
            Host = section["Host"] ?? "localhost",
            VirtualHost = section["VirtualHost"] ?? "/",
            Username = section["Username"] ?? "guest",
            Password = section["Password"] ?? "guest"
        };
    }
}
