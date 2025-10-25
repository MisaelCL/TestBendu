using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using C_C.App.Services;
using C_C.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace C_C.App.ViewModel;

public class MainWindowViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;
    private readonly IServiceScopeFactory _scopeFactory;
    private IServiceScope? _currentScope;
    private object? _currentView;

    public MainWindowViewModel(NavigationService navigationService, IServiceProvider serviceProvider)
    {
        _navigationService = navigationService;
        _scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        NavigateLoginCommand = new ViewModelCommand(async _ => await NavigateTo<LoginView>());
        NavigateRegistroCommand = new ViewModelCommand(async _ => await NavigateTo<RegistroView>());
        NavigateDescubrirCommand = new ViewModelCommand(async _ => await NavigateTo<DescubrirView>());
        NavigateMatchesCommand = new ViewModelCommand(async _ => await NavigateTo<MatchesView>());
        NavigateChatListCommand = new ViewModelCommand(async _ => await NavigateTo<ChatListView>());
        NavigatePerfilCommand = new ViewModelCommand(async _ => await NavigateTo<PerfilView>());
        NavigatePreferenciasCommand = new ViewModelCommand(async _ => await NavigateTo<PreferenciasView>());
        NavigateAjustesCommand = new ViewModelCommand(async _ => await NavigateTo<AjustesView>());

        _ = NavigateTo<LoginView>();
    }

    public ViewModelCommand NavigateLoginCommand { get; }
    public ViewModelCommand NavigateRegistroCommand { get; }
    public ViewModelCommand NavigateDescubrirCommand { get; }
    public ViewModelCommand NavigateMatchesCommand { get; }
    public ViewModelCommand NavigateChatListCommand { get; }
    public ViewModelCommand NavigatePerfilCommand { get; }
    public ViewModelCommand NavigatePreferenciasCommand { get; }
    public ViewModelCommand NavigateAjustesCommand { get; }

    public object? CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    private Task NavigateTo<TView>() where TView : UserControl
    {
        _currentScope?.Dispose();
        _currentScope = _scopeFactory.CreateScope();
        var view = _currentScope.ServiceProvider.GetRequiredService<TView>();
        var viewModelType = _navigationService.ResolveViewModelType(typeof(TView));
        var viewModel = _currentScope.ServiceProvider.GetRequiredService(viewModelType);
        view.DataContext = viewModel;
        CurrentView = view;
        return Task.CompletedTask;
    }
}
