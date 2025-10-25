using System;
using System.IO;
using System.Windows;
using C_C.App.Repositories;
using C_C.App.Services;
using C_C.App.ViewModel;
using C_C.App.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace C_C.App;

public partial class App : Application
{
    private IHost? _host;

    public IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("Application host not built");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var builder = Host.CreateApplicationBuilder();
        ConfigureConfiguration(builder);
        ConfigureLogging(builder);
        ConfigureServices(builder.Services, builder.Configuration);
        _host = builder.Build();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureConfiguration(HostApplicationBuilder builder)
    {
        builder.Configuration
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    private static void ConfigureLogging(HostApplicationBuilder builder)
    {
        var logPath = builder.Configuration.GetSection("Logging")?.GetValue<string>("Path") ?? "Logs/app.log";
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connection = configuration.GetConnectionString("Default") ?? "Data Source=c_c.db";
            options.UseSqlite(connection);
        });

        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPerfilRepository, PerfilRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IMensajeRepository, MensajeRepository>();

        services.AddSingleton<UserSession>();
        services.AddScoped<AuthService>();
        services.AddScoped<MatchService>();
        services.AddScoped<DiscoveryService>();
        services.AddScoped<ChatService>();
        services.AddScoped<ModerationService>();
        services.AddScoped<InputSanitizer>();

        services.AddSingleton<NavigationService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegistroViewModel>();
        services.AddTransient<DescubrirViewModel>();
        services.AddTransient<MatchesViewModel>();
        services.AddTransient<ChatListViewModel>();
        services.AddTransient<ChatViewModel>();
        services.AddTransient<PerfilViewModel>();
        services.AddTransient<PreferenciasViewModel>();
        services.AddTransient<AjustesViewModel>();
        services.AddTransient<MainWindowViewModel>();

        services.AddTransient<LoginView>();
        services.AddTransient<RegistroView>();
        services.AddTransient<DescubrirView>();
        services.AddTransient<MatchesView>();
        services.AddTransient<ChatListView>();
        services.AddTransient<ChatView>();
        services.AddTransient<PerfilView>();
        services.AddTransient<PreferenciasView>();
        services.AddTransient<AjustesView>();
        services.AddSingleton<MainWindow>();
    }
}
