using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using ProyectoIMC.Model;
using ProyectoIMC.Repositories;
using ProyectoIMC.Views;

namespace ProyectoIMC.ViewModels
{
    public partial class PacientesListaViewModel : ObservableObject
    {
        private readonly IPacienteRepository _pacienteRepository;

        public ObservableCollection<Paciente> Pacientes { get; } = new();

        [ObservableProperty]
        private Paciente? pacienteSeleccionado;

        [ObservableProperty]
        private bool isBusy;

        public PacientesListaViewModel(IPacienteRepository pacienteRepository)
        {
            _pacienteRepository = pacienteRepository ?? throw new ArgumentNullException(nameof(pacienteRepository));
        }

        // CARGAR PACIENTES
        public async Task CargarPacientesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Pacientes.Clear();

                var lista = await _pacienteRepository.ListarTodosAsync();
                foreach (var p in lista)
                {
                    Pacientes.Add(p);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // NUEVO PACIENTE
        [RelayCommand]
        private async Task NuevoPacienteAsync()
        {
            await Shell.Current.GoToAsync(nameof(PacienteFormPage));
        }

        // EDITAR PACIENTE
        [RelayCommand]
        private async Task EditarPacienteAsync(Paciente? paciente)
        {
            var seleccionado = paciente ?? PacienteSeleccionado;
            if (seleccionado == null) return;

            await Shell.Current
                .GoToAsync($"{nameof(PacienteFormPage)}?IdPaciente={seleccionado.IdPaciente}");
        }

        // ELIMINAR PACIENTE
        [RelayCommand]
        private async Task EliminarPacienteAsync(Paciente? paciente)
        {
            var seleccionado = paciente ?? PacienteSeleccionado;
            if (seleccionado == null) return;

            var confirmado = await Application.Current.MainPage.DisplayAlert(
                "Confirmar",
                $"Eliminar al paciente {seleccionado.Nombre} {seleccionado.Apellido}?",
                "Si",
                "No");

            if (!confirmado) return;

            var id = seleccionado.IdPaciente;

            var ok = await _pacienteRepository.EliminarAsync(id);
            if (ok)
            {
                Pacientes.Remove(seleccionado);
                if (ReferenceEquals(PacienteSeleccionado, seleccionado))
                {
                    PacienteSeleccionado = null;
                }
            }
        }
    }
}
