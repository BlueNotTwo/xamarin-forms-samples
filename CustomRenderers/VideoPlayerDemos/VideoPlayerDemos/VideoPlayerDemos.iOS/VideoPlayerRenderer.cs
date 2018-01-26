using System;
using System.ComponentModel;
using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using AVFoundation;
using AVKit;
using CoreMedia;
using Foundation;
using UIKit;

using MediaHelpers;
using MediaHelpers.iOS;

[assembly: ExportRenderer(typeof(VideoPlayer), typeof(VideoPlayerRenderer))]

namespace MediaHelpers.iOS
{
    public class VideoPlayerRenderer : ViewRenderer<VideoPlayer, UIView>
    {
        AVPlayerViewController _playerViewController;       // solely for ViewController property

        public override UIViewController ViewController => _playerViewController;

        protected override void OnElementChanged(ElementChangedEventArgs<VideoPlayer> args)
        {
            base.OnElementChanged(args);

            if (Control == null)
            {
                _playerViewController = new AVPlayerViewController();
                AVPlayer player = new AVPlayer();
                _playerViewController.Player = player;
                SetNativeControl(_playerViewController.View);


                System.Diagnostics.Debug.WriteLine("Initial Status: {0} Time-Control Status: {1}", player.Status, player.TimeControlStatus);

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
                // TODO: Notice no SetPosition property

                AVPlayer player = ((AVPlayerViewController)ViewController).Player;

                TimeSpan controlPosition = ConvertTime(player.CurrentTime);

                if (Math.Abs((controlPosition - Element.Position).TotalSeconds) > 1)
                {
                    player.Seek(CMTime.FromSeconds(Element.Position.TotalSeconds, 1));
                }
            }
        }

        void SetSource()
        {
            AVPlayerItem playerItem = null;

            if (Element.Source != null)
            {
                AVAsset asset = null;

                if (Element.Source is UriVideoSource)
                {
                    string uri = (Element.Source as UriVideoSource).Uri;
                    asset = AVAsset.FromUrl(new NSUrl(uri));
                }
                else if (Element.Source is FileVideoSource)
                {
                    string uri = (Element.Source as FileVideoSource).File;
                    asset = AVAsset.FromUrl(new NSUrl(uri));
                }
                else if (Element.Source is ResourceVideoSource)
                {
                    string path = (Element.Source as ResourceVideoSource).Path;
                    string directory = Path.GetDirectoryName(path);
                    string filename = Path.GetFileNameWithoutExtension(path);
                    string extension = Path.GetExtension(path).Substring(1);
                    NSUrl url = NSBundle.MainBundle.GetUrlForResource(filename, extension, directory);
                    asset = AVAsset.FromUrl(url);
                }


                if (asset != null)
                {
                    playerItem = new AVPlayerItem(asset);

                    ((IVideoPlayerController)Element).Duration = ConvertTime(playerItem.Duration); 

                    observer = playerItem.AddObserver("duration", NSKeyValueObservingOptions.Initial, 
                        

                        (sender) =>
                                       // {
                                            ((IVideoPlayerController)Element).Duration = ConvertTime(playerItem.Duration) // ; //  TimeSpan.FromSeconds(playerItem.Duration.Seconds);
//                                        System.Diagnostics.Debug.WriteLine(playerItem.Duration.Seconds); //      TimeSpan.FromMilliseconds(playerItem.Duration.Value));
                //                        }
                
                );
/*
                    timeObserver = playerItem.AddObserver("currenttime", NSKeyValueObservingOptions.New,
                        (sender) =>
                        {
                            System.Diagnostics.Debug.WriteLine(TimeSpan.FromMilliseconds(playerItem.CurrentTime.Value));

                        });
  */                  
                }
            }
            
            ((AVPlayerViewController)ViewController).Player.ReplaceCurrentItemWithPlayerItem(playerItem);

            // TODO: Is there an AutoPlay property?

            if (playerItem != null && Element.AutoPlay)
            {
                ((AVPlayerViewController)ViewController).Player.Play();
            }
        }

        TimeSpan ConvertTime(CMTime cmTime)
        {
            return TimeSpan.FromSeconds(Double.IsNaN(cmTime.Seconds) ? 0 : cmTime.Seconds);

        }

        private IDisposable observer;
    //    private IDisposable timeObserver;
/*
        public void Observer(NSObservedChange nsObservedChange)
        {
            ((IVideoController)Element).Duration = TimeSpan.FromMilliseconds(Control.playerItem.Duration.Value);
        }
*/
        void SetAreTransportControlsEnabled()
        {
            ((AVPlayerViewController)ViewController).ShowsPlaybackControls = Element.AreTransportControlsEnabled;
        }

        AVPlayerStatus status;
        AVPlayerTimeControlStatus tcStatus;

        // Event handler to update status
        void OnUpdateStatus(object sender, EventArgs args)
        {
            AVPlayerStatus currentStatus = ((AVPlayerViewController)ViewController).Player.Status;

            if (status != currentStatus)
            {
                status = currentStatus;
                System.Diagnostics.Debug.WriteLine("Status: {0}", status);
            }

            AVPlayerTimeControlStatus currentTcStatus = ((AVPlayerViewController)ViewController).Player.TimeControlStatus;

            if (tcStatus != currentTcStatus)
            {
                tcStatus = currentTcStatus;
                System.Diagnostics.Debug.WriteLine("Time Control Status: {0}", tcStatus);
            }

            //       if (tcStatus == AVPlayerTimeControlStatus/)

            VideoStatus videoStatus = VideoStatus.Unknown;

            switch (status)
            {
                case AVPlayerStatus.ReadyToPlay:
                    switch (tcStatus)
                    {
                        case AVPlayerTimeControlStatus.Playing:
                            videoStatus = VideoStatus.Playing;
                            break;

                        case AVPlayerTimeControlStatus.Paused:
                            videoStatus = VideoStatus.Paused;
                            break;
                    }
                    break;

                default:
                    videoStatus = VideoStatus.NotReady;
                    break;
/*
                default:
                    switch (status)
                    {
                        case AVPlayerStatus.ReadyToPlay:
                            videoStatus = VideoStatus.NotReady;
                            break;

                        default:
                            videoStatus = VideoStatus.Unknown;
                            break;
                    }
                    break;
*/
            }

            ((IVideoPlayerController)Element).Status = videoStatus;



            CMTime cmTime = ((AVPlayerViewController)ViewController).Player.CurrentTime;
            //     seconds = Double.IsNaN(seconds) ? 0 : seconds;

            ((IElementController)Element).SetValueFromRenderer(VideoPlayer.PositionProperty, ConvertTime(cmTime)); //  TimeSpan.FromSeconds(seconds));
        }

        // Event handlers to implement methods
        void OnPlayRequested(object sender, EventArgs args)
        {
            ((AVPlayerViewController)ViewController).Player.Play();
        }

        void OnPauseRequested(object sender, EventArgs args)
        {
            ((AVPlayerViewController)ViewController).Player.Pause();
        }

        void OnStopRequested(object sender, EventArgs args)
        {
            ((AVPlayerViewController)ViewController).Player.Pause();
            ((AVPlayerViewController)ViewController).Player.Seek(new CMTime(0, 1));
        }
    }
}