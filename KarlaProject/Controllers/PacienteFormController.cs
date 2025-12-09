using CommunityToolkit.Mvvm.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetMAUI_Clase6_Crud_SQLLite.Controllers;

[QueryProperty(nameof(Id), nameof(Id))]
public partial class PacienteFormController : ObservableValidator
{
    private readonly IPacientes pacientesService;

    public ObservableCollection<string> Errores { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool datosCargados;

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    private string nombre = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "El apellido es obligatorio")]
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    private string apellido = string.Empty;

    [ObservableProperty]
    [Range(1, 120, ErrorMessage = "La edad debe estar entre 1 y 120")]
    private int edad = 18;

    [ObservableProperty]
    [Range(1, 500, ErrorMessage = "El peso debe ser mayor a cero")]
    private double pesoKg = 70;

    [ObservableProperty]
    [Range(50, 260, ErrorMessage = "La estatura debe estar entre 50 y 260 cm")]
    private double estaturaCm = 170;

    [ObservableProperty]
    private Sexo sexo = Sexo.Masculino;

    [ObservableProperty]
    private NivelActividad nivelActividad = NivelActividad.Sedentario;

    [ObservableProperty]
    private double imc;

    [ObservableProperty]
    private string estadoImc = string.Empty;

    [ObservableProperty]
    private double porcentajeGrasa;

    [ObservableProperty]
    private string clasificacionGrasa = string.Empty;

    [ObservableProperty]
    private double pesoIdeal;

    [ObservableProperty]
    private double bmr;

    [ObservableProperty]
    private double tdee;

    public PacienteFormController()
    {
        pacientesService = App.Current.Services.GetService<IPacientes>();
        RecalcularMetrica();
    }

    partial void OnNombreChanged(string value) => RecalcularMetrica();
    partial void OnApellidoChanged(string value) => RecalcularMetrica();
    partial void OnEdadChanged(int value) => RecalcularMetrica();
    partial void OnPesoKgChanged(double value) => RecalcularMetrica();
    partial void OnEstaturaCmChanged(double value) => RecalcularMetrica();
    partial void OnSexoChanged(Sexo value) => RecalcularMetrica();
    partial void OnNivelActividadChanged(NivelActividad value) => RecalcularMetrica();

    public async Task CargarPacienteAsync()
    {
        if (Id <= 0)
        {
            DatosCargados = true;
            return;
        }

        IsBusy = true;
        var paciente = await pacientesService.GetById(Id);
        if (paciente != null)
        {
            Nombre = paciente.Nombre;
            Apellido = paciente.Apellido;
            Edad = paciente.Edad;
            PesoKg = paciente.PesoKg;
            EstaturaCm = paciente.EstaturaCm;
            Sexo = paciente.Sexo;
            NivelActividad = paciente.NivelActividad;
            ActualizarDerivados(paciente);
        }

        IsBusy = false;
        DatosCargados = true;
    }

    [RelayCommand]
    public async Task GuardarAsync()
    {
        IsBusy = true;
        Errores.Clear();
        ValidateAllProperties();

        AgregarErrores(nameof(Nombre), "Nombre");
        AgregarErrores(nameof(Apellido), "Apellido");
        AgregarErrores(nameof(Edad), "Edad");
        AgregarErrores(nameof(PesoKg), "Peso");
        AgregarErrores(nameof(EstaturaCm), "Estatura");

        if (Errores.Any())
        {
            IsBusy = false;
            return;
        }

        var paciente = new Paciente
        {
            Id = Id,
            Nombre = Nombre,
            Apellido = Apellido,
            Edad = Edad,
            PesoKg = PesoKg,
            EstaturaCm = EstaturaCm,
            Sexo = Sexo,
            NivelActividad = NivelActividad
        };

        paciente.RecalcularMetricas();
        ActualizarDerivados(paciente);

        if (paciente.Id == 0)
        {
            Id = await pacientesService.InsertPaciente(paciente);
        }
        else
        {
            await pacientesService.UpdatePaciente(paciente);
        }

        IsBusy = false;
        await Shell.Current.DisplayAlert("Éxito", "Paciente guardado correctamente", "Aceptar");
        await Shell.Current.Navigation.PopAsync();
    }

    private void RecalcularMetrica()
    {
        var paciente = new Paciente
        {
            Nombre = Nombre,
            Apellido = Apellido,
            Edad = Edad,
            PesoKg = PesoKg,
            EstaturaCm = EstaturaCm,
            Sexo = Sexo,
            NivelActividad = NivelActividad
        };

        paciente.RecalcularMetricas();
        ActualizarDerivados(paciente);
    }

    private void ActualizarDerivados(Paciente paciente)
    {
        Imc = paciente.Imc;
        EstadoImc = paciente.EstadoImc;
        PorcentajeGrasa = paciente.PorcentajeGrasa;
        ClasificacionGrasa = paciente.ClasificacionGrasa;
        PesoIdeal = paciente.PesoIdeal;
        Bmr = paciente.Bmr;
        Tdee = paciente.Tdee;
    }

    private void AgregarErrores(string propertyName, string etiqueta)
    {
        foreach (var error in GetErrors(propertyName))
        {
            if (error is ValidationResult validation)
            {
                Errores.Add($"{etiqueta}: {validation.ErrorMessage}");
            }
        }
    }

    [RelayCommand]
    public async Task CancelarAsync()
    {
        await Shell.Current.Navigation.PopAsync();
    }
}