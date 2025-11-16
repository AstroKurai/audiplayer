using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace AudiPlayer
{
    public partial class SettingsWindow : Window
    {
        private Settings _settings;

        public SettingsWindow()
        {
            InitializeComponent();
            _settings = Settings.Load();
            LoadSettings();
        }

        private void LoadSettings()
        {
            ClientIdTextBox.Text = _settings.ClientId;
            ClientSecretTextBox.Text = _settings.ClientSecret;
            RedirectUriTextBox.Text = _settings.RedirectUri;
            RedirectPortTextBox.Text = _settings.RedirectPort.ToString();

            AlwaysOnTopCheckBox.IsChecked = _settings.AlwaysOnTop;

            iPodBodyColorTextBox.Text = _settings.iPodBodyColor;
            iPodBorderColorTextBox.Text = _settings.iPodBorderColor;
            ScreenBackgroundColorTextBox.Text = _settings.ScreenBackgroundColor;
            ScreenBorderColorTextBox.Text = _settings.ScreenBorderColor;
            ScreenTextColorTextBox.Text = _settings.ScreenTextColor;
            ScreenSubtextColorTextBox.Text = _settings.ScreenSubtextColor;
            ClickWheelColorTextBox.Text = _settings.ClickWheelColor;
            ClickWheelBorderColorTextBox.Text = _settings.ClickWheelBorderColor;
            ClickWheelCenterColorTextBox.Text = _settings.ClickWheelCenterColor;
            ButtonTextColorTextBox.Text = _settings.ButtonTextColor;
            ProgressBarColorTextBox.Text = _settings.ProgressBarColor;
            ProgressBarBackgroundColorTextBox.Text = _settings.ProgressBarBackgroundColor;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.ClientId = ClientIdTextBox.Text;
            _settings.ClientSecret = ClientSecretTextBox.Text;
            _settings.RedirectUri = RedirectUriTextBox.Text;

            if (int.TryParse(RedirectPortTextBox.Text, out int port))
            {
                _settings.RedirectPort = port;
            }

            _settings.AlwaysOnTop = AlwaysOnTopCheckBox.IsChecked ?? true;

            _settings.iPodBodyColor = iPodBodyColorTextBox.Text;
            _settings.iPodBorderColor = iPodBorderColorTextBox.Text;
            _settings.ScreenBackgroundColor = ScreenBackgroundColorTextBox.Text;
            _settings.ScreenBorderColor = ScreenBorderColorTextBox.Text;
            _settings.ScreenTextColor = ScreenTextColorTextBox.Text;
            _settings.ScreenSubtextColor = ScreenSubtextColorTextBox.Text;
            _settings.ClickWheelColor = ClickWheelColorTextBox.Text;
            _settings.ClickWheelBorderColor = ClickWheelBorderColorTextBox.Text;
            _settings.ClickWheelCenterColor = ClickWheelCenterColorTextBox.Text;
            _settings.ButtonTextColor = ButtonTextColorTextBox.Text;
            _settings.ProgressBarColor = ProgressBarColorTextBox.Text;
            _settings.ProgressBarBackgroundColor = ProgressBarBackgroundColorTextBox.Text;

            _settings.Save();

            System.Windows.MessageBox.Show("Settings saved successfully!",
                          "Settings Saved",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
