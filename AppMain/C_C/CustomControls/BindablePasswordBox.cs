using System.Windows;
using System.Windows.Controls;

namespace C_C_Final.CustomControls
{
    public sealed class BindablePasswordBox : PasswordBox
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

        public BindablePasswordBox()
        {
            PasswordChanged += OnPasswordChanged;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (SecurePassword != Password)
            {
                SecurePassword = Password;
            }
        }

        private static void OnSecurePasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BindablePasswordBox box)
            {
                var newValue = e.NewValue as string ?? string.Empty;
                if (box.Password != newValue)
                {
                    box.Password = newValue;
                }
            }
        }
    }
}
