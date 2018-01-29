using System;
using System.ComponentModel;
using System.IO;

using Android.Content;
using Android.Media;
using Android.Widget;
using ARelativeLayout = Android.Widget.RelativeLayout;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FormsVideoLibrary.VideoPlayer),
                          typeof(FormsVideoLibrary.Droid.VideoPlayerRenderer))]

namespace FormsVideoLibrary.Droid
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, ARelativeLayout>
    {
        VideoView videoView;
        MediaController mediaController;    // Used to display transport controls
        bool isPrepared;

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (Control == null)
            {
                // Save the VideoView for future reference
                videoView = new VideoView(Context);

                // Put the VideoView in a RelativeLayout
                ARelativeLayout relativeLayout = new ARelativeLayout(Context);
                relativeLayout.AddView(videoView);

                // Center the VideoView in the RelativeLayout
                ARelativeLayout.LayoutParams layoutParams =
                    new ARelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                layoutParams.AddRule(LayoutRules.CenterInParent);
                videoView.LayoutParameters = layoutParams;

                // Handle some VideoView events
                videoView.Prepared += OnVideoViewPrepared;
                videoView.Info += OnVideoViewInfo;
                videoView.Completion += OnVideoViewCompletion;
                videoView.Error += OnVideoViewError;

                // Use the RelativeLayout as the native control
                SetNativeControl(relativeLayout);
            }

            if (args.OldElement != null)
            {
                args.OldElement.UpdateStatus -= OnUpdateStatus;
                args.OldElement.PlayRequested -= OnPlayRequested;
                args.OldElement.PauseRequested -= OnPauseRequested;
                args.OldElement.StopRequested -= OnStopRequested;
            }

            if (args.NewElement != null)
            {
                SetSource();
                SetAreTransportControlsEnabled();

                args.NewElement.UpdateStatus += OnUpdateStatus;
                args.NewElement.PlayRequested += OnPlayRequested;
                args.NewElement.PauseRequested += OnPauseRequested;
                args.NewElement.StopRequested += OnStopRequested;
            }
        }

        // VideoView event handlers
        private void OnVideoViewPrepared(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Prepared!");

            isPrepared = true;

            ((IVideoPlayerController)Element).Duration = TimeSpan.FromMilliseconds(videoView.Duration);
        }

        private void OnVideoViewInfo(object sender, MediaPlayer.InfoEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Info: {0}", args.What);
        }

        private void OnVideoViewCompletion(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Completion!");
        }

        private void OnVideoViewError(object sender, MediaPlayer.ErrorEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("Error: {0}", args.What); 
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == VideoPlayer.SourceProperty.PropertyName)
            {
                SetSource();
            }
            else if (args.PropertyName == VideoPlayer.AreTransportControlsEnabledProperty.PropertyName)
            {
                SetAreTransportControlsEnabled();
            }
            else if (args.PropertyName == VideoPlayer.PositionProperty.PropertyName)
            {
                SetPosition();
            }
        }

        void SetSource()
        {
            isPrepared = false;
            bool hasSetSource = false;

            if (Element.Source is UriVideoSource)
            {
                string uri = (Element.Source as UriVideoSource).Uri;

                if (!String.IsNullOrWhiteSpace(uri))
                {
                    videoView.SetVideoURI(Android.Net.Uri.Parse(uri));
                    hasSetSource = true;
                }
            }
            else if (Element.Source is FileVideoSource)
            {
                string filename = (Element.Source as FileVideoSource).File;

                if (!String.IsNullOrWhiteSpace(filename))
                {
                    videoView.SetVideoPath(filename);
                    hasSetSource = true;
                }
            }
            else if (Element.Source is ResourceVideoSource)
            {
                string package = Context.PackageName;
                string path = (Element.Source as ResourceVideoSource).Path;

                if (!String.IsNullOrWhiteSpace(path))
                {
                    string filename = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                    string uri = "android.resource://" + package + "/raw/" + filename;
                    videoView.SetVideoURI(Android.Net.Uri.Parse(uri));
                    hasSetSource = true;
                }
            }

                

            // TODO: Is there an AutoPlay property to use instead of this logic?
            if (hasSetSource && Element.AutoPlay)
            {
                videoView.Start();
            }
        }

        void SetAreTransportControlsEnabled()
        {
            if (Element.AreTransportControlsEnabled)
            {
                mediaController = new MediaController(Context);
                // SetAnchorView and SetMediaPlayer seem to have the same effect 
                //     mediaController.SetAnchorView(videoView);
                mediaController.SetMediaPlayer(videoView);
                videoView.SetMediaController(mediaController);
            }
            else
            {
                videoView.SetMediaController(null);

                if (mediaController != null)
                {
                    mediaController.SetMediaPlayer(null);
                    mediaController = null;
                }
            }
        }

        // TODO: Can Move up 
        void SetPosition()
        {
            if (Math.Abs(videoView.CurrentPosition - Element.Position.TotalMilliseconds) > 1000)
            {
                videoView.SeekTo((int)Element.Position.TotalMilliseconds);
            }
        }


        bool isPlaying = false;

        // Event handler to update status
        void OnUpdateStatus(object sender, EventArgs args)
        {
//             if (videoView != null && Element != null)
            {
                if (isPlaying != videoView.IsPlaying)
                {
                    isPlaying = videoView.IsPlaying;
                    System.Diagnostics.Debug.WriteLine("IsPlaying = {0}", isPlaying);
                }

                VideoStatus status = VideoStatus.NotReady;

                if (isPrepared)
                {
                    status = isPlaying ? VideoStatus.Playing : VideoStatus.Paused;
                }

                ((IVideoPlayerController)Element).Status = status;






                TimeSpan timeSpan = TimeSpan.FromMilliseconds(videoView.CurrentPosition);
                ((IElementController)Element).SetValueFromRenderer(VideoPlayer.PositionProperty, timeSpan);
            }
        }

        // Event handlers to implement methods
        void OnPlayRequested(object sender, EventArgs args)
        {
            videoView.Start();
        }

        void OnPauseRequested(object sender, EventArgs args)
        {
            videoView.Pause();
        }

        void OnStopRequested(object sender, EventArgs args)
        {
            videoView.StopPlayback();
        }
    }
}