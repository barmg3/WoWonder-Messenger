using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.ViewPager2.Widget;
using Com.Google.Android.Play.Core.Install.Model;
using Google.Android.Material.Tabs;
using Java.IO;
using Newtonsoft.Json;
using TheArtOfDev.Edmodo.Cropper;
using WoWonder.Activities.Call;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.DefaultUser;
using WoWonder.Activities.Editor;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.NearBy;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.Story;
using WoWonder.Activities.Tab.Fragment;
using WoWonder.Activities.Tab.Services;
using WoWonder.Adapters;
using WoWonder.Frameworks.Floating;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Message;
using Console = System.Console;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Tab
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", Exported = false, WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.AdjustPan, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class TabbedMainActivity : AppCompatActivity, TabLayoutMediator.ITabConfigurationStrategy, MaterialDialog.IListCallback 
    {
        #region Variables

        private MainTabAdapter Adapter;
        public TabLayout Tabs;
        private ViewPager2 ViewPager;
        //private FloatingActionButton FloatingActionFilter;
        private FrameLayout FloatingActionButtonView;
        private ImageView FloatingActionImageView;
        private TextView TxtAppName;
        private string FloatingActionTag;
        public LastChatFragment LastChatTab;
        public LastStoriesFragment LastStoriesTab;
        public LastCallsFragment LastCallsTab;
        private static TabbedMainActivity Instance;
        private string ImageType;
        //public static ServiceResultReceiver Receiver;
        public static bool RunCall = false;
        private PowerManager.WakeLock Wl;
        private Handler ExitHandler;
        private bool RecentlyBackPressed;
        public LastGroupChatsFragment LastGroupChatsTab;
        public LastPageChatsFragment LastPageChatsTab;
        //ToolBar 
        private ImageView DiscoverImageView, SearchImageView, MoreImageView;

        public InitFloating Floating;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this);

                //AddFlagsWakeLock();

                // Create your application here
                SetContentView(Resource.Layout.TabbedMainPage);

                Instance = this;
                Floating = new InitFloating();
                RunCall = false;

                Task.Factory.StartNew(GetGeneralAppData);

                //Get Value And Set Toolbar
                InitComponent();
                //InitToolbar();
                SetService();

                if ((int)Build.VERSION.SdkInt > 23 && !PermissionsController.CheckPermissionStorage())
                    new PermissionsController(this).RequestPermission(100);
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
                AddOrRemoveEvent(true);
                SetWakeLock();
                LastStoriesTab?.PublisherAdView?.Resume();
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
                OffWakeLock();
                LastStoriesTab?.PublisherAdView?.Pause();
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

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        MainSettings.ApplyTheme(MainSettings.LightMode);
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                        break;
                }

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
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
                LastStoriesTab?.PublisherAdView?.Destroy();

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                    UserDetails.Socket?.Emit_loggedoutEvent(UserDetails.AccessToken);

                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override void OnBackPressed()
        {
            try
            {
                ExitHandler ??= new Handler(Looper.MainLooper);
                if (RecentlyBackPressed)
                {
                    ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                    RecentlyBackPressed = false;
                    MoveTaskToBack(true);
                    Finish();
                }
                else
                {
                    RecentlyBackPressed = true;
                    ToastUtils.ShowToast(this, GetString(Resource.String.press_again_exit), ToastLength.Long);
                    ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                DiscoverImageView = FindViewById<ImageView>(Resource.Id.discoverButton);
                SearchImageView = FindViewById<ImageView>(Resource.Id.searchButton);
                MoreImageView = FindViewById<ImageView>(Resource.Id.moreButton);
                TxtAppName = FindViewById<TextView>(Resource.Id.appName);
                //FloatingActionFilter = FindViewById<FloatingActionButton>(Resource.Id.floatingActionFilter);

                TxtAppName.Text = AppSettings.ApplicationName;

                FloatingActionButtonView = FindViewById<FrameLayout>(Resource.Id.FloatingAction);
                FloatingActionImageView = FindViewById<ImageView>(Resource.Id.Image);
                FloatingActionTag = "lastMessages";
                Tabs = FindViewById<TabLayout>(Resource.Id.tabsLayout);
                ViewPager = FindViewById<ViewPager2>(Resource.Id.viewpager);

                SetUpViewPager(ViewPager);
                new TabLayoutMediator(Tabs, ViewPager, this).Attach();

                //var tab = Tabs.GetTabAt(0); //Lbl_Tab_Chats 
                ////set custom view
                //tab.SetCustomView(Resource.Layout.IconBadgeLayout);
                //TextView textView = (TextView)tab.CustomView.FindViewById(Resource.Id.text);
                //textView.Visibility = ViewStates.Gone;
                //var d = Color.ParseColor("");

                Tabs.SetTabTextColors(AppSettings.SetTabDarkTheme ? Color.White : Color.DimGray, Color.ParseColor(AppSettings.MainColor));
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
                    FloatingActionButtonView.Click += FloatingActionButtonView_Click;
                    //FloatingActionFilter.Click += FloatingActionFilterOnClick;
                    DiscoverImageView.Click += DiscoverImageView_Click;
                    SearchImageView.Click += SearchImageView_Click;
                    MoreImageView.Click += MoreImageView_Click;
                }
                else
                {
                    FloatingActionButtonView.Click -= FloatingActionButtonView_Click;
                    //FloatingActionFilter.Click -= FloatingActionFilterOnClick;
                    DiscoverImageView.Click -= DiscoverImageView_Click;
                    SearchImageView.Click -= SearchImageView_Click;
                    MoreImageView.Click -= MoreImageView_Click;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static TabbedMainActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        #endregion

        #region Events

        private void MoreImageView_Click(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                switch (FloatingActionTag)
                {
                    case "lastMessages" when AppSettings.LastChatSystem == SystemGetLastChat.MultiTab:
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Blocked_User_List));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                        break;
                    case "lastMessages":
                        arrayAdapter.Add(GetText(Resource.String.Lbl_CreateNewGroup));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_GroupRequest));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Blocked_User_List));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                        break;
                    case "GroupChats":
                        arrayAdapter.Add(GetText(Resource.String.Lbl_CreateNewGroup));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_GroupRequest));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                        break;
                    case "PageChats":
                    case "Story":
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                        break;
                    case "Call":
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Clear_call_log));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Settings));
                        break;
                }

                dialogList.Title(GetString(Resource.String.Lbl_Menu_More));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchImageView_Click(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchActivity));
                intent.PutExtra("Key", "");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DiscoverImageView_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(PeopleNearByActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnVideo_Button_Click()
        {
            try
            {
                ImageType = "Video";

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //requestCode >> 501 => video Gallery
                    new IntentController(this).OpenIntentVideoGallery();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                    {
                        //requestCode >> 501 => video Gallery
                        new IntentController(this).OpenIntentVideoGallery();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnImage_Button_Click()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (AppSettings.ImageCropping)
                        OpenDialogGallery("Image"); //requestCode >> 500 => Image Gallery
                    else
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage())
                    {
                        if (AppSettings.ImageCropping)
                            OpenDialogGallery("Image"); //requestCode >> 500 => Image Gallery
                        else
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception exe)
            {
                Methods.DisplayReportResultTrack(exe);
            }
        }

        private void FloatingActionButtonView_Click(object sender, EventArgs e)
        {
            try
            {
                switch (FloatingActionTag)
                {
                    case "lastMessages":
                        {
                            var intent = new Intent(this, typeof(UserContactsActivity));
                            intent.PutExtra("ContactsType", "Following");
                            intent.PutExtra("UserId", UserDetails.UserId);
                            StartActivity(intent);
                            break;
                        }
                    case "GroupChats":
                        {
                            var intent = new Intent(this, typeof(CreateGroupChatActivity));
                            StartActivity(intent);
                            break;
                        }
                    case "Story":
                        {
                            var intent = new Intent(this, typeof(AddStoryActivity));
                            StartActivity(intent);
                            break;
                        }
                    case "Call":
                        StartActivity(new Intent(this, typeof(AddNewCallActivity)));
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                switch (requestCode)
                {
                    // Add video story
                    case 501 when resultCode == Result.Ok:
                        {
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (filepath != null)
                            {
                                var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                                if (type == "Video")
                                {
                                    if (AppSettings.EnableVideoEditor)
                                    {
                                        Intent intent = new Intent(this, typeof(VideoEditorActivity));
                                        intent.PutExtra("Uri", filepath);
                                        intent.PutExtra("Thumbnail", filepath);
                                        intent.PutExtra("Type", "Story");
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Intent intent = new Intent(this, typeof(AddStoryActivity));
                                        intent.PutExtra("Uri", filepath);
                                        intent.PutExtra("Thumbnail", filepath);
                                        intent.PutExtra("Type", "video");
                                        StartActivity(intent);
                                    }
                                }
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                            }

                            break;
                        }
                    // Add image story
                    case 500 when resultCode == Result.Ok:
                        {
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (filepath != null)
                            {
                                var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                                if (type == "Image")
                                {
                                    if (!string.IsNullOrEmpty(filepath))
                                    {
                                        //Do something with your Uri
                                        Intent intent = new Intent(this, typeof(AddStoryActivity));
                                        intent.PutExtra("Uri", filepath);
                                        intent.PutExtra("Type", "image");
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                                    }
                                }
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                            }

                            break;
                        }
                    case CropImage.CropImageActivityRequestCode when resultCode == Result.Ok:
                        {
                            var result = CropImage.GetActivityResult(data);
                            if (result.IsSuccessful)
                            {
                                var resultUri = result.Uri;

                                if (!string.IsNullOrEmpty(resultUri.Path))
                                {
                                    //Do something with your Uri
                                    Intent intent = new Intent(this, typeof(AddStoryActivity));
                                    intent.PutExtra("Uri", resultUri.Path);
                                    intent.PutExtra("Type", "image");
                                    StartActivity(intent);
                                }
                                else
                                {
                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                                }
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                            }

                            break;
                        }
                    default:
                        {
                            if (requestCode == InitFloating.ChatHeadDataRequestCode && InitFloating.CanDrawOverlays(this))
                            {
                                Floating.FloatingShow(InitFloating.FloatingObject);

                                UserDetails.ChatHead = true;
                                MainSettings.SharedData?.Edit()?.PutBoolean("chatheads_key", UserDetails.ChatHead)?.Commit();
                            }
                            else switch (requestCode)
                                {
                                    // => NiceArtEditor add story text
                                    case 2200 when resultCode == Result.Ok:
                                        RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                var imagePath = data.GetStringExtra("ImagePath") ?? "Data not available";
                                                if (imagePath != "Data not available" && !string.IsNullOrEmpty(imagePath))
                                                {
                                                    //Do something with your Uri
                                                    Intent intent = new Intent(this, typeof(AddStoryActivity));
                                                    intent.PutExtra("Uri", imagePath);
                                                    intent.PutExtra("Type", "image");
                                                    StartActivity(intent);
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        });
                                        break;
                                    case 4711:
                                        switch (resultCode) // The switch block will be triggered only with flexible update since it returns the install result codes
                                        {
                                            case Result.Ok:
                                                // In app update success
                                                if (UpdateManagerApp.AppUpdateTypeSupported == AppUpdateType.Immediate)
                                                    ToastUtils.ShowToast(this, "App updated", ToastLength.Short);
                                                break;
                                            case Result.Canceled:
                                                ToastUtils.ShowToast(this, "In app update cancelled", ToastLength.Short);
                                                break;
                                            case (Result)ActivityResult.ResultInAppUpdateFailed:
                                                ToastUtils.ShowToast(this, "In app update failed", ToastLength.Short);
                                                break;
                                        }

                                        break;
                                }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        switch (ImageType)
                        {
                            //requestCode >> 500 => Image Gallery
                            case "Image" when AppSettings.ImageCropping:
                                OpenDialogGallery("Image");
                                break;
                            case "Image": //requestCode >> 500 => Image Gallery
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false);
                                break;
                            case "Video":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "Camera":
                                //requestCode >> 503 => Camera
                                new IntentController(this).OpenIntentCamera();
                                break;
                        }

                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 110 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                        break;
                    case 110:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Methods.Path.Chack_MyFolder();
                        break;
                    case 100:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                LastChatTab = new LastChatFragment();
                LastStoriesTab = new LastStoriesFragment(this);
                LastCallsTab = new LastCallsFragment();

                Adapter = new MainTabAdapter(this);
                switch (AppSettings.LastChatSystem)
                {
                    case SystemGetLastChat.Default:
                        Adapter.AddFragment(LastChatTab, GetText(Resource.String.Lbl_Tab_Chats));
                        break;
                    case SystemGetLastChat.MultiTab:
                        {
                            LastGroupChatsTab = new LastGroupChatsFragment();
                            LastPageChatsTab = new LastPageChatsFragment();

                            Adapter.AddFragment(LastChatTab, GetText(Resource.String.Lbl_Tab_Chats));
                            if (AppSettings.EnableChatGroup)
                                Adapter.AddFragment(LastGroupChatsTab, GetText(Resource.String.Lbl_Tab_GroupChats));

                            if (AppSettings.EnableChatPage)
                                Adapter.AddFragment(LastPageChatsTab, GetText(Resource.String.Lbl_Tab_PageChats));

                            break;
                        }
                }

                Adapter.AddFragment(LastStoriesTab, GetText(Resource.String.Lbl_Tab_Stories));

                if (AppSettings.EnableAudioVideoCall)
                    Adapter.AddFragment(LastCallsTab, GetText(Resource.String.Lbl_Tab_Calls));

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();

                viewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly TabbedMainActivity Activity;

            public MyOnPageChangeCallback(TabbedMainActivity activity)
            {
                try
                {
                    Activity = activity;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnPageSelected(int position)
            {
                try
                {
                    base.OnPageSelected(position);

                    if (AppSettings.LastChatSystem == SystemGetLastChat.Default)
                    {
                        switch (position)
                        {
                            // lastMessages
                            case 0:
                                AdsGoogle.Ad_Interstitial(Activity);
                                break;
                            // Story
                            case 1:
                                AdsGoogle.Ad_RewardedVideo(Activity);
                                //Activity.LastStoriesTab.Task.Factory.StartNew(() => StartApiService());
                                Activity.InAppReview();
                                break;
                            // Call
                            case 2:
                                AdsGoogle.Ad_RewardedInterstitial(Activity);
                                //Activity.LastCallsTab.Get_CallUser();
                                break;
                        }
                    }
                    else
                    {
                        switch (position)
                        {
                            // lastMessages
                            case 0:
                                AdsGoogle.Ad_AppOpenManager(Activity);
                                break;
                            // GroupChats
                            case 1:
                                AdsGoogle.Ad_RewardedVideo(Activity);
                                //if (AppSettings.EnableChatGroup)
                                //{
                                //    LastGroupChatsTab.Task.Factory.StartNew(() => StartApiService());
                                //}
                                //else if (AppSettings.EnableChatPage)
                                //{
                                //    LastPageChatsTab.Task.Factory.StartNew(() => StartApiService());
                                //}
                                //else
                                //{
                                //    LastStoriesTab.Task.Factory.StartNew(() => StartApiService());
                                //}
                                break;
                            // PageChats
                            case 2:
                                AdsGoogle.Ad_Interstitial(Activity);
                                //if (AppSettings.EnableChatPage)
                                //{
                                //    LastPageChatsTab.Task.Factory.StartNew(() => StartApiService());
                                //}
                                //else
                                //{
                                //    LastStoriesTab.Task.Factory.StartNew(() => StartApiService());
                                //}
                                break;
                            // Story
                            case 3:
                                AdsGoogle.Ad_AppOpenManager(Activity);
                                //if (AppSettings.EnableChatGroup)
                                //{
                                //    LastStoriesTab.Task.Factory.StartNew(() => StartApiService());
                                //}
                                //else
                                //{
                                //    LastCallsTab.Get_CallUser();
                                //}
                                Activity.InAppReview();
                                break;
                            // Call
                            case 4:
                                AdsGoogle.Ad_RewardedInterstitial(Activity);
                                //LastCallsTab.Get_CallUser();
                                break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                try
                {
                    base.OnPageScrolled(position, positionOffset, positionOffsetPixels);

                    if (AppSettings.LastChatSystem == SystemGetLastChat.Default)
                    {
                        switch (position)
                        {
                            // lastMessages
                            case 0:
                                {
                                    if (Activity.FloatingActionTag != "lastMessages")
                                    {
                                        Activity.FloatingActionTag = "lastMessages";
                                        Activity.FloatingActionImageView.SetImageResource(Resource.Drawable.icon_profile_vector);
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Visible;
                                        //FloatingActionFilter.Visibility = ViewStates.Visible;
                                    }

                                    break;
                                }
                            // Story
                            case 1:
                                {
                                    if (Activity.FloatingActionTag != "Story")
                                    {
                                        Activity.FloatingActionTag = "Story";
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Invisible;
                                        //FloatingActionFilter.Visibility = ViewStates.Invisible;

                                        //if (Tabs != null)
                                        //{
                                        //    var tab = Tabs.GetTabAt(0); //Lbl_Tab_Chats

                                        //    var textView = (TextView)tab.CustomView.FindViewById(Resource.Id.text);
                                        //    textView.Visibility = ViewStates.Gone;
                                        //}
                                    }

                                    break;
                                }
                            // Call
                            case 2:
                                {
                                    if (Activity.FloatingActionTag != "Call")
                                    {
                                        Activity.FloatingActionTag = "Call";
                                        Activity.FloatingActionImageView.SetImageResource(Resource.Drawable.ic_phone_user);
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Visible;
                                        //FloatingActionFilter.Visibility = ViewStates.Invisible;

                                        //if (Tabs != null)
                                        //{
                                        //    var tab = Tabs.GetTabAt(0); //Lbl_Tab_Chats

                                        //    var textView = (TextView)tab.CustomView.FindViewById(Resource.Id.text);
                                        //    textView.Visibility = ViewStates.Gone;
                                        //}
                                    }

                                    break;
                                }
                        }
                    }
                    else
                    {
                        switch (position)
                        {
                            //FloatingActionFilter.Visibility = ViewStates.Invisible;
                            // lastMessages
                            case 0:
                                {
                                    if (Activity.FloatingActionTag != "lastMessages")
                                    {
                                        Activity.FloatingActionTag = "lastMessages";
                                        Activity.FloatingActionImageView.SetImageResource(Resource.Drawable.icon_profile_vector);
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Visible;
                                    }

                                    break;
                                }
                            // GroupChats
                            case 1 when AppSettings.EnableChatGroup:
                                {
                                    if (Activity.FloatingActionTag != "GroupChats")
                                    {
                                        Activity.FloatingActionTag = "GroupChats";
                                        Activity.FloatingActionImageView.SetImageResource(Resource.Drawable.ic_add);
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Visible;
                                    }

                                    break;
                                }
                            case 1 when AppSettings.EnableChatPage:
                                {
                                    if (Activity.FloatingActionTag != "PageChats")
                                    {
                                        Activity.FloatingActionTag = "PageChats";
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Invisible;
                                    }

                                    break;
                                }
                            case 1:
                                {
                                    if (Activity.FloatingActionTag != "Story")
                                    {
                                        Activity.FloatingActionTag = "Story";
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Invisible;
                                    }

                                    break;
                                }
                            // PageChats
                            case 2 when AppSettings.EnableChatPage:
                                {
                                    if (Activity.FloatingActionTag != "PageChats")
                                    {
                                        Activity.FloatingActionTag = "PageChats";
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Invisible;
                                    }

                                    break;
                                }
                            case 2:
                                {
                                    if (Activity.FloatingActionTag != "Story")
                                    {
                                        Activity.FloatingActionTag = "Story";
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Invisible;
                                    }

                                    break;
                                }
                            // Story
                            case 3 when AppSettings.EnableChatPage:
                                {
                                    if (Activity.FloatingActionTag != "Story")
                                    {
                                        Activity.FloatingActionTag = "Story";
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Invisible;
                                    }
                                    else
                                    {
                                        if (Activity.FloatingActionTag != "Call")
                                        {
                                            Activity.FloatingActionTag = "Call";
                                            Activity.FloatingActionImageView.SetImageResource(Resource.Drawable.ic_phone_user);
                                            Activity.FloatingActionButtonView.Visibility = ViewStates.Visible;
                                        }
                                    }

                                    break;
                                }
                            case 3:
                                {
                                    if (Activity.FloatingActionTag != "Call")
                                    {
                                        Activity.FloatingActionTag = "Call";
                                        Activity.FloatingActionImageView.SetImageResource(Resource.Drawable.ic_phone_user);
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Visible;
                                    }

                                    break;
                                }
                            // Call
                            case 4:
                                {
                                    if (Activity.FloatingActionTag != "Call")
                                    {
                                        Activity.FloatingActionTag = "Call";
                                        Activity.FloatingActionImageView.SetImageResource(Resource.Drawable.ic_phone_user);
                                        Activity.FloatingActionButtonView.Visibility = ViewStates.Visible;
                                    }

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
        }

        #endregion

        #region WakeLock System

        public void AddFlagsWakeLock()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.WakeLock) == Permission.Granted)
                    {
                        Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        //request Code 110
                        new PermissionsController(this).RequestPermission(110);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetWakeLock()
        {
            try
            {
                if (Wl == null)
                {
                    PowerManager pm = (PowerManager)GetSystemService(PowerService);
                    Wl = pm?.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                    Wl?.Acquire();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOnWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm?.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                Wl?.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOffWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm?.NewWakeLock(WakeLockFlags.ProximityScreenOff, "My Tag");
                Wl?.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Service

        private void SetService(bool run = true)
        {
            try
            {
                if (run)
                {
                    // reschedule the job
                    ChatJobInfo.ScheduleJob(this);
                }
                else
                {
                    // Cancel all jobs
                    ChatJobInfo.StopJob(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        

        public void OnReceiveResult(string resultData)
        {
            try
            {
                //ToastUtils.ShowToast(Application.Context, "Result got ", ToastLength.Short);

                var result = JsonConvert.DeserializeObject<LastChatObject>(resultData);
                if (result != null)
                {
                    LastChatTab?.LoadDataLastChatNewV(result);
                    RunOnUiThread(() => { LastChatFragment.LoadCall(result); });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void OpenDialogGallery(string typeImage)
        {
            try
            {
                ImageType = typeImage;
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && PermissionsController.CheckPermissionStorage() && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenEditColor()
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditColorActivity));
                StartActivityForResult(intent, 2200);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        
        #region General App Data

        private void GetGeneralAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();
                var data = sqlEntity.Get_data_Login_Credentials();
                if (data != null && data.Status != "Active")
                {
                    data.Status = "Active";
                    UserDetails.Status = "Active";
                    sqlEntity.InsertOrUpdateLogin_Credentials(data);

                    RunOnUiThread(() =>
                    {
                        if (!InitFloating.CanDrawOverlays(this))
                            DisplayChatHeadDialog();
                    });
                }

                Task.Run(() =>
                {
                    try
                    {
                        sqlEntity.Get_MyProfile();

                        ListUtils.MuteList = sqlEntity.Get_MuteList();
                        ListUtils.PinList = sqlEntity.Get_PinList();
                        ListUtils.ArchiveList = sqlEntity.Get_ArchiveList();

                        ListUtils.UserList = sqlEntity.Get_LastUsersChat_List();

                        ListUtils.StickersList = sqlEntity.Get_From_StickersTb();

                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetPinChats, ApiRequest.GetArchivedChats });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.Get_MyProfileData_Api });

                LoadConfigSettings();

                InAppUpdate();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void GetOneSignalNotification()
        {
            try
            {
                string userId = Intent?.GetStringExtra("userId") ?? "";
                string pageId = Intent?.GetStringExtra("PageId") ?? "";
                string groupId = Intent?.GetStringExtra("GroupId") ?? "";
                string type = Intent?.GetStringExtra("type") ?? "";

                Intent intent = null!;
                switch (type)
                {
                    case "user":
                        {
                            var item = LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == userId && a.LastChat?.ChatType == "user");
                            string mainChatColor = AppSettings.MainColor;

                            intent = new Intent(this, typeof(ChatWindowActivity));
                            intent.PutExtra("UserID", userId);
                            intent.PutExtra("ShowEmpty", "no");

                            if (item?.LastChat != null)
                            {
                                if (!WoWonderTools.ChatIsAllowed(item.LastChat))
                                    return;

                                if (item.LastChat.LastMessage.LastMessageClass != null)
                                    mainChatColor = item.LastChat.LastMessage.LastMessageClass.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(item.LastChat.LastMessage.LastMessageClass.ChatColor) : item.LastChat.LastMessage.LastMessageClass.ChatColor ?? AppSettings.MainColor;

                                intent.PutExtra("TypeChat", "LastMessenger");
                                intent.PutExtra("ChatId", item.LastChat.ChatId);
                                intent.PutExtra("ColorChat", mainChatColor);
                                intent.PutExtra("UserItem", JsonConvert.SerializeObject(item.LastChat));
                            }
                            else
                            {
                                intent.PutExtra("TypeChat", "OneSignalNotification");
                                intent.PutExtra("ColorChat", mainChatColor);
                            }
                            break;
                        }
                    case "page":
                        {
                            Classes.LastChatsClass item;
                            if (AppSettings.LastChatSystem == SystemGetLastChat.Default)
                                item = LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.PageId == pageId && a.LastChat?.ChatType == "page");
                            else
                                item = LastPageChatsTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChatPage?.PageId == pageId);

                            intent = new Intent(this, typeof(PageChatWindowActivity));
                            intent.PutExtra("ShowEmpty", "no");
                            intent.PutExtra("PageId", pageId);

                            if (item?.LastChat != null)
                            {
                                intent.PutExtra("ChatId", item.LastChat.ChatId);
                                intent.PutExtra("PageObject", JsonConvert.SerializeObject(item.LastChat));
                                intent.PutExtra("TypeChat", "");
                            }
                            else if (item?.LastChatPage != null)
                            {
                                intent.PutExtra("ChatId", item.LastChatPage.ChatId);
                                intent.PutExtra("PageObject", JsonConvert.SerializeObject(item.LastChatPage));
                                intent.PutExtra("TypeChat", "PageProfile");
                            }
                            break;
                        }
                    case "group":
                        {
                            Classes.LastChatsClass item;
                            if (AppSettings.LastChatSystem == SystemGetLastChat.Default)
                                item = LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == groupId && a.LastChat?.ChatType == "group");
                            else
                                item = LastGroupChatsTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.GroupId == groupId);

                            intent = new Intent(this, typeof(GroupChatWindowActivity));
                            intent.PutExtra("ShowEmpty", "no");
                            intent.PutExtra("GroupId", groupId);

                            if (item?.LastChat != null)
                            {
                                intent.PutExtra("ChatId", item.LastChat.ChatId);
                                intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item.LastChat));
                            }

                            break;
                        }
                }

                if (intent != null)
                    StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InAppUpdate()
        {
            RunOnUiThread(() =>
            {
                try
                {
                    if (AppSettings.ShowSettingsUpdateManagerApp)
                        UpdateManagerApp.CheckUpdateApp(this, 4711, Intent);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        private static int CountRateApp;
        private void InAppReview()
        {
            try
            {
                bool inAppReview = MainSettings.InAppReview?.GetBoolean(MainSettings.PrefKeyInAppReview, false) ?? false;
                if (!inAppReview && AppSettings.ShowSettingsRateApp)
                {
                    if (CountRateApp == AppSettings.ShowRateAppCount)
                    {
                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialog.Title(GetText(Resource.String.Lbl_RateOurApp));
                        dialog.Content(GetText(Resource.String.Lbl_RateOurAppContent));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Rate)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                StoreReviewApp store = new StoreReviewApp();
                                store.OpenStoreReviewPage(PackageName);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();

                        MainSettings.InAppReview?.Edit()?.PutBoolean(MainSettings.PrefKeyInAppReview, true)?.Commit();
                    }
                    else
                    {
                        CountRateApp++;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

       #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Lbl_CreateNewGroup))
                {
                    StartActivity(new Intent(this, typeof(CreateGroupChatActivity)));
                }
                else if (itemString == GetText(Resource.String.Lbl_GroupRequest))
                {
                    StartActivity(new Intent(this, typeof(GroupRequestActivity)));
                }
                else if (itemString == GetText(Resource.String.Lbl_Blocked_User_List))
                {
                    StartActivity(new Intent(this, typeof(BlockedUsersActivity)));
                }
                else if (itemString == GetText(Resource.String.Lbl_Settings))
                {
                    StartActivity(new Intent(this, typeof(SettingsActivity)));
                }
                else if (itemString == GetText(Resource.String.Lbl_Clear_call_log))
                {
                    var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialogBuilder.Title(GetText(Resource.String.Lbl_Warning));
                    dialogBuilder.Content(GetText(Resource.String.Lbl_Clear_call_log));
                    dialogBuilder.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            LastCallsTab?.MAdapter?.MCallUser?.Clear();
                            LastCallsTab?.MAdapter?.NotifyDataSetChanged();
                            LastCallsTab?.ShowEmptyPage();

                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Done), ToastLength.Long);

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.Clear_CallUser_List();

                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                    dialogBuilder.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(new MyMaterialDialog());
                    dialogBuilder.AlwaysCallSingleChoiceCallback();
                    dialogBuilder.Build().Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Chat Head

        private Dialog ChatHeadWindow;
        private void DisplayChatHeadDialog()
        {
            try
            {
                if (AppSettings.ShowChatHeads)
                {
                    UserDetails.OpenDialog = MainSettings.SharedData.GetBoolean("OpenDialogChatHead_key", false);

                    if (UserDetails.OpenDialog) return;
                    if (InitFloating.CanDrawOverlays(this)) return;

                    ChatHeadWindow = new Dialog(this, AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                    ChatHeadWindow.SetContentView(Resource.Layout.ChatHeadDialogLayout);

                    var subTitle1 = ChatHeadWindow.FindViewById<TextView>(Resource.Id.subTitle1);
                    var btnNotNow = ChatHeadWindow.FindViewById<TextView>(Resource.Id.notNowButton);
                    var btnGoToSettings = ChatHeadWindow.FindViewById<AppCompatButton>(Resource.Id.goToSettingsButton);

                    subTitle1.Text = GetText(Resource.String.Lbl_EnableChatHead_SubTitle1) + " " + AppSettings.ApplicationName + ", " + GetText(Resource.String.Lbl_EnableChatHead_SubTitle2);

                    btnNotNow.Click += BtnNotNowOnClick;
                    btnGoToSettings.Click += BtnGoToSettingsOnClick;

                    ChatHeadWindow.Show();

                    UserDetails.OpenDialog = true;
                    MainSettings.SharedData?.Edit()?.PutBoolean("OpenDialogChatHead_key", true)?.Commit();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BtnGoToSettingsOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Floating.CheckPermission())
                {
                    Floating.OpenManagePermission();
                }

                if (ChatHeadWindow != null)
                {
                    ChatHeadWindow.Hide();
                    ChatHeadWindow.Dispose();
                    ChatHeadWindow = null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnNotNowOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ChatHeadWindow != null)
                {
                    ChatHeadWindow.Hide();
                    ChatHeadWindow.Dispose();
                    ChatHeadWindow = null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}