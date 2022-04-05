using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AT.Markushi.UI;
using DT.Xamarin.Agora;
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
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait)]
    public class AgoraAudioCallActivity : AppCompatActivity, ISensorEventListener
    {
        #region Variables Basic

        private string CallType = "0", Token = "";
        private CallUserObject CallUserObject;

        private RtcEngine AgoraEngine;
        private AgoraRtcAudioCallHandler AgoraHandler;

        private CircleButton EndCallButton, SpeakerAudioButton, MuteAudioButton;
        private ImageView UserImageView;
        private TextView UserNameTextView, DurationTextView;

        private int CountSecondsOfOutGoingCall;
        private Timer TimerRequestWaiter, TimerSound;

        private TabbedMainActivity GlobalContext;

        private SensorManager SensorManager;
        private Sensor Proximity;
        private readonly int SensorSensitivity = 4;

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
                SetContentView(Resource.Layout.AgoraAudioCallActivityLayout);

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
                SpeakerAudioButton = FindViewById<CircleButton>(Resource.Id.speaker_audio_button);
                EndCallButton = FindViewById<CircleButton>(Resource.Id.end_audio_call_button);
                MuteAudioButton = FindViewById<CircleButton>(Resource.Id.mute_audio_call_button);

                UserImageView = FindViewById<ImageView>(Resource.Id.audiouserImageView);
                UserNameTextView = FindViewById<TextView>(Resource.Id.audiouserNameTextView);
                DurationTextView = FindViewById<TextView>(Resource.Id.audiodurationTextView);


                SpeakerAudioButton.SetImageResource(Resource.Drawable.ic_speaker_close);
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
                    SpeakerAudioButton.Click += SpeakerAudioButtonOnClick;
                    EndCallButton.Click += EndCallButtonOnClick;
                    MuteAudioButton.Click += MuteAudioButtonOnClick;
                }
                else
                {
                    SpeakerAudioButton.Click -= SpeakerAudioButtonOnClick;
                    EndCallButton.Click -= EndCallButtonOnClick;
                    MuteAudioButton.Click -= MuteAudioButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

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

        private void SpeakerAudioButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                //Speaker
                if (SpeakerAudioButton.Selected)
                {
                    SpeakerAudioButton.Selected = false;
                    SpeakerAudioButton.SetImageResource(Resource.Drawable.ic_speaker_close);
                }
                else
                {
                    SpeakerAudioButton.Selected = true;
                    SpeakerAudioButton.SetImageResource(Resource.Drawable.ic_speaker_up);
                }

                AgoraEngine?.SetEnableSpeakerphone(SpeakerAudioButton.Selected);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                    case "Agora_audio_call_recieve":
                        {
                            if (!string.IsNullOrEmpty(CallUserObject.Data.AccessToken))
                            {
                                if (!string.IsNullOrEmpty(CallUserObject.UserId))
                                    Load_userWhenCall();

                                Token = CallUserObject.Data.AccessToken;

                                DurationTextView.Text = GetText(Resource.String.Lbl_Waiting_for_answer);

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
                    case "Agora_audio_calling_start":
                        DurationTextView.Text = GetText(Resource.String.Lbl_Calling);

                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("outgoin_call.mp3", "left");

                        //string channelName = "room";
                        //int uid = 0; 
                        //int expirationTimeInSeconds = 3600; 

                        //RtcTokenBuilder token = new RtcTokenBuilder();
                        //int timestamp = (int)(Methods.Time.CurrentTimeMillis() / 1000 + expirationTimeInSeconds);

                        //Token = token.BuildTokenWithUid(ListUtils.SettingsSiteList?.AgoraChatAppId, ListUtils.SettingsSiteList?.AgoraChatAppCertificate, channelName, uid, RtcTokenBuilder.Role.RolePublisher, timestamp);

                        if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_Create_callEvent(CallUserObject.UserId);

                        StartApiService();
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
                AgoraHandler = new AgoraRtcAudioCallHandler(this);
                AgoraEngine = RtcEngine.Create(this, ListUtils.SettingsSiteList?.AgoraChatAppId, AgoraHandler);
                AgoraEngine?.SetChannelProfile(Constants.ChannelProfileCommunication);
                AgoraEngine?.EnableAudio();
                AgoraEngine?.DisableVideo();
            }
            catch (Exception e)
            {
                //Colud not create RtcEngine
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
            var (apiStatus, respond) = await RequestsAsync.Call.CreateNewCallAgoraAsync(CallUserObject.UserId, Token, TypeCall.Audio);
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
                var (apiStatus, respond) = await RequestsAsync.Call.CheckForAnswerAgoraAsync(CallUserObject.Data.Id, TypeCall.Audio);
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
                    DurationTextView.Text = GetText(Resource.String.Lbl_Lost_his_connection);
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

        public void OnUserJoined(int uid, int elapsed)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    DurationTextView.Text = GetText(Resource.String.Lbl_Please_wait);
                    Methods.AudioRecorderAndPlayer.StopAudioFromAsset();

                    TimerSound = new Timer();
                    TimerSound.Interval = 1000;
                    TimerSound.Elapsed += TimerSoundOnElapsed;
                    TimerSound.Start();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private string TimeCall;
        private bool IsMuted;
        private void TimerSoundOnElapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    if (!IsMuted)
                    {
                        //Write your own duration function here 
                        TimeCall = TimeSpan.FromSeconds(e.SignalTime.Second).ToString(@"mm\:ss");
                        DurationTextView.Text = TimeCall;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            });
        }

        public void OnJoinChannelSuccess(string channel, int uid, int elapsed)
        {

        }

        public void OnUserMuteAudio(int uid, bool muted)
        {
            try
            {
                IsMuted = muted;
                if (muted)
                {
                    DurationTextView.Text = GetText(Resource.String.Lbl_Muted_his_video);
                }
                else
                {
                    DurationTextView.Text = TimeCall;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public override void OnBackPressed()
        {
            FinishCall();
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