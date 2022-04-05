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
using Newtonsoft.Json;
using TwilioVideo;
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

namespace WoWonder.Frameworks.Twilio
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ResizeableActivity = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class TwilioAudioCallActivity : AppCompatActivity, ISensorEventListener, TwilioVideoHelper.IListener
    {
        #region Variables Basic

        private TwilioVideoHelper TwilioVideo { get; set; }
        private string CallType = "0";
        private CallUserObject CallUserObject;
        private CircleButton EndCallButton, SpeakerAudioButton, MuteAudioButton;
        private ImageView UserImageView;
        private TextView UserNameTextView, DurationTextView;
        private Timer TimerRequestWaiter = new Timer();
        private LocalVideoTrack LocalVideoTrack;
        private VideoTrack UserVideoTrack;
        private bool DataUpdated;
        private int CountSecondsOfOutGoingCall;
        private string LocalVideoTrackId, RemoteVideoTrackId;
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
                SetContentView(Resource.Layout.TwilioAudioCallActivityLayout);

                SensorManager = (SensorManager)GetSystemService(SensorService);
                Proximity = SensorManager?.GetDefaultSensor(SensorType.Proximity);

                GlobalContext = TabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitTwilioCall();
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
                UpdateState();
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
                UpdateState();
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
                DataUpdated = false;
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
                TwilioVideo = TwilioVideoHelper.GetOrCreate(this, TypeCall.Audio);
                UpdateState();
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
                    Finish();
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

                SpeakerAudioButton.Selected = false;
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

                TwilioVideo.Mute(MuteAudioButton.Selected);
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
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Audio) });
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

                TwilioVideo.Speaker(SpeakerAudioButton.Selected);
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

        #region Twilio  

        private async void InitTwilioCall()
        {
            try
            {
                bool granted =
                    ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) ==
                    Permission.Granted &&
                    ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.RecordAudio) ==
                    Permission.Granted;

                CheckVideoCallPermissions(granted);

                CallType = Intent?.GetStringExtra("type") ?? ""; // Twilio_video_call , Twilio_audio_call,Agora_video_call_recieve,Agora_audio_call_recieve

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("callUserObject")))
                    CallUserObject = JsonConvert.DeserializeObject<CallUserObject>(Intent?.GetStringExtra("callUserObject") ?? "");

                switch (CallType)
                {
                    case "Twilio_audio_call":
                        {
                            if (!string.IsNullOrEmpty(CallUserObject.Data.AccessToken))
                            {
                                if (!string.IsNullOrEmpty(CallUserObject.UserId))
                                    Load_userWhenCall();

                                TwilioVideo = TwilioVideoHelper.GetOrCreate(this, TypeCall.Audio);
                                UpdateState();
                                DurationTextView.Text = GetText(Resource.String.Lbl_Waiting_for_answer);

                                var (apiStatus, respond) = await RequestsAsync.Call.AnswerCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Audio);
                                if (apiStatus == 200)
                                {
                                    ConnectToRoom();

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
                    case "Twilio_audio_calling_start":
                        DurationTextView.Text = GetText(Resource.String.Lbl_Calling);
                        TwilioVideo = TwilioVideoHelper.GetOrCreate(this, TypeCall.Audio);

                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("outgoin_call.mp3", "left");

                        if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            UserDetails.Socket?.EmitAsync_Create_callEvent(CallUserObject.UserId);

                        Task.Factory.StartNew(() => StartApiService());

                        UpdateState();
                        break;
                }
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
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadProfileFromUserId });
        }

        private async Task LoadProfileFromUserId()
        {
            Load_userWhenCall();
            var (apiStatus, respond) = await RequestsAsync.Call.CreateNewCallTwilioAsync(CallUserObject.UserId, TypeCall.Audio);
            if (apiStatus == 200)
            {
                if (respond is CallUserObject.DataCallUser result)
                {
                    CallUserObject.Data.Id = result.Id.ToString();
                    CallUserObject.Data.AccessToken = result.AccessToken;
                    CallUserObject.Data.AccessToken2 = result.AccessToken2;
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
                var (apiStatus, respond) = await RequestsAsync.Call.CheckForAnswerTwilioAsync(CallUserObject.Data.Id, TypeCall.Audio);
                switch (apiStatus)
                {
                    case 200:
                        {
                            Methods.AudioRecorderAndPlayer.StopAudioFromAsset();

                            if (!string.IsNullOrEmpty(CallUserObject.Data.AccessToken))
                            {
                                RunOnUiThread(async () =>
                                {
                                    try
                                    {
                                        TimerRequestWaiter.Enabled = false;
                                        TimerRequestWaiter.Stop();
                                        TimerRequestWaiter.Close();

                                        await Task.Delay(1000);

                                        TwilioVideo?.UpdateToken(CallUserObject.Data.AccessToken2);
                                        TwilioVideo?.JoinRoom(this, CallUserObject.Data.RoomName);

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
                            }

                            break;
                        }
                    case 300 when CountSecondsOfOutGoingCall < 70:
                        CountSecondsOfOutGoingCall += 10;
                        break;
                    case 300:
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
                    default:
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions

        private void RequestCameraAndMicrophonePermissions()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera) ||
                ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.RecordAudio))
                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Need_Camera), ToastLength.Long);
            else
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Camera, Manifest.Permission.RecordAudio, Manifest.Permission.ModifyAudioSettings }, 1);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
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

        #region TwilioVideoHelper.IListener

        public void SetLocalVideoTrack(LocalVideoTrack track)
        {
            try
            {
                if (LocalVideoTrack == null)
                {
                    LocalVideoTrack = track;
                    var trackId = track?.Name;
                    if (LocalVideoTrackId == trackId)
                    {
                        LocalVideoTrack.Enable(false);
                    }
                    else
                    {
                        LocalVideoTrackId = trackId;
                        LocalVideoTrack.Enable(false);
                    }
                }
                else
                {
                    if (LocalVideoTrack.IsEnabled)
                        LocalVideoTrack.Enable(false);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetRemoteVideoTrack(VideoTrack track)
        {
            try
            {
                var trackId = track?.Name;

                if (RemoteVideoTrackId == trackId)
                    return;

                RemoteVideoTrackId = trackId;
                if (UserVideoTrack == null)
                {
                    UserVideoTrack = track;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveLocalVideoTrack(LocalVideoTrack track)
        {
            try
            {
                SetLocalVideoTrack(null);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveRemoteVideoTrack(VideoTrack track)
        {

        }

        public void OnRoomConnected(string roomId)
        {

        }

        public void OnRoomDisconnected(TwilioVideoHelper.StopReason reason)
        {
            try
            {
                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Room_Disconnected), ToastLength.Short);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnParticipantConnected(string participantId)
        {
            try
            {
                DurationTextView.Text = GetText(Resource.String.Lbl_Connected);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnParticipantDisconnected(string participantId)
        {
            RunOnUiThread(async () =>
            {
                try
                {
                    DurationTextView.Text = GetText(Resource.String.Lbl_User_Lost_Connection);
                    await Task.Delay(2000);
                    FinishCall();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        public void SetCallTime(int seconds)
        {
            try
            {
                DurationTextView.Text = seconds.ToString();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void ConnectToRoom()
        {
            TwilioVideo?.UpdateToken(CallUserObject.Data.AccessToken);
            TwilioVideo?.JoinRoom(ApplicationContext, CallUserObject.Data.RoomName);
        }

        public override bool OnSupportNavigateUp()
        {
            TryCancelCall();
            return true;
        }

        public override void OnBackPressed()
        {
            FinishCall();
        }

        void UpdateState()
        {
            try
            {
                if (DataUpdated)
                    return;

                DataUpdated = true;

                TwilioVideo?.Bind(this);

                UpdatingState();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdatingState()
        {
        }

        private void TryCancelCall()
        {
            CloseScreen();
        }

        private void CloseScreen()
        {
            Finish();
        }

        private void FinishCall()
        {
            try
            {
                //Close Api Starts here >> 
                if (!Methods.CheckConnectivity())
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Call.CloseCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Audio) });

                if (TwilioVideo.ClientIsReady)
                {
                    TwilioVideo.Unbind(this);
                    TwilioVideo.FinishCall();
                }

                Methods.AudioRecorderAndPlayer.StopAudioFromAsset();
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}