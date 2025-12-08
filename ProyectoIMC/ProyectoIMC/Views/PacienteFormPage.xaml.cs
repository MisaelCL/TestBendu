using ProyectoIMC.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProyectoIMC.Views;

public partial class PacienteFormPage : ContentPage, IQueryAttributable
{
    private readonly PacienteFormViewModel _viewModel;

    public PacienteFormPage(PacienteFormViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;

        BindingContext = viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        SincronizarPickers();
    }

    private void PickerSexo_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (BindingContext is PacienteFormViewModel vm && sender is Picker picker)
        {
            if (picker.SelectedIndex == 0)
                vm.Sexo = "M";
            else if (picker.SelectedIndex == 1)
                vm.Sexo = "F";
        }
    }

    private void PickerActividad_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (BindingContext is PacienteFormViewModel vm && sender is Picker picker)
        {
            vm.NivelActividad = picker.SelectedIndex + 1;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PacienteFormViewModel.Sexo) or nameof(PacienteFormViewModel.NivelActividad))
        {
            SincronizarPickers();
        }
    }

    private void SincronizarPickers()
    {
        SexoPicker.SelectedIndex = _viewModel.Sexo.Equals("F", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        ActividadPicker.SelectedIndex = Math.Clamp(_viewModel.NivelActividad - 1, 0, ActividadPicker.Items.Count - 1);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(PacienteFormViewModel.IdPaciente), out var idValue)
            && int.TryParse(idValue?.ToString(), out var id))
        {
            _viewModel.IdPaciente = id;
        }
    }
}
