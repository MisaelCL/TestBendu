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
        await _viewModel.CargarPacientesAsync();
    }
}