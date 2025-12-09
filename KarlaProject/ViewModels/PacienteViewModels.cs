using CommunityToolkit.Mvvm.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace NetMAUI_Clase6_Crud_SQLLite.ViewModels;

[QueryProperty(nameof(Id), nameof(Id))]
public partial class PacienteViewModels : ObservableValidator
{
    private readonly IPacientes pacientesService;
    public ObservableCollection<string> Errores { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    private bool isBusy;

    [ObservableProperty]
    private bool isVisible;

    public bool IsEnabled => !IsVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DatosCargados))]
    private int id;

    private string nombre = string.Empty;
    [Required(ErrorMessage = "El campo nombre es obligatorio")]
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string Nombre
    {
        get => nombre;
        set => SetProperty(ref nombre, value, true);
    }

    private string apellido = string.Empty;
    [Required(ErrorMessage = "El campo apellido es obligatorio")]
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string Apellido
    {
        get => apellido;
        set => SetProperty(ref apellido, value, true);
    }

    private int edad = 18;
    [Range(1, 120, ErrorMessage = "La edad debe estar entre 1 y 120 años")]
    public int Edad
    {
        get => edad;
        set
        {
            SetProperty(ref edad, value, true);
            RecalcularMetricas();
        }
    }

    private double pesoKg = 70;
    [Range(1, 500, ErrorMessage = "El peso debe ser mayor a cero")]
    public double PesoKg
    {
        get => pesoKg;
        set
        {
            SetProperty(ref pesoKg, value, true);
            RecalcularMetricas();
        }
    }

    private double estaturaCm = 170;
    [Range(50, 260, ErrorMessage = "La estatura debe estar entre 50 y 260 cm")]
    public double EstaturaCm
    {
        get => estaturaCm;
        set
        {
            SetProperty(ref estaturaCm, value, true);
            RecalcularMetricas();
        }
    }

    [ObservableProperty]
    private Sexo sexo = Sexo.Masculino;

    partial void OnSexoChanged(Sexo value) => RecalcularMetricas();

    [ObservableProperty]
    private NivelActividad nivelActividad = NivelActividad.Sedentario;

    partial void OnNivelActividadChanged(NivelActividad value) => RecalcularMetricas();

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

    public bool DatosCargados => Id >= 0;

    public PacienteViewModels()
    {
        pacientesService = App.Current.Services.GetService<IPacientes>();
    }

    public async Task CargarPaciente()
    {
        if (Id > 0)
        {
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
                RecalcularMetricas();
            }
        }
        else
        {
            RecalcularMetricas();
        }
    }

    private void RecalcularMetricas()
    {
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
        Imc = paciente.Imc;
        EstadoImc = paciente.EstadoImc;
        PorcentajeGrasa = paciente.PorcentajeGrasa;
        ClasificacionGrasa = paciente.ClasificacionGrasa;
        PesoIdeal = paciente.PesoIdeal;
        Bmr = paciente.Bmr;
        Tdee = paciente.Tdee;
    }

    [RelayCommand]
    public async Task Guardar()
    {
        IsBusy = true;
        IsVisible = false;
        ValidateAllProperties();

        Errores.Clear();
        GetErrors(nameof(Nombre)).ToList().ForEach(err => Errores.Add("Nombre: " + err.ErrorMessage));
        GetErrors(nameof(Apellido)).ToList().ForEach(err => Errores.Add("Apellido: " + err.ErrorMessage));
        GetErrors(nameof(Edad)).ToList().ForEach(err => Errores.Add("Edad: " + err.ErrorMessage));
        GetErrors(nameof(PesoKg)).ToList().ForEach(err => Errores.Add("Peso: " + err.ErrorMessage));
        GetErrors(nameof(EstaturaCm)).ToList().ForEach(err => Errores.Add("Estatura: " + err.ErrorMessage));

        IsBusy = false;
        if (Errores.Count > 0) return;

        IsBusy = true;
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

        if (Id == 0)
        {
            Id = await pacientesService.InsertPaciente(paciente);
        }
        else
        {
            await pacientesService.UpdatePaciente(paciente);
        }

        IsBusy = false;
        IsVisible = true;

        await Shell.Current.Navigation.PopToRootAsync();
    }

    [RelayCommand]
    public async Task Cancelar()
    {
        await Shell.Current.Navigation.PopAsync();
    }
}