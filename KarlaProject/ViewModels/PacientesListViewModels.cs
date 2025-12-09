using NetMAUI_Clase6_Crud_SQLLite.Interfaces;
using NetMAUI_Clase6_Crud_SQLLite.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NetMAUI_Clase6_Crud_SQLLite.ViewModels;

public partial class PacientesListViewModels : ObservableObject
{
    private readonly IPacientes pacientesService;

    public ObservableCollection<Paciente> Pacientes { get; } = new();

    public PacientesListViewModels()
    {
        pacientesService = App.Current.Services.GetService<IPacientes>();
    }

    [RelayCommand]
    public async Task CargarPacientes()
    {
        Pacientes.Clear();
        var lista = await pacientesService.GetAll();
        foreach (var p in lista)
            Pacientes.Add(p);
    }
}