using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using ProyectoIMC.Model;
using ProyectoIMC.Repositories;
using ProyectoIMC.Services;
using System;
using System.Threading.Tasks;

namespace ProyectoIMC.ViewModels
{
    [QueryProperty(nameof(IdPaciente), "IdPaciente")]
    public partial class PacienteFormViewModel : ObservableObject
    {
        private readonly IPacienteRepository _pacienteRepository;

        [ObservableProperty] private int idPaciente;
        [ObservableProperty] private string nombre = string.Empty;
        [ObservableProperty] private string apellido = string.Empty;
        [ObservableProperty] private int? edad;
        [ObservableProperty] private double? pesoKg;
        [ObservableProperty] private double? estaturaCm;
        [ObservableProperty] private string sexo = "M";
        [ObservableProperty] private int nivelActividad = 1;

        public int SexoIndex
        {
            get => string.Equals(Sexo, "F", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            set => Sexo = value == 1 ? "F" : "M";
        }

        public int NivelActividadIndex
        {
            get => Math.Clamp(NivelActividad - 1, 0, 4);
            set => NivelActividad = Math.Clamp(value + 1, 1, 5);
        }

        [ObservableProperty] private double imc;
        [ObservableProperty] private string clasificacionImc = string.Empty;
        [ObservableProperty] private double porcentajeGrasa;
        [ObservableProperty] private double pesoIdeal;
        [ObservableProperty] private double tdee;

        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private string? errorMessage;

        public PacienteFormViewModel(IPacienteRepository pacienteRepository)
        {
            _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
        }

        partial void OnIdPacienteChanged(int value)
        {
            if (value > 0)
            {
                _ = CargarAsync(value);
            }
        }

        private async Task CargarAsync(int id)
        {
            var paciente = await _pacienteRepository.ObtenerPorIdAsync(id);
            if (paciente == null) return;

            IdPaciente = paciente.IdPaciente;
            Nombre = paciente.Nombre;
            Apellido = paciente.Apellido;
            Edad = paciente.Edad;
            PesoKg = paciente.PesoKg;
            EstaturaCm = paciente.EstaturaCm;
            Sexo = paciente.Sexo;
            NivelActividad = paciente.NivelActividad;

            CalcularIndicadores();
        }

        [RelayCommand]
        private void CalcularIndicadores()
        {
            ErrorMessage = null;

            var paciente = ConstruirPaciente();
            var imc = SaludCalculoService.CalcularImc(paciente);
            var clasImc = SaludCalculoService.ClasificarImc(imc);
            var grasa = SaludCalculoService.CalcularPorcentajeGrasa(paciente, imc);
            var pesoIdeal = SaludCalculoService.CalcularPesoIdeal(paciente);
            var tdee = SaludCalculoService.CalcularTdee(paciente);

            Imc = Math.Round(imc, 2);
            ClasificacionImc = clasImc;
            PorcentajeGrasa = Math.Round(grasa, 2);
            PesoIdeal = Math.Round(pesoIdeal, 2);
            Tdee = Math.Round(tdee, 2);
        }

        [RelayCommand]
        private async Task GuardarAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                if (string.IsNullOrWhiteSpace(Nombre) ||
                    string.IsNullOrWhiteSpace(Apellido))
                {
                    ErrorMessage = "Nombre y apellido son obligatorios.";
                    return;
                }

                if (!Edad.HasValue || !PesoKg.HasValue || !EstaturaCm.HasValue || Edad <= 0 || PesoKg <= 0 || EstaturaCm <= 0)
                {
                    ErrorMessage = "Edad, peso y estatura deben ser mayores que cero.";
                    return;
                }

                var paciente = ConstruirPaciente();
                var id = await _pacienteRepository.GuardarAsync(paciente);
                IdPaciente = id;

                CalcularIndicadores();

                await Application.Current.MainPage.DisplayAlert(
                    "Guardado",
                    "El paciente se guardó correctamente.",
                    "OK");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private Paciente ConstruirPaciente()
        {
            return new Paciente
            {
                IdPaciente = IdPaciente,
                Nombre = Nombre?.Trim() ?? string.Empty,
                Apellido = Apellido?.Trim() ?? string.Empty,
                Edad = Edad ?? 0,
                PesoKg = PesoKg ?? 0,
                EstaturaCm = EstaturaCm ?? 0,
                Sexo = Sexo,
                NivelActividad = NivelActividad
            };
        }

        partial void OnSexoChanged(string value) => OnPropertyChanged(nameof(SexoIndex));

        partial void OnNivelActividadChanged(int value) => OnPropertyChanged(nameof(NivelActividadIndex));
    }
}
