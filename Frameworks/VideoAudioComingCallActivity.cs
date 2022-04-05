using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AT.Markushi.UI;
using Newtonsoft.Json;
using WoWonder.Activities.Tab;
using WoWonder.Frameworks.Agora;
using WoWonder.Frameworks.Twilio;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Call;
using WoWonder.SQLite;
using WoWonderClient.Classes.Call;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Frameworks
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode, ScreenOrientation = ScreenOrientation.Portrait)]
    public class VideoAudioComingCallActivity : AppCompatActivity, CallAnswerDeclineButton.IAnswerDeclineListener, MaterialDialog.ISingleButtonCallback, MaterialDialog.IListCallback, MaterialDialog.IInputCallback
    {
        private string CallType = "0";

        private CallUserObject CallUserObject;

        private ImageView UserImageView;
        private TextView UserNameTextView, TypeCallTextView;
        public static VideoAudioComingCallActivity CallActivity;

        private CircleButton MessageCallButton;

        public static bool IsActive = false;


        private Ringtone Ringtone;
        private Vibrator Vibrator;

        private CallAnswerDeclineButton AnswerDeclineButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetContentView(Resource.Layout.TwilioCommingVideoCallLayout);
                Window.AddFlags(WindowManagerFlags.KeepScreenOn);

                CallActivity = this;

                CallType = Intent?.GetStringExtra("type") ?? "";

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("callUserObject")))
                    CallUserObject = JsonConvert.DeserializeObject<CallUserObject>(Intent?.GetStringExtra("callUserObject") ?? "");

                UserNameTextView = FindViewById<TextView>(Resource.Id.UsernameTextView);
                TypeCallTextView = FindViewById<TextView>(Resource.Id.TypecallTextView);
                UserImageView = FindViewById<ImageView>(Resource.Id.UserImageView);

                MessageCallButton = FindViewById<CircleButton>(Resource.Id.message_call_button);

                AnswerDeclineButton = FindViewById<CallAnswerDeclineButton>(Resource.Id.answer_decline_button);

                AnswerDeclineButton.SetAnswerDeclineListener(this);
                AnswerDeclineButton.Visibility = ViewStates.Visible;
                AnswerDeclineButton.StartRingingAnimation();

                MessageCallButton.Click += MessageCallButton_Click;

                if (!string.IsNullOrEmpty(CallUserObject.Name))
                    UserNameTextView.Text = CallUserObject.Name;

                if (!string.IsNullOrEmpty(CallUserObject.Avatar))
                    GlideImageLoader.LoadImage(this, CallUserObject.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                if (CallType == "Twilio_video_call" || CallType == "Agora_video_call_recieve")
                    TypeCallTextView.Text = GetText(Resource.String.Lbl_Video_call);
                else
                    TypeCallTextView.Text = GetText(Resource.String.Lbl_Voice_call);

                PlayAudioFromAsset("mystic_call.mp3");

                if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                {
                    VibratorManager vibratorManager = (VibratorManager)GetSystemService(VibratorManagerService);
                    Vibrator = vibratorManager?.DefaultVibrator;
                }
                else
                {
                    Vibrator = (Vibrator)GetSystemService("vibrator");
                }

                var vibrate = new long[]
                {
                    1000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000,
                    2000, 1000, 2000, 1000, 2000, 1000, 2000, 1000, 2000
                };

                // Vibrate for 500 milliseconds
                Vibrator?.Vibrate(VibrationEffect.CreateWaveform(vibrate, 3));
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
                IsActive = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                IsActive = false;
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
                TabbedMainActivity.GetInstance()?.OffWakeLock();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MessageCallButton_Click(object sender, EventArgs e)
        {
            try
            {

                if (Methods.CheckConnectivity())
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall1));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall2));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall3));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall4));
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MessageCall5));

                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RejectCallButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (CallType)
                {
                    case "Twilio_video_call":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Video) });
                        break;
                    case "Twilio_audio_call":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Audio) });
                        break;
                    case "Agora_video_call_recieve":
                    case "Agora_audio_call_recieve":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallAgoraAsync(CallUserObject.Data.Id) });
                        break;
                }

                if (!string.IsNullOrEmpty(CallUserObject.Data.Id))
                {
                    var ckd = TabbedMainActivity.GetInstance()?.LastCallsTab?.MAdapter?.MCallUser?.FirstOrDefault(a => a.Id == CallUserObject.Data.Id); // id >> Call_Id
                    if (ckd == null)
                    {
                        Classes.CallUser cv = new Classes.CallUser
                        {
                            Id = CallUserObject.Data.Id,
                            UserId = CallUserObject.UserId,
                            Avatar = CallUserObject.Avatar,
                            Name = CallUserObject.Name,
                            AccessToken = CallUserObject.Data.AccessToken,
                            AccessToken2 = CallUserObject.Data.AccessToken2,
                            FromId = CallUserObject.Data.FromId,
                            Active = CallUserObject.Data.Active,
                            Time = "Missed call",
                            Status = CallUserObject.Data.Status,
                            RoomName = CallUserObject.Data.RoomName,
                            Type = CallType,
                            TypeIcon = "Cancel",
                            TypeColor = "#FF0000"
                        };

                        TabbedMainActivity.GetInstance()?.LastCallsTab?.MAdapter?.Insert(cv);

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_CallUser(cv);

                    }
                }

                TabbedMainActivity.RunCall = false;
                FinishVideoAudio();
            }
            catch (Exception exception)
            {
                TabbedMainActivity.RunCall = false;
                FinishVideoAudio();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AcceptCallButton_Click(object sender, EventArgs e)
        {
            try
            {
                switch (CallType)
                {
                    case "Twilio_video_call":
                        {
                            Intent intent = new Intent(this, typeof(TwilioVideoCallActivity));
                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                    case "Twilio_audio_call":
                        {
                            Intent intent = new Intent(this, typeof(TwilioAudioCallActivity));
                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                    case "Agora_audio_call_recieve":
                        {
                            Intent intent = new Intent(this, typeof(AgoraAudioCallActivity));
                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                    case "Agora_video_call_recieve":
                        {
                            Intent intent = new Intent(this, typeof(AgoraVideoCallActivity));

                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                }

                FinishVideoAudio();
            }
            catch (Exception exception)
            {
                FinishVideoAudio();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;

                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (text == GetString(Resource.String.Lbl_MessageCall5))
                    {
                        var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialogBuilder.Input(Resource.String.Lbl_Write_your_message, 0, false, this);
                        dialogBuilder.InputType(InputTypes.TextFlagImeMultiLine);
                        dialogBuilder.PositiveText(GetText(Resource.String.Btn_Send)).OnPositive(this);
                        dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                        dialogBuilder.Build().Show();
                        dialogBuilder.AlwaysCallSingleChoiceCallback();
                    }
                    else
                    {
                        SendMess(text);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnInput(MaterialDialog p0, string p1)
        {
            try
            {
                if (p1.Length > 0)
                {
                    var text = p1;
                    SendMess(text);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private async void SendMess(string text)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var time2 = unixTimestamp.ToString();

                    //Here on This function will send Selected audio file to the user 
                    var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(CallUserObject.UserId, time2, text);
                    if (apiStatus == 200)
                    {
                        if (respond is SendMessageObject result)
                        {
                            Console.WriteLine(result.MessageData);
                            if (!string.IsNullOrEmpty(CallUserObject.Data.Id))
                            {
                                var ckd = TabbedMainActivity.GetInstance()?.LastCallsTab?.MAdapter?.MCallUser?.FirstOrDefault(a => a.Id == CallUserObject.Data.Id); // id >> Call_Id
                                if (ckd == null)
                                {
                                    Classes.CallUser cv = new Classes.CallUser
                                    {
                                        Id = CallUserObject.Data.Id,
                                        UserId = CallUserObject.UserId,
                                        Avatar = CallUserObject.Avatar,
                                        Name = CallUserObject.Name,
                                        AccessToken = CallUserObject.Data.AccessToken,
                                        AccessToken2 = CallUserObject.Data.AccessToken2,
                                        FromId = CallUserObject.Data.FromId,
                                        Active = CallUserObject.Data.Active,
                                        Time = "Missed call",
                                        Status = CallUserObject.Data.Status,
                                        RoomName = CallUserObject.Data.RoomName,
                                        Type = CallType,
                                        TypeIcon = "Cancel",
                                        TypeColor = "#FF0000"
                                    };

                                    TabbedMainActivity.GetInstance()?.LastCallsTab.MAdapter?.Insert(cv);

                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    dbDatabase.Insert_CallUser(cv);

                                }
                            }

                            switch (CallType)
                            {
                                case "Twilio_video_call":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Video) });
                                    break;
                                case "Twilio_audio_call":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Audio) });
                                    break;
                                case "Agora_video_call_recieve":
                                case "Agora_audio_call_recieve":
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallAgoraAsync(CallUserObject.Data.Id) });
                                    break;
                            }

                            TabbedMainActivity.RunCall = false;
                            FinishVideoAudio();
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FinishVideoAudio()
        {
            try
            {
                StopAudioFromAsset();
                Vibrator?.Cancel();

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlayAudioFromAsset(string fileName, string typeVolume = "right")
        {
            try
            {
                Ringtone = RingtoneManager.GetRingtone(this, RingtoneManager.GetDefaultUri(RingtoneType.Ringtone));
                Ringtone.Play();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopAudioFromAsset()
        {
            try
            {
                if (Ringtone != null && Ringtone.IsPlaying)
                {
                    Ringtone.Stop();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnAnswered()
        {
            try
            {
                switch (CallType)
                {
                    case "Twilio_video_call":
                        {
                            Intent intent = new Intent(this, typeof(TwilioVideoCallActivity));
                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                    case "Twilio_audio_call":
                        {
                            Intent intent = new Intent(this, typeof(TwilioAudioCallActivity));
                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                    case "Agora_audio_call_recieve":
                        {
                            Intent intent = new Intent(this, typeof(AgoraAudioCallActivity));
                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                    case "Agora_video_call_recieve":
                        {
                            Intent intent = new Intent(this, typeof(AgoraVideoCallActivity));

                            intent.SetFlags(ActivityFlags.TaskOnHome | ActivityFlags.BroughtToFront | ActivityFlags.NewTask);
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(CallUserObject));
                            intent.PutExtra("type", CallType);
                            StartActivity(intent);
                            break;
                        }
                }

                FinishVideoAudio();
            }
            catch (Exception exception)
            {
                FinishVideoAudio();
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnDeclined()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                switch (CallType)
                {
                    case "Twilio_video_call":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Video) });
                        break;
                    case "Twilio_audio_call":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallTwilioAsync(CallUserObject.Data.Id, TypeCall.Audio) });
                        break;
                    case "Agora_video_call_recieve":
                    case "Agora_audio_call_recieve":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { async () => await RequestsAsync.Call.DeclineCallAgoraAsync(CallUserObject.Data.Id) });
                        break;
                }

                if (!string.IsNullOrEmpty(CallUserObject.Data.Id))
                {
                    var ckd = TabbedMainActivity.GetInstance()?.LastCallsTab?.MAdapter?.MCallUser?.FirstOrDefault(a => a.Id == CallUserObject.Data.Id); // id >> Call_Id
                    if (ckd == null)
                    {
                        Classes.CallUser cv = new Classes.CallUser
                        {
                            Id = CallUserObject.Data.Id,
                            UserId = CallUserObject.UserId,
                            Avatar = CallUserObject.Avatar,
                            Name = CallUserObject.Name,
                            AccessToken = CallUserObject.Data.AccessToken,
                            AccessToken2 = CallUserObject.Data.AccessToken2,
                            FromId = CallUserObject.Data.FromId,
                            Active = CallUserObject.Data.Active,
                            Time = "Missed call",
                            Status = CallUserObject.Data.Status,
                            RoomName = CallUserObject.Data.RoomName,
                            Type = CallType,
                            TypeIcon = "Cancel",
                            TypeColor = "#FF0000"
                        };

                        TabbedMainActivity.GetInstance()?.LastCallsTab?.MAdapter?.Insert(cv);

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_CallUser(cv);
                    }
                }

                TabbedMainActivity.RunCall = false;
                FinishVideoAudio();
            }
            catch (Exception exception)
            {
                TabbedMainActivity.RunCall = false;
                FinishVideoAudio();
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}