using System;
using System.Windows;
using C_C.Application.Repositories;
using C_C.Application.Services;
using C_C.Infrastructure.Common;
using C_C.Infrastructure.Repositories;
using C_C.Resources.utils;
using C_C.View;
using C_C.ViewModel;
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
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<SqlConnectionFactory>();
                services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

                services.AddScoped<ICuentaRepository, CuentaRepository>();
                services.AddScoped<IPerfilRepository, PerfilRepository>();
                services.AddScoped<IMatchRepository, MatchRepository>();

                services.AddScoped<IRegisterAlumnoService, RegisterAlumnoService>();
                services.AddScoped<IMatchService, MatchService>();

                services.AddTransient<LoginViewModel>();
                services.AddTransient<RegistroViewModel>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<ChatViewModel>();
                services.AddTransient<PerfilViewModel>();
                services.AddTransient<PreferenciasViewModel>();
                services.AddTransient<InboxViewModel>();
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
