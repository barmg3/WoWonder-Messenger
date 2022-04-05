using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AT.Markushi.UI;
using DT.Xamarin.Agora;
using DT.Xamarin.Agora.Video;
using Newtonsoft.Json;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;

namespace WoWonder.Frameworks.Agora
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ResizeableActivity = true, SupportsPictureInPicture = true)]
    public class AgoraVideoCallActivity : AppCompatActivity, ISensorEventListener
    {
        #region Variables Basic

        private string CallType = "0", Token = "";
        private CallUserObject CallUserObject;

        private const int MaxLocalVideoDimension = 150;
        private RtcEngine AgoraEngine;
        private AgoraRtcHandler AgoraHandler;
        private bool IsVideoEnabled = true;
        private SurfaceView LocalVideoView;

        //Controls
        private AppCompatButton SwitchCamButton;

        private CircleButton EndCallButton, MuteVideoButton, MuteAudioButton;
        private RelativeLayout UserInfoViewContainer;
        private ImageView UserImageView, PictureInToPictureButton;
        private TextView UserNameTextView, NoteTextView;

        private int CountSecondsOfOutGoingCall;
        private Timer TimerRequestWaiter;

        private TabbedMainActivity GlobalContext;

        private SensorManager SensorManager;
        private Sensor Proximity;
        private readonly int SensorSensitivity = 4;

        private PictureInPictureParams PictureInPictureParams;
        private RelativeLayout ThumbnailVideo;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                Window?.AddFlags(WindowManagerFlags.KeepScreenOn);

                // Create your application here
                SetContentView(Resource.Layout.AgoraVideoCallActivityLayout);

                SensorManager = (SensorManager)GetSystemService(SensorService);
                Proximity = SensorManager?.GetDefaultSensor(SensorType.Proximity);

                GlobalContext = TabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitAgoraCall();
                TabbedMainActivity.RunCall = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                SensorManager.RegisterListener(this, Proximity, SensorDelay.Normal);
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStart()
        {
            try
            {
                base.OnStart();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                SensorManager.UnregisterListener(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnRestart()
        {
            try
            {
                base.OnRestart();
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

        protected override void OnDestroy()
        {
            try
            {
                TabbedMainActivity.RunCall = false;
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                TabbedMainActivity.RunCall = false;
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    FinishCall();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                SwitchCamButton = FindViewById<AppCompatButton>(Resource.Id.switch_cam_button);
                MuteVideoButton = FindViewById<CircleButton>(Resource.Id.mute_video_button);
                EndCallButton = FindViewById<CircleButton>(Resource.Id.end_call_button);
                MuteAudioButton = FindViewById<CircleButton>(Resource.Id.mute_audio_button);

                UserInfoViewContainer = FindViewById<RelativeLayout>(Resource.Id.userInfoview_container);
                UserImageView = FindViewById<ImageView>(Resource.Id.userImageView);
                UserNameTextView = FindViewById<TextView>(Resource.Id.userNameTextView);
                NoteTextView = FindViewById<TextView>(Resource.Id.noteTextView);
                PictureInToPictureButton = FindViewById<ImageView>(Resource.Id.pictureintopictureButton);

                ThumbnailVideo = FindViewById<RelativeLayout>(Resource.Id.local_video_container);

                if (!PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture))
                {
                    PictureInToPictureButton.Visibility = ViewStates.Gone;
                }
                else
                {
                    PictureInPictureParams = new PictureInPictureParams.Builder().SetAspectRatio(new Rational(9, 16))
                        //.SetSourceRectHint(sourceRectHint)
                        ?.SetAutoEnterEnabled(true)
                        ?.Build();
                    SetPictureInPictureParams(PictureInPictureParams);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    SwitchCamButton.Click += SwitchCamButtonOnClick;
                    MuteVideoButton.Click += MuteVideoButtonOnClick;
                    EndCallButton.Click += EndCallButtonOnClick;
                    MuteAudioButton.Click += MuteAudioButtonOnClick;
                    PictureInToPictureButton.Click += PictureInToPictureButtonOnClick;
                }
                else
                {
                    SwitchCamButton.Click -= SwitchCamButtonOnClick;
                    MuteVideoButton.Click -= MuteVideoButtonOnClick;
                    EndCallButton.Click -= EndCallButtonOnClick;
                    MuteAudioButton.Click -= MuteAudioButtonOnClick;
                    PictureInToPictureButton.Click += PictureInToPictureButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void PictureInToPictureButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    EnterPictureInPictureMode(PictureInPictureParams);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MuteVideoButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MuteVideoButton.Selected)
                {
                    MuteVideoButton.Selected = false;
                    MuteVideoButton.SetImageResource(Resource.Drawable.ic_camera_video_open);
                }
                else
                {
                    MuteVideoButton.Selected = true;
                    MuteVideoButton.SetImageResource(Resource.Drawable.ic_camera_video_mute);
                }

                AgoraEngine?.MuteLocalVideoStream(MuteVideoButton.Selected);

                IsVideoEnabled = !MuteVideoButton.Selected;
                ThumbnailVideo.Visibility = IsVideoEnabled ? ViewStates.Visible : ViewStates.Gone;
                LocalVideoView.Visibility = IsVideoEnabled ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchCamButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                AgoraEngine?.SwitchCamera();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MuteAudioButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MuteAudioButton.Selected)
                {
                    MuteAudioButton.Selected = false;
                    MuteAudioButton.SetImageResource(Resource.Drawable.ic_camera_mic_open);
                }
                else
                {
                    MuteAudioButton.Selected = true;
                    MuteAudioButton.SetImageResource(Resource.Drawable.ic_camera_mic_mute);
                }

                AgoraEngine?.MuteLocalAudioStream(MuteAudioButton.Selected);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EndCallButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                FinishCall();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region PictureInPicture

        public override void OnPictureInPictureModeChanged(bool isInPictureInPictureMode, Configuration newConfig)
        {
            try
            {
                if (isInPictureInPictureMode)
                {
                    EndCallButton.Visibility = ViewStates.Gone;
                    MuteAudioButton.Visibility = ViewStates.Gone;
                    MuteVideoButton.Visibility = ViewStates.Gone;
                    UserNameTextView.Visibility = ViewStates.Gone;
                    NoteTextView.Visibility = ViewStates.Gone;
                    PictureInToPictureButton.Visibility = ViewStates.Gone;
                    ThumbnailVideo.Visibility = ViewStates.Gone;
                }
                else
                {
                    EndCallButton.Visibility = ViewStates.Visible;
                    MuteAudioButton.Visibility = ViewStates.Visible;
                    UserNameTextView.Visibility = ViewStates.Visible;
                    NoteTextView.Visibility = ViewStates.Visible;
                    MuteVideoButton.Visibility = ViewStates.Visible;
                    PictureInToPictureButton.Visibility = ViewStates.Visible;
                    ThumbnailVideo.Visibility = ViewStates.Visible;
                }

                base.OnPictureInPictureModeChanged(isInPictureInPictureMode, newConfig);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnUserLeaveHint()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    EnterPictureInPictureMode(PictureInPictureParams);
                }

                base.OnUserLeaveHint();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Sensor System

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            try
            {
                // Do something here if sensor accuracy changes.
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnSensorChanged(SensorEvent e)
        {
            try
            {
                if (e.Sensor.Type == SensorType.Proximity)
                {
                    if (e.Values[0] >= -SensorSensitivity && e.Values[0] <= SensorSensitivity)
                    {
                        //near 
                        GlobalContext?.SetOffWakeLock();
                    }
                    else
                    {
                        //far 
                        GlobalContext?.SetOnWakeLock();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Agora  

        private async void InitAgoraCall()
        {
            try
            {
                bool granted = ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) == Permission.Granted && ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.RecordAudio) == Permission.Granted;
                CheckVideoCallPermissions(granted);

                CallType = Intent?.GetStringExtra("type") ?? ""; // Agora_audio_call_recieve , Agora_audio_calling_start

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("callUserObject")))
                    CallUserObject = JsonConvert.DeserializeObject<CallUserObject>(Intent?.GetStringExtra("callUserObject") ?? "");

                InitializeAgoraEngine();

                switch (CallType)
                {
                    case "Agora_video_call_recieve":
                        {
                            if (!string.IsNullOrEmpty(CallUserObject.Data.AccessToken))
                            {
                                if (!string.IsNullOrEmpty(CallUserObject.UserId))
                                    Load_userWhenCall();

                                Token = CallUserObject.Data.AccessToken;

                                NoteTextView.Text = GetText(Resource.String.Lbl_Waiting_for_answer);

                                var (apiStatus, respond) = await RequestsAsync.Call.AnswerCallAgoraAsync(CallUserObject.Data.Id);
                                if (apiStatus == 200)
                                {
                                    JoinChannel(Token, CallUserObject.Data.RoomName);

                                    var ckd = GlobalContext?.LastCallsTab?.MAdapter?.MCallUser?.FirstOrDefault(a => a.Id == CallUserObject.Data.Id); // id >> Call_Id
                                    if (ckd == null)
                                    {
                                        Classes.CallUser cv = new Classes.CallUser
                                        {
                                            Id = CallUserObject.Data.Id,
                                            UserId = CallUserObject.UserId,
                                            Avatar = CallUserObject.Avatar,
                                            Name = CallUserObject.Name,
                                            FromId = CallUserObject.Data.FromId,
                                            Active = CallUserObject.Data.Active,
                                            Time = "Answered call",
                                            Status = CallUserObject.Data.Status,
                                            RoomName = CallUserObject.Data.RoomName,
                                            Type = CallType,
                                            TypeIcon = "Accept",
                                            TypeColor = "#008000"
                                        };

                                        GlobalContext?.LastCallsTab?.MAdapter?.Insert(cv);

                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Insert_CallUser(cv);

                                    }
                                }
                                //else Methods.DisplayReportResult(this, respond);
                            }

                            break;
                        }
                    case "Agora_video_calling_start":
                        NoteTextView.Text = GetText(Resource.String.Lbl_Calling);

                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("outgoin_call.mp3", "Looping");

                        //string channelName = "room";
                        //int uid = 0; 
                        //int expirationTimeInSeconds = 3600; 

                        //RtcTokenBuilder token = new RtcTokenBuilder();
                        //int timestamp = (int)(Methods.Time.CurrentTimeMillis() / 1000 + expirationTimeInSeconds);

                        //Token = token.BuildTokenWithUid(ListUtils.SettingsSiteList?.AgoraChatAppId, ListUtils.SettingsSiteList?.AgoraChatAppCertificate, channelName, uid, RtcTokenBuilder.Role.RolePublisher, timestamp);

                        if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_Create_callEvent(CallUserObject.UserId);

                        Task.Factory.StartNew(() => StartApiService());
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitializeAgoraEngine()
        {
            try
            {
                AgoraHandler = new AgoraRtcHandler(this);
                AgoraEngine = RtcEngine.Create(this, ListUtils.SettingsSiteList?.AgoraChatAppId, AgoraHandler);
                AgoraEngine?.SetChannelProfile(Constants.ChannelProfileCommunication);
                AgoraEngine?.EnableAudio();
                AgoraEngine?.EnableVideo();

                SetupLocalVideo();
            }
            catch (Exception e)
            {
                //Colud not create RtcEngine
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetupLocalVideo()
        {
            try
            {
                FrameLayout container = (FrameLayout)FindViewById(Resource.Id.local_video_view_container);
                LocalVideoView = RtcEngine.CreateRendererView(BaseContext);
                LocalVideoView.SetZOrderMediaOverlay(true);
                container.AddView(LocalVideoView);
                //AgoraEngine?.SetupLocalVideo(new VideoCanvas(LocalVideoView, VideoCanvas.RenderModeAdaptive, 0)); >> Old
                AgoraEngine?.SetupLocalVideo(new VideoCanvas(LocalVideoView, VideoCanvas.RenderModeHidden, 0));

                AgoraEngine?.StartPreview();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void JoinChannel(string accessToken, string channelName)
        {
            try
            {
                AgoraEngine?.JoinChannel(accessToken, channelName, string.Empty, 0);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Load_userWhenCall()
        {
            try
            {
                UserNameTextView.Text = CallUserObject.Name;

                //profile_picture
                GlideImageLoader.LoadImage(this, CallUserObject.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { CreateNewCall });
        }

        private async Task CreateNewCall()
        {

            if (!Methods.CheckConnectivity())
                return;

            Load_userWhenCall();
            var (apiStatus, respond) = await RequestsAsync.Call.CreateNewCallAgoraAsync(CallUserObject.UserId, Token, TypeCall.Video);
            if (apiStatus == 200)
            {
                if (respond is CreateNewCallAgoraObject result)
                {
                    CallUserObject.Data.Id = result.Id;
                    Token = CallUserObject.Data.AccessToken = result.Token;
                    CallUserObject.Data.RoomName = result.RoomName;

                    TimerRequestWaiter = new Timer { Interval = 5000 };
                    TimerRequestWaiter.Elapsed += TimerCallRequestAnswer_Waiter_Elapsed;
                    TimerRequestWaiter.Start();
                }
            }
            else
            {
                FinishCall();
                //Methods.DisplayReportResult(this, respond);
            }
        }

        private async void TimerCallRequestAnswer_Waiter_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Call.CheckForAnswerAgoraAsync(CallUserObject.Data.Id, TypeCall.Video);
                if (apiStatus == 200)
                {
                    if (respond is CheckForAnswerAgoraObject agoraObject)
                    {
                        if (string.IsNullOrEmpty(agoraObject.CallStatus))
                            return;

                        RunOnUiThread(Methods.AudioRecorderAndPlayer.StopAudioFromAsset);

                        switch (agoraObject.CallStatus)
                        {
                            case "answered":
                                {
                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            JoinChannel(Token, CallUserObject.Data.RoomName);

                                            TimerRequestWaiter.Enabled = false;
                                            TimerRequestWaiter.Stop();
                                            TimerRequestWaiter.Close();

                                            var ckd = GlobalContext?.LastCallsTab?.MAdapter?.MCallUser?.FirstOrDefault(a => a.Id == CallUserObject.Data.Id); // id >> Call_Id
                                            if (ckd == null)
                                            {
                                                Classes.CallUser cv = new Classes.CallUser
                                                {
                                                    Id = CallUserObject.Data.Id,
                                                    UserId = CallUserObject.UserId,
                                                    Avatar = CallUserObject.Avatar,
                                                    Name = CallUserObject.Name,
                                                    FromId = CallUserObject.Data.FromId,
                                                    Active = CallUserObject.Data.Active,
                                                    Time = "Answered call",
                                                    Status = CallUserObject.Data.Status,
                                                    RoomName = CallUserObject.Data.RoomName,
                                                    Type = CallType,
                                                    TypeIcon = "Accept",
                                                    TypeColor = "#008000"
                                                };

                                                GlobalContext?.LastCallsTab?.MAdapter?.Insert(cv);

                                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                                dbDatabase.Insert_CallUser(cv);
                                            }
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });
                                    break;
                                }
                            case "calling" when CountSecondsOfOutGoingCall < 80:
                                CountSecondsOfOutGoingCall += 10;
                                break;
                            case "calling":
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        //Call Is inactive 
                                        TimerRequestWaiter.Enabled = false;
                                        TimerRequestWaiter.Stop();
                                        TimerRequestWaiter.Close();

                                        var ckd = GlobalContext?.LastCallsTab?.MAdapter?.MCallUser?.FirstOrDefault(a => a.Id == CallUserObject.Data.Id); // id >> Call_Id
                                        if (ckd == null)
                                        {
                                            Classes.CallUser cv = new Classes.CallUser
                                            {
                                                Id = CallUserObject.Data.Id,
                                                UserId = CallUserObject.UserId,
                                                Avatar = CallUserObject.Avatar,
                                                Name = CallUserObject.Name,
                                                FromId = CallUserObject.Data.FromId,
                                                Active = CallUserObject.Data.Active,
                                                Time = "Missed call",
                                                Status = CallUserObject.Data.Status,
                                                RoomName = CallUserObject.Data.RoomName,
                                                Type = CallType,
                                                TypeIcon = "Cancel",
                                                TypeColor = "#FF0000"
                                            };

                                            GlobalContext?.LastCallsTab?.MAdapter?.Insert(cv);

                                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                            dbDatabase.Insert_CallUser(cv);

                                        }

                                        FinishCall();
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                                break;
                            case "declined":
                                {
                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            //Call Is inactive 
                                            TimerRequestWaiter.Enabled = false;
                                            TimerRequestWaiter.Stop();
                                            TimerRequestWaiter.Close();

                                            var ckd = GlobalContext?.LastCallsTab?.MAdapter?.MCallUser?.FirstOrDefault(a => a.Id == CallUserObject.Data.Id); // id >> Call_Id
                                            if (ckd == null)
                                            {
                                                Classes.CallUser cv = new Classes.CallUser
                                                {
                                                    Id = CallUserObject.Data.Id,
                                                    UserId = CallUserObject.UserId,
                                                    Avatar = CallUserObject.Avatar,
                                                    Name = CallUserObject.Name,
                                                    FromId = CallUserObject.Data.FromId,
                                                    Active = CallUserObject.Data.Active,
                                                    Time = "Missed call",
                                                    Status = CallUserObject.Data.Status,
                                                    RoomName = CallUserObject.Data.RoomName,
                                                    Type = CallType,
                                                    TypeIcon = "Cancel",
                                                    TypeColor = "#FF0000"
                                                };

                                                GlobalContext?.LastCallsTab?.MAdapter?.Insert(cv);

                                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                                dbDatabase.Insert_CallUser(cv);
                                            }

                                            FinishCall();
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });

                                    break;
                                }
                            case "no_answer":
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        //Call Is inactive 
                                        TimerRequestWaiter.Enabled = false;
                                        TimerRequestWaiter.Stop();
                                        TimerRequestWaiter.Close();

                                        var ckd = GlobalContext?.LastCallsTab?.MAdapter?.MCallUser?.FirstOrDefault(a =>
                                            a.Id == CallUserObject.Data.Id); // id >> Call_Id
                                        if (ckd == null)
                                        {
                                            Classes.CallUser cv = new Classes.CallUser
                                            {
                                                Id = CallUserObject.Data.Id,
                                                UserId = CallUserObject.UserId,
                                                Avatar = CallUserObject.Avatar,
                                                Name = CallUserObject.Name,
                                                FromId = CallUserObject.Data.FromId,
                                                Active = CallUserObject.Data.Active,
                                                Time = "Declined call",
                                                Status = CallUserObject.Data.Status,
                                                RoomName = CallUserObject.Data.RoomName,
                                                Type = CallType,
                                                TypeIcon = "Declined",
                                                TypeColor = "#FF8000"
                                            };

                                            GlobalContext?.LastCallsTab?.MAdapter?.Insert(cv);

                                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                            dbDatabase.Insert_CallUser(cv);
                                        }

                                        FinishCall();
                                        //Methods.DisplayReportResult(this, respond);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                                break;
                        }

                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions

        private void RequestCameraAndMicrophonePermissions()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera) || ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.RecordAudio))
                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Need_Camera), ToastLength.Long);
            else
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Camera, Manifest.Permission.RecordAudio, Manifest.Permission.ModifyAudioSettings }, 1);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == 1)
                CheckVideoCallPermissions(grantResults.Any(p => p == Permission.Denied));
        }

        private void CheckVideoCallPermissions(bool granted)
        {
            if (!granted)
                RequestCameraAndMicrophonePermissions();
        }


        #endregion

        #region Agora Rtc Handler

        public void OnConnectionLost()
        {
            RunOnUiThread(() =>
            {
                try
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Lost_Connection), ToastLength.Short);
                    FinishCall();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    FinishCall();
                }
            });
        }

        public void OnUserOffline(int uid, int reason)
        {
            RunOnUiThread(async () =>
            {
                try
                {
                    Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                    //Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Error.mp3");
                    NoteTextView.Text = GetText(Resource.String.Lbl_Lost_his_connection);
                    await Task.Delay(2000);
                    FinishCall();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    FinishCall();
                }
            });
        }

        public void OnNetworkQuality(int uid, int txQuality, int rxQuality)
        {

        }

        public void OnFirstRemoteVideoDecoded(int uid, int width, int height, int elapsed)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    SetupRemoteVideo(uid);
                    Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        public void OnUserJoined(int uid, int elapsed)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    UserInfoViewContainer.Visibility = ViewStates.Gone;
                    NoteTextView.Text = "";
                    Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private void SetupRemoteVideo(int uid)
        {
            try
            {
                FrameLayout container = (FrameLayout)FindViewById(Resource.Id.remote_video_view_container);
                if (container.ChildCount >= 1)
                {
                    return;
                }

                SurfaceView surfaceView = RtcEngine.CreateRendererView(BaseContext);
                container.AddView(surfaceView);

                //AgoraEngine?.SetupRemoteVideo(new VideoCanvas(surfaceView, VideoCanvas.RenderModeAdaptive, uid)); >> Old
                AgoraEngine?.SetupRemoteVideo(new VideoCanvas(surfaceView, VideoCanvas.RenderModeHidden, uid));
                surfaceView.Tag = uid; // for mark purpose
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFirstLocalVideoFrame(float height, float width, int p2)
        {
            try
            {
                var ratio = height / width;
                var ratioHeight = ratio * MaxLocalVideoDimension;
                var ratioWidth = MaxLocalVideoDimension / ratio;
                var containerHeight = height > width ? MaxLocalVideoDimension : ratioHeight;
                var containerWidth = height > width ? ratioWidth : MaxLocalVideoDimension;
                RunOnUiThread(() =>
                {
                    var parameters = ThumbnailVideo.LayoutParameters;
                    parameters.Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, containerHeight, Resources.DisplayMetrics);
                    parameters.Width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, containerWidth, Resources.DisplayMetrics);
                    ThumbnailVideo.LayoutParameters = parameters;
                    ThumbnailVideo.Visibility = IsVideoEnabled ? ViewStates.Visible : ViewStates.Invisible;
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnJoinChannelSuccess(string channel, int uid, int elapsed)
        {

        }

        public void OnUserMuteVideo(int uid, bool muted)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    UserInfoViewContainer.Visibility = ViewStates.Visible;
                    OnRemoteUserVideoMuted(uid, muted);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            });
        }

        #endregion

        public override void OnBackPressed()
        {
            FinishCall();
        }

        private void OnRemoteUserVideoMuted(int uid, bool muted)
        {

            FrameLayout container = (FrameLayout)FindViewById(Resource.Id.remote_video_view_container);
            SurfaceView surfaceView = (SurfaceView)container.GetChildAt(0);
            var tag = surfaceView.Tag;
            if (tag != null && (int)tag == uid)
            {
                NoteTextView.Text = GetText(Resource.String.Lbl_Muted_his_video);
                surfaceView.Visibility = muted ? ViewStates.Gone : ViewStates.Visible;
                if (muted)
                {
                    UserInfoViewContainer.Visibility = ViewStates.Visible;
                    NoteTextView.Text = GetText(Resource.String.Lbl_Muted_his_video);
                }
                else
                {
                    UserInfoViewContainer.Visibility = ViewStates.Gone;
                    NoteTextView.Text = "";
                }
            }
        }

        private void FinishCall()
        {
            try
            {
                //Close Api Starts here >>

                if (!Methods.CheckConnectivity())
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.CloseCallAgoraAsync(CallUserObject.Data.Id) });

                if (AgoraEngine != null)
                {
                    AgoraEngine?.StopPreview();
                    AgoraEngine?.SetupLocalVideo(null);
                    AgoraEngine?.LeaveChannel();
                    AgoraEngine?.Dispose();
                    AgoraEngine = null!;
                }

                TabbedMainActivity.RunCall = false;
                Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                TabbedMainActivity.RunCall = false;
                Finish();
            }
        }
    }
}