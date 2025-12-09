namespace NetMAUI_Clase6_Crud_SQLLite.Views;

public partial class PacienteFormPage : ContentPage
{
    public PacienteFormPage()
    {
        InitializeComponent();
        BindingContext = App.Current.Services.GetService<PacienteViewModels>();
    }
}