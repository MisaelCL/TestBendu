namespace ProyectoIMC;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        // Arrancamos la aplicación mostrando el shell principal.
        MainPage = new AppShell();
    }
}
