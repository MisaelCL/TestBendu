namespace NetMAUI_Clase6_Crud_SQLLite.Views;

public partial class PacienteFormPage : ContentPage
{
    public PacienteFormPage()
    {
        InitializeComponent();
        BindingContext = App.Current.Services.GetService<PacienteViewModels>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PacienteViewModels viewModel)
        {
            await viewModel.CargarPaciente();
        }
    }
}
