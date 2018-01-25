using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MediaHelpers
{
    public class VideoPlayer : View, IVideoPlayerController
    {
        public event EventHandler UpdateStatus;

        public VideoPlayer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
  //              System.Diagnostics.Debug.WriteLine("Timer calling UpdateStatus");

                UpdateStatus?.Invoke(this, EventArgs.Empty);
                return true;
            });
        }


        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create("Source", typeof(VideoSource), typeof(VideoPlayer), null);

        public VideoSource Source
        {
            set { SetValue(SourceProperty, value); }
            get { return (VideoSource)GetValue(SourceProperty); }
        }

        public static readonly BindableProperty AutoPlayProperty =
            BindableProperty.Create("AutoPlay", typeof(bool), typeof(VideoPlayer), true);

        public bool AutoPlay
        {
            set { SetValue(AutoPlayProperty, value); }
            get { return (bool)GetValue(AutoPlayProperty); }
        }

        public static readonly BindableProperty AreTransportControlsEnabledProperty =
            BindableProperty.Create("AreTransportControlsEnabled", typeof(bool), typeof(VideoPlayer), true);

        public bool AreTransportControlsEnabled
        {
            set { SetValue(AreTransportControlsEnabledProperty, value); }
            get { return (bool)GetValue(AreTransportControlsEnabledProperty); }
        }

        // Status read-only property
        private static readonly BindablePropertyKey StatusPropertyKey =
            BindableProperty.CreateReadOnly("Status", typeof(VideoStatus), typeof(VideoPlayer), VideoStatus.None);

        public static readonly BindableProperty StatusProperty = StatusPropertyKey.BindableProperty;

        public VideoStatus Status
        {
            get { return (VideoStatus)GetValue(StatusProperty); }
        }

        VideoStatus IVideoPlayerController.Status
        {
            set { SetValue(StatusPropertyKey, value); }
            get { return Status; }
        }

        // CanPause read-only property
        private static readonly BindablePropertyKey CanPausePropertyKey =
            BindableProperty.CreateReadOnly("CanPause", typeof(bool), typeof(VideoPlayer), false);

        public static readonly BindableProperty CanPauseProperty = CanPausePropertyKey.BindableProperty;

        public bool CanPause
        {
            get { return (bool)GetValue(CanPauseProperty); }
        }

        bool IVideoPlayerController.CanPause
        {
            set { SetValue(CanPausePropertyKey, value); }
            get { return CanPause; }
        }

        // CanSeek read-only property
        private static readonly BindablePropertyKey CanSeekPropertyKey =
            BindableProperty.CreateReadOnly("CanSeek", typeof(bool), typeof(VideoPlayer), false);

        public static readonly BindableProperty CanSeekProperty = CanSeekPropertyKey.BindableProperty;

        public bool CanSeek
        {
            get { return (bool)GetValue(CanSeekProperty); }
        }

        bool IVideoPlayerController.CanSeek
        {
            set { SetValue(CanSeekPropertyKey, value); }
            get { return CanSeek; }
        }


        // Duration read-only property
        private static readonly BindablePropertyKey DurationPropertyKey =
            BindableProperty.CreateReadOnly("Duration", typeof(TimeSpan), typeof(VideoPlayer), new TimeSpan());

        public static readonly BindableProperty DurationProperty = DurationPropertyKey.BindableProperty;

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
        }

        TimeSpan IVideoPlayerController.Duration
        {
            set { SetValue(DurationPropertyKey, value); }
            get { return Duration; }
        }

        // Position property
        public static readonly BindableProperty PositionProperty =
            BindableProperty.Create("Position", typeof(TimeSpan), typeof(VideoPlayer), new TimeSpan());

        public TimeSpan Position
        {
            set { SetValue(PositionProperty, value); }
            get { return (TimeSpan)GetValue(PositionProperty); }
        }

        // Methods handled by renderers
        public event EventHandler PlayRequested;

        public void Play()
        {
            PlayRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler PauseRequested;

        public void Pause()
        {
            PauseRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StopRequested;

        public void Stop()
        {
            StopRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
