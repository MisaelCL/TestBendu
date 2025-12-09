namespace NetMAUI_Clase6_Crud_SQLLite.Views;

public partial class PacienteDetallePage : ContentPage
{
    public PacienteDetallePage()
    {
        this.InitializeComponent();
        BindingContext = App.Current.Services.GetService<PacienteDetalleViewModels>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PacienteDetalleViewModels viewModel)
        {
            await viewModel.CargarPaciente();
        }
    }
}