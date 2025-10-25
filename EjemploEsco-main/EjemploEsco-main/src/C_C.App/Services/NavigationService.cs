using System;
using System.Collections.Concurrent;
using System.Windows.Controls;
using C_C.App.ViewModel;
using C_C.App.Views;

namespace C_C.App.Services;

public class NavigationService
{
    private readonly ConcurrentDictionary<Type, Type> _map = new();

    public NavigationService()
    {
        Register<LoginView, LoginViewModel>();
        Register<RegistroView, RegistroViewModel>();
        Register<DescubrirView, DescubrirViewModel>();
        Register<MatchesView, MatchesViewModel>();
        Register<ChatListView, ChatListViewModel>();
        Register<ChatView, ChatViewModel>();
        Register<PerfilView, PerfilViewModel>();
        Register<PreferenciasView, PreferenciasViewModel>();
        Register<AjustesView, AjustesViewModel>();
    }

    public void Register<TView, TViewModel>()
        where TView : UserControl
        where TViewModel : ViewModelBase
    {
        _map[typeof(TView)] = typeof(TViewModel);
    }

    public Type ResolveViewModelType(Type viewType)
    {
        if (_map.TryGetValue(viewType, out var viewModelType))
        {
            return viewModelType;
        }

        throw new InvalidOperationException($"No view model registered for view {viewType.Name}");
    }
}
