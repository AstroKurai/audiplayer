using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;

namespace AudiPlayer
{
    public class SpotifyService
    {
        private SpotifyClient? _spotify;
        private EmbedIOAuthServer? _server;
        private Settings _settings;

        public SpotifyService()
        {
            _settings = Settings.Load();
        }

        public async Task<bool> AuthenticateAsync()
        {
            if (string.IsNullOrEmpty(_settings.ClientId) || string.IsNullOrEmpty(_settings.ClientSecret))
            {
                return false;
            }

            _server = new EmbedIOAuthServer(new Uri(_settings.RedirectUri), _settings.RedirectPort);
            await _server.Start();

            _server.AuthorizationCodeReceived += async (sender, response) =>
            {
                await _server.Stop();
                var config = SpotifyClientConfig.CreateDefault();
                var tokenResponse = await new OAuthClient(config).RequestToken(
                    new AuthorizationCodeTokenRequest(
                        _settings.ClientId,
                        _settings.ClientSecret,
                        response.Code,
                        new Uri(_settings.RedirectUri)
                    )
                );

                _spotify = new SpotifyClient(tokenResponse.AccessToken);
            };

            var request = new LoginRequest(_server.BaseUri, _settings.ClientId, LoginRequest.ResponseType.Code)
            {
                Scope = new[] {
                    Scopes.UserReadCurrentlyPlaying,
                    Scopes.UserReadPlaybackState,
                    Scopes.UserModifyPlaybackState
                }
            };

            BrowserUtil.Open(request.ToUri());
            return true;
        }

        public async Task<CurrentlyPlaying?> GetCurrentlyPlayingAsync()
        {
            if (_spotify == null) return null;
            try
            {
                return await _spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
            }
            catch
            {
                return null;
            }
        }

        public async Task<CurrentlyPlayingContext?> GetCurrentPlaybackAsync()
        {
            if (_spotify == null) return null;
            try
            {
                return await _spotify.Player.GetCurrentPlayback();
            }
            catch
            {
                return null;
            }
        }

        public async Task PlayPauseAsync()
        {
            if (_spotify == null) return;
            try
            {
                var playback = await _spotify.Player.GetCurrentPlayback();
                if (playback == null) return;

                if (playback.IsPlaying)
                    await _spotify.Player.PausePlayback();
                else
                    await _spotify.Player.ResumePlayback();
            }
            catch
            {
            }
        }

        public async Task NextTrackAsync()
        {
            if (_spotify == null) return;
            try
            {
                await _spotify.Player.SkipNext();
            }
            catch
            {
            }
        }

        public async Task PreviousTrackAsync()
        {
            if (_spotify == null) return;
            try
            {
                await _spotify.Player.SkipPrevious();
            }
            catch
            {
            }
        }

        public async Task ToggleShuffleAsync()
        {
            if (_spotify == null) return;
            try
            {
                var playback = await _spotify.Player.GetCurrentPlayback();
                if (playback == null) return;

                await _spotify.Player.SetShuffle(new PlayerShuffleRequest(!playback.ShuffleState));
            }
            catch
            {
            }
        }

        public async Task ToggleRepeatAsync()
        {
            if (_spotify == null) return;
            try
            {
                var playback = await _spotify.Player.GetCurrentPlayback();
                if (playback == null) return;

                var newState = playback.RepeatState == "off" ? "context" :
                              playback.RepeatState == "context" ? "track" : "off";

                var repeatState = newState == "context" ? PlayerSetRepeatRequest.State.Context :
                                newState == "track" ? PlayerSetRepeatRequest.State.Track :
                                PlayerSetRepeatRequest.State.Off;

                await _spotify.Player.SetRepeat(new PlayerSetRepeatRequest(repeatState));
            }
            catch
            {
            }
        }

        public async Task SetVolumeAsync(int volumePercent)
        {
            if (_spotify == null) return;
            try
            {
                await _spotify.Player.SetVolume(new PlayerVolumeRequest(volumePercent));
            }
            catch
            {
            }
        }

        public async Task<int?> GetVolumeAsync()
        {
            if (_spotify == null) return null;
            try
            {
                var playback = await _spotify.Player.GetCurrentPlayback();
                return playback?.Device?.VolumePercent;
            }
            catch
            {
                return null;
            }
        }
    }
}
