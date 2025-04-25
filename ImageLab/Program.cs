using ImageLab;
using ImageLab.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

#if DEBUG
var botToken = GetEnvironmentVariable("IMAGELAB_BOT_TOKEN_DEBUG");

if (string.IsNullOrEmpty(botToken))
{
    throw new ConfigurationException("IMAGELAB_BOT_TOKEN_DEBUG environment variable is not set");
}
#else
var botToken = GetEnvironmentVariable("IMAGELAB_BOT_TOKEN");

if (string.IsNullOrEmpty(botToken))
{
    throw new ConfigurationException("IMAGELAB_BOT_TOKEN environment variable is not set");
}
#endif

var builder = Host.CreateApplicationBuilder();
ConfigureDatabase(builder.Services);
ConfigureDiscord(builder, botToken);

var host = builder.Build();

ConfigureDiscordCommands(host);

await host.RunAsync();

return;

void ConfigureDiscord(HostApplicationBuilder builder, string botToken)
{
    builder.Services
        .AddDiscordGateway(options => options.Token = botToken)
        .AddApplicationCommands();
}

void ConfigureDiscordCommands(IHost host)
{
    host
        .AddModules(typeof(Program).Assembly)
        .UseGatewayEventHandlers();
}

void ConfigureDatabase(IServiceCollection services)
{
#if DEBUG
    var dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ava.db");
    ConfigureSqliteDatabase(services, dbFilePath);
#else
    var connectionString = GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ConfigurationException("DATABASE_CONNECTION_STRING environment variable is not set");
    }

    ConfigurePostgresqlDatabase(services, connectionString);
#endif
}

static void ConfigureSqliteDatabase(IServiceCollection services, string dbFilePath)
{
    services.AddDbContext<ImageLabContext>(options => { options.UseSqlite($"Data Source={dbFilePath}"); });
}

static void ConfigurePostgresqlDatabase(IServiceCollection services, string connectionString)
{
    services.AddDbContext<ImageLabContext>(options => { options.UseNpgsql(connectionString); });
}

string? GetEnvironmentVariable(string name)
{
    string? process = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    if (process != null) return process;

    string? user = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
    if (user != null) return user;

    string? machine = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
    return machine;
}