using NetMAUI_Clase6_Crud_SQLLite.Interfaces;
using NetMAUI_Clase6_Crud_SQLLite.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NetMAUI_Clase6_Crud_SQLLite.ViewModels;

public partial class PacientesListViewModels : ObservableObject
{
    private readonly IPacientes pacientesService;

    public ObservableCollection<Paciente> Pacientes { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isRefreshing;

    public bool IsReady => !IsBusy;

    public PacientesListViewModels()
    {
        pacientesService = App.Current.Services.GetService<IPacientes>();
    }

    [RelayCommand]
    public async Task CargarPacientes()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        Pacientes.Clear();
        var lista = await pacientesService.GetAll();
        foreach (var p in lista)
        {
            Pacientes.Add(p);
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