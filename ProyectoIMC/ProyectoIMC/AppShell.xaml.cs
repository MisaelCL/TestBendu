using ProyectoIMC.Views;

namespace ProyectoIMC;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registro la ruta del formulario para poder navegar con Shell.
        Routing.RegisterRoute(nameof(PacienteFormPage), typeof(PacienteFormPage));
    }
}
