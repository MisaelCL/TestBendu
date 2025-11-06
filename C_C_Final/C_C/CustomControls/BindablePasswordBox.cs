using System.Windows;
using System.Windows.Controls;

namespace C_C_Final.CustomControls
{
    public sealed class BindablePasswordBox : Control
    {
        public static readonly DependencyProperty SecurePasswordProperty = DependencyProperty.Register(
            nameof(SecurePassword),
            typeof(string),
            typeof(BindablePasswordBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSecurePasswordChanged));

        public string SecurePassword
        {
            get => (string)GetValue(SecurePasswordProperty);
            set => SetValue(SecurePasswordProperty, value);
        }

        private PasswordBox _passwordBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (_passwordBox != null)
            {
                _passwordBox.PasswordChanged -= OnPasswordChanged;
            }
            _passwordBox = GetTemplateChild("PART_PasswordBox") as PasswordBox;
            if (_passwordBox != null)
            {
                _passwordBox.PasswordChanged += OnPasswordChanged;
                _passwordBox.Password = SecurePassword;
            }
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_passwordBox != null && SecurePassword != _passwordBox.Password)
            {
                SecurePassword = _passwordBox.Password;
            }
        }

        private static void OnSecurePasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BindablePasswordBox box && box._passwordBox != null)
            {
                var newValue = e.NewValue as string ?? string.Empty;
                if (box._passwordBox.Password != newValue)
                {
                    box._passwordBox.Password = newValue;
                }
            }
        }
    }
}
