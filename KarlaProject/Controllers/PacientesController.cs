namespace NetMAUI_Clase6_Crud_SQLLite.Controllers;

public partial class PacientesController : ObservableObject
{
    private readonly IPacientes pacientesService;

    public ObservableCollection<Paciente> Pacientes { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    public bool IsReady => !IsBusy;

    public PacientesController()
    {
        pacientesService = App.Current.Services.GetService<IPacientes>();
        Task.Run(async () => await ListarPacientes());
    }

    [RelayCommand]
    public async Task ListarPacientes()
    {
        IsBusy = true;
        Pacientes.Clear();
        var lista = await pacientesService.GetAll();
        foreach (var item in lista)
        {
            Pacientes.Add(item);
        }

        IsBusy = false;
        IsRefreshing = false;
    }

    [RelayCommand]
    public async Task IrANuevo()
    {
        await Shell.Current.GoToAsync($"/{nameof(PacienteFormPage)}", false);
    }

    [RelayCommand]
    public async Task VerDetalle(Paciente paciente)
    {
        await Shell.Current.GoToAsync($"/{nameof(PacienteDetallePage)}?Id={paciente.Id}", false);
    }
}