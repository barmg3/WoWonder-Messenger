﻿using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Utils;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Viewer
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Locale | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
    public class VideoFullScreenActivity : AppCompatActivity
    {
        #region Variables Basic

        private ProgressBar ProgressBar;
        private VideoView PostVideoView;
        private string VideoUrl;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                Methods.App.FullScreenApp(this , true); 

                //ScreenOrientation.Portrait >>  Make to run your application only in portrait mode
                //ScreenOrientation.Landscape >> Make to run your application only in LANDSCAPE mode 
                //RequestedOrientation = ScreenOrientation.Landscape;

                SetContentView(Resource.Layout.VideoFullScreenLayout);

                VideoUrl = Intent?.GetStringExtra("videoUrl") ?? "";
                //var VideoDuration = Intent?.GetStringExtra("videoDuration") ?? "";

                //Get Value And Set Toolbar
                InitComponent(); 
            } 
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                var media = new MediaController(this);
                media.Show(5000);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progress_bar);
                ProgressBar.Visibility = ViewStates.Visible;
                 
                PostVideoView = FindViewById<VideoView>(Resource.Id.videoView);
                PostVideoView.Completion += PostVideoViewOnCompletion;
                PostVideoView.SetMediaController(media);
                PostVideoView.Prepared += PostVideoViewOnPrepared;
                PostVideoView.CanSeekBackward();
                PostVideoView.CanSeekForward();
                PostVideoView.SetAudioAttributes(new AudioAttributes.Builder()?.SetUsage(AudioUsageKind.Media)?.SetContentType(AudioContentType.Movie)?.Build());

                if (VideoUrl.Contains("http"))
                {
                    PostVideoView.SetVideoURI(Uri.Parse(VideoUrl)); 
                }
                else
                {
                    PostVideoView.SetVideoPath(VideoUrl);
                }

                TabbedMainActivity.GetInstance()?.SetWakeLock();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PostVideoViewOnPrepared(object sender, EventArgs e)
        {
            try
            {
                PostVideoView.Start();
                ProgressBar.Visibility = ViewStates.Invisible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PostVideoViewOnCompletion(object sender, EventArgs e)
        {
            try
            {
                PostVideoView.Pause();
                OnBackPressed();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        public override void OnBackPressed()
        {
            try
            {
                PostVideoView?.StopPlayback();
                PostVideoView = null!;

                TabbedMainActivity.GetInstance()?.OffWakeLock();

                base.OnBackPressed();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                base.OnBackPressed();
            }
        }

    }
}