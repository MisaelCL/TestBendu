using NetMAUI_Clase6_Crud_SQLLite.Interfaces;
using NetMAUI_Clase6_Crud_SQLLite.Services;
using NetMAUI_Clase6_Crud_SQLLite.ViewModels;
using NetMAUI_Clase6_Crud_SQLLite.Views;

namespace NetMAUI_Clase6_Crud_SQLLite;

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


        // Servicios
        builder.Services.AddSingleton<IPacientes, PacientesService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();

        // ViewModels
        builder.Services.AddTransient<PacientesListViewModels>();
        builder.Services.AddTransient<PacienteViewModels>();
        builder.Services.AddTransient<PacienteDetalleViewModels>();

        // Views
        builder.Services.AddTransient<PacientesListPage>();
        builder.Services.AddTransient<PacienteFormPage>();
        builder.Services.AddTransient<PacienteDetallePage>();

        return builder.Build();
    }
}