using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using C_C.ViewModel;

namespace C_C.View;

public partial class RegistroView : Window
{
    private readonly RegistroViewModel _viewModel;

    public RegistroView(RegistroViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PasswordBox.Password = _viewModel.Password;
        ConfirmPasswordBox.Password = _viewModel.ConfirmPassword;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        Loaded -= OnLoaded;
        Unloaded -= OnUnloaded;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.Password = passwordBox.Password;
        }
    }

    private void OnConfirmPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.ConfirmPassword = passwordBox.Password;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RegistroViewModel.Password) && string.IsNullOrEmpty(_viewModel.Password) && PasswordBox.Password.Length > 0)
        {
            PasswordBox.Password = string.Empty;
        }

        if (e.PropertyName == nameof(RegistroViewModel.ConfirmPassword) && string.IsNullOrEmpty(_viewModel.ConfirmPassword) && ConfirmPasswordBox.Password.Length > 0)
        {
            ConfirmPasswordBox.Password = string.Empty;
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnMinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnDragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
