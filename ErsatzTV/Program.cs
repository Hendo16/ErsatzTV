using Destructurama;
using ErsatzTV.Core;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ErsatzTV;

public class Program
{
    private static readonly string BasePath;

    static Program()
    {
        string executablePath = Environment.ProcessPath ?? string.Empty;
        string executable = Path.GetFileNameWithoutExtension(executablePath);

        IConfigurationBuilder builder = new ConfigurationBuilder();

        BasePath = Path.GetDirectoryName(
            "dotnet".Equals(executable, StringComparison.InvariantCultureIgnoreCase)
                ? typeof(Program).Assembly.Location
                : executablePath);

        Configuration = builder
            .SetBasePath(BasePath)
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                true)
            .AddEnvironmentVariables()
            .Build();

        LoggingLevelSwitch = new LoggingLevelSwitch();
    }

    private static IConfiguration Configuration { get; }

    private static LoggingLevelSwitch LoggingLevelSwitch { get; }

    public static async Task<int> Main(string[] args)
    {
        LoggingLevelSwitch.MinimumLevel = LogEventLevel.Information;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .Destructure.UsingAttributes()
            .Enrich.FromLogContext()
            .WriteTo.File(FileSystemLayout.LogFilePath, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            await CreateHostBuilder(args).Build().RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services.AddSingleton(LoggingLevelSwitch))
            .ConfigureWebHostDefaults(
                webBuilder => webBuilder.UseStartup<Startup>()
                    .UseConfiguration(Configuration)
                    .UseKestrel(options => options.AddServerHeader = false)
                    .UseContentRoot(BasePath))
            .UseSerilog();
}
