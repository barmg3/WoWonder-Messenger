using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS; 
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Tab.Fragment;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SocketSystem;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using Exception = Java.Lang.Exception;

namespace WoWonder.Activities.Tab.Services
{
    [Service(Exported = false, Permission = "android.permission.BIND_JOB_SERVICE")]
    public class ChatApiService : JobService
    {
        public static JobService Instance;
        public static JobParameters JobParameters;

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                //Toast.MakeText(Application.Context, "OnCreate", ToastLength.Short)?.Show();
                new Handler(Looper.MainLooper).PostDelayed(new ChatUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                base.OnStartCommand(intent, flags, startId);

                new Handler(Looper.MainLooper).PostDelayed(new ChatUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
                //Toast.MakeText(Application.Context, "OnStartCommand", ToastLength.Short)?.Show();

                return StartCommandResult.Sticky;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return StartCommandResult.NotSticky;
            }
        }

        public override bool OnStartJob(JobParameters jobParams)
        {
            //Toast.MakeText(Application.Context, "On Start Job " + Methods.AppLifecycleObserver.AppState, ToastLength.Short)?.Show();

            Instance = this;
            JobParameters = jobParams;
            new Handler(Looper.MainLooper).PostDelayed(new ChatUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);

            // Our task will run in background, we will take care of notifying the finish.
            return true;
        }

        public override bool OnStopJob(JobParameters jobParams)
        {
            //Toast.MakeText(Application.Context, "On Stop Job 321" + Methods.AppLifecycleObserver.AppState, ToastLength.Short)?.Show();
            // I want it to reschedule so returned true, if we would have returned false, then job would have ended here.
            // It would not fire onStartJob() when constraints are re satisfied.

            new Handler(Looper.MainLooper).PostDelayed(new ChatUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);

            return false;
        }
    }

    public static class ChatJobInfo
    {
        public static void ScheduleJob(Context context)
        {
            try
            {
                ComponentName serviceComponent = new ComponentName(context, Class.FromType(typeof(ChatApiService)));

                JobInfo jobInfo;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    JobInfo.Builder builder = new JobInfo.Builder(1, serviceComponent);
                    builder.SetMinimumLatency(AppSettings.RefreshChatActivitiesSeconds); // wait at least
                    builder.SetOverrideDeadline(0); // maximum delay
                    builder.SetRequiredNetworkType(NetworkType.Unmetered); // require unmetered network
                    //builder.SetRequiresDeviceIdle(true); // the device should be idle
                    builder.SetRequiresCharging(false); // we don't care if the device is charging or not 

//                    if (Build.VERSION.SdkInt == BuildVersionCodes.S)
//                        builder.SetExpedited(true);
//#pragma warning disable CS0618
//                    else if (Build.VERSION.SdkInt == BuildVersionCodes.R)
//                        builder.SetImportantWhileForeground(false);
//#pragma warning restore CS0618

                    builder.SetPersisted(true);
                    jobInfo = builder?.Build();
                }
                else
                {
                    jobInfo = new JobInfo.Builder(1, serviceComponent)?.SetPeriodic(AppSettings.RefreshChatActivitiesSeconds)?.Build();
                }

                var jobScheduler = (JobScheduler)context.GetSystemService(Context.JobSchedulerService);
                if (jobInfo != null)
                    jobScheduler?.Schedule(jobInfo);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void StopJob(Context context)
        {
            try
            {
                var jobScheduler = (JobScheduler)context.GetSystemService(Context.JobSchedulerService);
                jobScheduler?.CancelAll();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class ChatUpdaterHelper : Java.Lang.Object, IRunnable
    {
        private static Handler MainHandler;

        public ChatUpdaterHelper(Handler mainHandler)
        {
            MainHandler = mainHandler;
        }

        public void Run()
        {
            try
            {
                if (string.IsNullOrEmpty(Methods.AppLifecycleObserver.AppState))
                    Methods.AppLifecycleObserver.AppState = "Background";

                //ToastUtils.ShowToast(Application.Context, "AppState " + Methods.AppLifecycleObserver.AppState, ToastLength.Short);

                if (Methods.AppLifecycleObserver.AppState == "Background" && string.IsNullOrEmpty(Current.AccessToken))
                {
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    var login = dbDatabase.Get_data_Login_Credentials();
                    Console.WriteLine(login);
                }

                if (string.IsNullOrEmpty(Current.AccessToken))
                    return;

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    if (UserDetails.Socket == null)
                    {
                        UserDetails.Socket = new WoSocketHandler();
                        UserDetails.Socket?.InitStart();

                        //Connect to socket with access token
                        UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
                    }
                    else
                    {
                        if (UserDetails.Socket.Client is { Connected: false } || !WoSocketHandler.IsJoined)
                        {
                            //Connect to socket with access token
                            UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
                        }
                    }
                }
                else
                {
                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadChatAsync });
                }

                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(new ChatUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                //ToastUtils.ShowToast(Application.Context, "ResultSender failed",ToastLength.Short);
                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(new ChatUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async Task LoadChatAsync()
        {
            try
            {
                //ToastUtils.ShowToast(Application.Context, "StartApiService", ToastLength.Short);
                if (LastChatFragment.ApiRun)
                    return;

                LastChatFragment.ApiRun = true;

                var fetch = "users";
                if (AppSettings.EnableChatGroup)
                    fetch += ",groups";

                if (AppSettings.EnableChatPage)
                    fetch += ",pages";

                var (apiStatus, respond) = await RequestsAsync.Message.GetChatAsync(fetch, "", "0", "25", "0", "25", "0", "25");
                if (apiStatus != 200 || respond is not LastChatObject result || result.Data == null)
                {
                    LastChatFragment.ApiRun = false;
                    //Methods.DisplayReportResult(new Activity(), respond);
                }
                else
                {
                    LastChatFragment.LoadCall(result);
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (Methods.AppLifecycleObserver.AppState == "Foreground")
                        {
                            TabbedMainActivity.GetInstance()?.OnReceiveResult(JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            ListUtils.UserList = new ObservableCollection<ChatObject>(result.Data);

                            //Insert All data users to database
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.Insert_Or_Update_LastUsersChat(Application.Context, ListUtils.UserList, UserDetails.ChatHead);
                            LastChatFragment.ApiRun = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                LastChatFragment.ApiRun = false;
            }
        }
    }
}