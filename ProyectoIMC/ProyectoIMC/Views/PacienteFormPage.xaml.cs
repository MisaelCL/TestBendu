using System.Collections.Generic;
using ProyectoIMC.ViewModels;

namespace ProyectoIMC.Views;

public partial class PacienteFormPage : ContentPage, IQueryAttributable
{
    private readonly PacienteFormViewModel _viewModel;

    public PacienteFormPage(PacienteFormViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // Si nos llega un Id por navegación, lo leo y se lo paso al VM para que cargue esos datos.
        if (query.TryGetValue(nameof(PacienteFormViewModel.IdPaciente), out var idValue)
            && int.TryParse(idValue?.ToString(), out var id))
        {
            _viewModel.IdPaciente = id;
        }
    }
}
