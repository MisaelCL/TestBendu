namespace NetMAUI_Clase6_Crud_SQLLite.Controllers;

[QueryProperty(nameof(Id), nameof(Id))]
public partial class PacienteDetalleController : ObservableObject
{
    private readonly IPacientes pacientesService;
    private readonly IDialogService dialogService;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool datosCargados;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private Paciente paciente = new();

    public PacienteDetalleController()
    {
        pacientesService = App.Current.Services.GetService<IPacientes>();
        dialogService = App.Current.Services.GetService<IDialogService>();
    }

    public async Task CargarAsync()
    {
        if (Id <= 0)
        {
            DatosCargados = true;
            return;
        }

        IsBusy = true;
        var registro = await pacientesService.GetById(Id);
        if (registro != null)
        {
            Paciente = registro;
        }

        IsBusy = false;
        DatosCargados = true;
    }

    [RelayCommand]
    public async Task EditarAsync()
    {
        await Shell.Current.GoToAsync($"/{nameof(PacienteFormPage)}?Id={Id}", false);
    }

    [RelayCommand]
    public async Task EliminarAsync()
    {
        if (Paciente == null)
        {
            return;
        }

        var aceptar = await dialogService.ShowAlertAsync("Eliminar", "¿Desea eliminar este paciente?", "Aceptar", "Cancelar");
        if (!aceptar)
        {
            return;
        }

        IsBusy = true;
        await pacientesService.DeletePaciente(Paciente);
        IsBusy = false;
        await Shell.Current.Navigation.PopAsync();
    }

    [RelayCommand]
    public async Task RegresarAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }
}