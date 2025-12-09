namespace NetMAUI_Clase6_Crud_SQLLite.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
public partial class PacienteDetalleViewModels : ObservableObject
{
    private readonly IPacientes pacientesService;
    private readonly IDialogService dialogService;

    [ObservableProperty]
    private Paciente paciente = new();

    [ObservableProperty]
    private int id;

    public PacienteDetalleViewModels()
    {
        pacientesService = App.Current.Services.GetService<IPacientes>();
        dialogService = App.Current.Services.GetService<IDialogService>();
    }

    public async Task CargarPaciente()
    {
        if (Id <= 0) return;
        var encontrado = await pacientesService.GetById(Id);
        if (encontrado != null)
        {
            Paciente = encontrado;
        }
    }

    [RelayCommand]
    public async Task Editar()
    {
        await Shell.Current.GoToAsync($"/{nameof(PacienteFormPage)}?Id={Paciente.Id}");
    }

    [RelayCommand]
    public async Task Eliminar()
    {
        var confirmar = await dialogService.ShowAlertAsync("Eliminar", $"¿Desea eliminar a {Paciente.Nombre} {Paciente.Apellido}?", "Aceptar", "Cancelar");
        if (!confirmar) return;
        await pacientesService.DeletePaciente(Paciente);
        await Shell.Current.Navigation.PopToRootAsync();
    }

    [RelayCommand]
    public async Task Regresar()
    {
        await Shell.Current.Navigation.PopAsync();
    }
}