using ProyectoIMC.ViewModels;

namespace ProyectoIMC.Views;

public partial class PacientesPage : ContentPage
{
    private readonly PacientesListaViewModel _viewModel;

    public PacientesPage(PacientesListaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Cada vez que la vista vuelve a aparecer, refresco la lista para no mostrar datos viejos.
        await _viewModel.CargarPacientesAsync();
    }
}