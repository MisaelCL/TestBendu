using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using ProyectoIMC.Data;
using ProyectoIMC.Repositories;
using ProyectoIMC.ViewModels;
using ProyectoIMC.Views;
using SQLitePCL;

namespace ProyectoIMC;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ✅ Inicializar SQLite con el bundle e_sqlite3
        Batteries_V2.Init();

        // Servicios
        builder.Services.AddSingleton<AppDatabase>();
        builder.Services.AddSingleton<IPacienteRepository, PacienteRepository>();

        // ViewModels
        builder.Services.AddSingleton<PacientesListaViewModel>();
        builder.Services.AddTransient<PacienteFormViewModel>();

        // Views
        builder.Services.AddSingleton<PacientesPage>();
        builder.Services.AddTransient<PacienteFormPage>();

        // Devuelve la app lista para correr; no tiene más misterio.
        return builder.Build();
    }
}
