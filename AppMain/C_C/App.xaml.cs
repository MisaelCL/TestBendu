using System;
using System.Windows;
using C_C.Model;
using C_C.Repositories;
using C_C.Resources.utils;
using C_C.Services;
using C_C.View;
using C_C.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace C_C;

public partial class App : Application
{
    private IHost? _host;
    public static IServiceProvider Services => ((App)Current)._host!.Services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("C_CBD") ?? throw new InvalidOperationException("No se encontró la cadena de conexión C_CBD.");
                services.AddSingleton<IConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
                services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

                services.AddScoped<IChatRepository, ChatRepository>();
                services.AddScoped<IMensajeRepository, MensajeRepository>();
                services.AddScoped<IMatchRepository, MatchRepository>();
                services.AddScoped<IPerfilRepository, PerfilRepository>();
                services.AddScoped<ICuentaRepository, CuentaRepository>();

                services.AddScoped<IChatService, ChatService>();
                services.AddScoped<IMensajeService, MensajeService>();
                services.AddScoped<IMatchService, MatchService>();
                services.AddScoped<IPerfilService, PerfilService>();
                services.AddScoped<ICuentaService, CuentaService>();

                services.AddTransient<LoginViewModel>();
                services.AddTransient<RegistroViewModel>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<ChatViewModel>();
                services.AddTransient<PerfilViewModel>();
                services.AddTransient<ConfiguracionViewModel>();

                services.AddTransient<LoginView>();
                services.AddTransient<RegistroView>();
                services.AddTransient<HomeView>();
                services.AddTransient<ChatView>();
                services.AddTransient<PerfilView>();
                services.AddTransient<ConfiguracionView>();
            })
            .Build();

        await _host.StartAsync().ConfigureAwait(false);

        var loginView = Services.GetRequiredService<LoginView>();
        loginView.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync().ConfigureAwait(false);
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
