using NetMAUI_Clase6_Crud_SQLLite.Views;

namespace NetMAUI_Clase6_Crud_SQLLite;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(PacienteFormPage), typeof(PacienteFormPage));
        Routing.RegisterRoute(nameof(PacienteDetallePage), typeof(PacienteDetallePage));
        Routing.RegisterRoute(nameof(PacientesListPage), typeof(PacientesListPage));
    }
}