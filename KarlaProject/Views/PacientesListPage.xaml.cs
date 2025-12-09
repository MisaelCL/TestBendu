namespace NetMAUI_Clase6_Crud_SQLLite.Views;

public partial class PacientesListPage : ContentPage
{
    public PacientesListPage()
    {
        InitializeComponent();
        BindingContext = App.Current.Services.GetService<PacientesListViewModels>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is PacientesListViewModels viewModel)
        {
            await viewModel.CargarPacientes();
        }
    }
}
