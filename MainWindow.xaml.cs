using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace AudiPlayer
{
    public partial class MainWindow : Window
    {
        private SpotifyService _spotifyService;
        private DispatcherTimer _updateTimer;
        private WinForms.NotifyIcon? _notifyIcon;
        private Settings _settings;
        private bool _isSpotifyReady = false;
        private int _currentProgressMs = 0;
        private int _totalDurationMs = 0;
        private bool _isVolumeChanging = false;
        private bool _isShuffleOn = false;
        private string _repeatState = "off";

        public MainWindow()
        {
            InitializeComponent();

            _settings = Settings.Load();
            ApplySettings();
            LoadStickers();

            Left = SystemParameters.PrimaryScreenWidth - Width - 20;
            Top = 20;

            SetupSystemTray();

            _spotifyService = new SpotifyService();
            InitializeSpotify();

            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            AlwaysOnTopMenuItem.IsChecked = _settings.AlwaysOnTop;
            this.Topmost = _settings.AlwaysOnTop;
        }

        private void ApplySettings()
        {
            try
            {
                if (_settings.UseGradients)
                {
                    iPodBodyGradient.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString(_settings.iPodBodyColor);
                    iPodBodyGradient.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString(_settings.iPodBodyColor2);
                    iPodBodyGradient.GradientStops[2].Color = (Color)ColorConverter.ConvertFromString(_settings.iPodBodyColor2);

                    ScreenBackgroundGradient.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString(_settings.ScreenBackgroundColor);
                    ScreenBackgroundGradient.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString(_settings.ScreenBackgroundColor2);

                    ClickWheelGradient.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString(_settings.ClickWheelColor);
                    ClickWheelGradient.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString(_settings.ClickWheelColor2);

                    ClickWheelCenterGradient.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString(_settings.ClickWheelCenterColor);
                    ClickWheelCenterGradient.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString(_settings.ClickWheelCenterColor2);

                    ProgressBarGradient.GradientStops[0].Color = (Color)ColorConverter.ConvertFromString(_settings.ProgressBarColor);
                    ProgressBarGradient.GradientStops[1].Color = (Color)ColorConverter.ConvertFromString(_settings.ProgressBarColor2);
                }
                else
                {
                    iPodBody.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.iPodBodyColor));
                    ScreenBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ScreenBackgroundColor));
                    ClickWheel.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ClickWheelColor));
                    ClickWheelCenter.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ClickWheelCenterColor));
                    ProgressBarFill.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ProgressBarColor));
                }

                TrackName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ScreenTextColor));
                ArtistName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ScreenSubtextColor));
                AlbumName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ScreenSubtextColor));
                CurrentTimeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ScreenSubtextColor));
                TotalTimeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ScreenSubtextColor));

                var buttonColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_settings.ButtonTextColor));
                MenuButton.Foreground = buttonColor;
                PlayPauseButton.Foreground = buttonColor;
                CenterButton.Foreground = buttonColor;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying settings: {ex.Message}", "Settings Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadStickers()
        {
            StickersCanvas.Children.Clear();

            if (_settings.Stickers != null)
            {
                foreach (var sticker in _settings.Stickers)
                {
                    AddStickerToCanvas(sticker.Emoji, sticker.X, sticker.Y, sticker.Size);
                }
            }
        }

        private void AddStickerToCanvas(string emoji, double x, double y, double size)
        {
            var textBlock = new TextBlock
            {
                Text = emoji,
                FontSize = size,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            StickersCanvas.Children.Add(textBlock);
        }

        private void SetupSystemTray()
        {
            _notifyIcon = new WinForms.NotifyIcon();
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            _notifyIcon.Text = "AudiPlayer";
            _notifyIcon.Visible = true;

            var contextMenu = new WinForms.ContextMenuStrip();
            contextMenu.Items.Add("Show Widget", null, (s, e) => { this.Show(); this.Visibility = Visibility.Visible; });
            contextMenu.Items.Add("Hide Widget", null, (s, e) => { this.Visibility = Visibility.Collapsed; });
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Settings", null, (s, e) => { OpenSettings(); });
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => { Application.Current.Shutdown(); });

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => { this.Show(); this.Visibility = Visibility.Visible; };
        }

        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                _settings = Settings.Load();
                ApplySettings();
                LoadStickers();
                AlwaysOnTopMenuItem.IsChecked = _settings.AlwaysOnTop;
                this.Topmost = _settings.AlwaysOnTop;
            }
        }

        private async void InitializeSpotify()
        {
            if (string.IsNullOrEmpty(_settings.ClientId) || string.IsNullOrEmpty(_settings.ClientSecret))
            {
                MessageBox.Show("Please configure your Spotify API credentials in Settings.\n\n" +
                              "See TUTORIAL_COMPLETE.md for detailed setup instructions.",
                              "Setup Required",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
                OpenSettings();
                return;
            }

            await _spotifyService.AuthenticateAsync();
            await System.Threading.Tasks.Task.Delay(3000);
            _isSpotifyReady = true;
            await UpdateNowPlaying();
            await LoadCurrentVolume();
            await LoadPlaybackState();
        }

        private async System.Threading.Tasks.Task LoadCurrentVolume()
        {
            try
            {
                var volume = await _spotifyService.GetVolumeAsync();
                if (volume.HasValue)
                {
                    _isVolumeChanging = true;
                    VolumeSlider.Value = volume.Value;
                    VolumeText.Text = $"{volume.Value}%";
                    _isVolumeChanging = false;
                }
            }
            catch { }
        }

        private async System.Threading.Tasks.Task LoadPlaybackState()
        {
            try
            {
                var playback = await _spotifyService.GetCurrentPlaybackAsync();
                if (playback != null)
                {
                    _isShuffleOn = playback.ShuffleState;
                    _repeatState = playback.RepeatState;
                    UpdateShuffleButton();
                    UpdateRepeatButton();
                }
            }
            catch { }
        }

        private void UpdateShuffleButton()
        {
            ShuffleButton.Opacity = _isShuffleOn ? 1.0 : 0.4;
            ShuffleButton.ToolTip = _isShuffleOn ? "Shuffle: On" : "Shuffle: Off";
        }

        private void UpdateRepeatButton()
        {
            RepeatButton.Opacity = _repeatState != "off" ? 1.0 : 0.4;
            RepeatButton.Content = _repeatState == "track" ? "🔂" : "🔁";
            RepeatButton.ToolTip = _repeatState == "off" ? "Repeat: Off" :
                                  _repeatState == "track" ? "Repeat: Track" : "Repeat: All";
        }

        private async void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            await UpdateNowPlaying();
        }

        private async System.Threading.Tasks.Task UpdateNowPlaying()
        {
            try
            {
                var currentTrack = await _spotifyService.GetCurrentlyPlayingAsync();

                if (currentTrack != null && currentTrack.Item is FullTrack track)
                {
                    TrackName.Text = track.Name;
                    ArtistName.Text = string.Join(", ", track.Artists.ConvertAll(a => a.Name));
                    AlbumName.Text = track.Album.Name;

                    if (track.Album.Images.Count > 0)
                    {
                        var imageUrl = track.Album.Images[0].Url;
                        AlbumArt.Source = new BitmapImage(new Uri(imageUrl));
                    }

                    PlayPauseBtn.Content = currentTrack.IsPlaying ? "⏸" : "▶";

                    _currentProgressMs = currentTrack.ProgressMs ?? 0;
                    _totalDurationMs = track.DurationMs;

                    UpdateProgressBar();
                }
                else
                {
                    TrackName.Text = "Not Playing";
                    ArtistName.Text = "Select a song in Spotify";
                    AlbumName.Text = "";
                    AlbumArt.Source = null;
                    PlayPauseBtn.Content = "▶";
                    _currentProgressMs = 0;
                    _totalDurationMs = 0;
                    UpdateProgressBar();
                }
            }
            catch
            {
                TrackName.Text = "Connection Error";
                ArtistName.Text = "Check Spotify";
            }
        }

        private void UpdateProgressBar()
        {
            if (_totalDurationMs > 0)
            {
                double percentage = (double)_currentProgressMs / _totalDurationMs;
                ProgressBarFill.Width = ProgressBarBackground.ActualWidth * percentage;

                TimeSpan current = TimeSpan.FromMilliseconds(_currentProgressMs);
                TimeSpan total = TimeSpan.FromMilliseconds(_totalDurationMs);

                CurrentTimeText.Text = $"{(int)current.TotalMinutes}:{current.Seconds:D2}";
                TotalTimeText.Text = $"{(int)total.TotalMinutes}:{total.Seconds:D2}";
            }
            else
            {
                ProgressBarFill.Width = 0;
                CurrentTimeText.Text = "0:00";
                TotalTimeText.Text = "0:00";
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSpotifyReady) return;
            await _spotifyService.PlayPauseAsync();
            await System.Threading.Tasks.Task.Delay(500);
            await UpdateNowPlaying();
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSpotifyReady) return;
            await _spotifyService.PreviousTrackAsync();
            await System.Threading.Tasks.Task.Delay(500);
            await UpdateNowPlaying();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSpotifyReady) return;
            await _spotifyService.NextTrackAsync();
            await System.Threading.Tasks.Task.Delay(500);
            await UpdateNowPlaying();
        }

        private async void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSpotifyReady) return;
            _isShuffleOn = !_isShuffleOn;
            await _spotifyService.ToggleShuffleAsync();
            UpdateShuffleButton();
        }

        private async void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSpotifyReady) return;
            await _spotifyService.ToggleRepeatAsync();

            _repeatState = _repeatState == "off" ? "context" :
                          _repeatState == "context" ? "track" : "off";
            UpdateRepeatButton();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private async void CenterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSpotifyReady) return;
            await _spotifyService.PlayPauseAsync();
            await System.Threading.Tasks.Task.Delay(500);
            await UpdateNowPlaying();
        }

        private async void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isSpotifyReady || _isVolumeChanging) return;

            int volume = (int)e.NewValue;
            VolumeText.Text = $"{volume}%";
            await _spotifyService.SetVolumeAsync(volume);
        }

        private void AlwaysOnTopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _settings.AlwaysOnTop = AlwaysOnTopMenuItem.IsChecked;
            this.Topmost = _settings.AlwaysOnTop;
            _settings.Save();
        }

        private void CustomizeStickersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var stickerWindow = new StickerCustomizationWindow(_settings);
            if (stickerWindow.ShowDialog() == true)
            {
                _settings = Settings.Load();
                LoadStickers();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _notifyIcon?.Dispose();
            base.OnClosed(e);
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private void HideMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
