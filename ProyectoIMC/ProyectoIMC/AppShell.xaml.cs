using ProyectoIMC.Views;

namespace ProyectoIMC;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(PacienteFormPage), typeof(PacienteFormPage));
    }
}
