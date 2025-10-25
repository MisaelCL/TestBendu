using System.Windows.Input;
using C_C.ViewModel.Commands;

namespace C_C.ViewModel;

public sealed class ConfiguracionViewModel : BaseViewModel
{
    private bool _modoOscuro;

    public ConfiguracionViewModel()
    {
        ToggleModoOscuroCommand = new RelayCommand(_ => ModoOscuro = !ModoOscuro);
    }

    public bool ModoOscuro
    {
        get => _modoOscuro;
        set => SetProperty(ref _modoOscuro, value);
    }

    public ICommand ToggleModoOscuroCommand { get; }
}
