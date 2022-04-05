using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Com.Aghajari.Emojiview.View;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Ads;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.Animations;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using AndroidX.Interpolator.View.Animation;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget; 
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Google.Android.Material.FloatingActionButton;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using Refractored.Controls;
using TheArtOfDev.Edmodo.Cropper;
using WoWonder.Activities.ChatWindow.Adapters;
using WoWonder.Activities.ChatWindow.Fragment;
using WoWonder.Activities.Editor;
using WoWonder.Activities.Gif;
using WoWonder.Activities.SettingsPreferences;
using WoWonder.Activities.SharedFiles;
using WoWonder.Activities.StickersView;
using WoWonder.Activities.Tab;
using WoWonder.Activities.Viewer;
using WoWonder.Adapters;
using WoWonder.Frameworks.Agora;
using WoWonder.Frameworks.Twilio;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.XRecordView;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using MessageData = WoWonder.Helpers.Model.MessageDataExtra;
using SupportFragment = AndroidX.Fragment.App.Fragment;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.ChatWindow
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ResizeableActivity = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class ChatWindowActivity : AppCompatActivity, MaterialDialog.IListCallback, View.IOnLayoutChangeListener, IOnRecordClickListener, IOnRecordListener, SwipeReply.ISwipeControllerActions
    {
        #region Variables Basic

        public ImageView ChatEmojImage;
        private LinearLayout RootView;
        private LinearLayout LayoutEditText;
        public AXEmojiEditText EmojIconEditTextView;
        private CircleImageView UserChatProfile;
        public ImageView ChatMediaButton;
        public RecyclerView MRecycler;
        private RecyclerView RecyclerHiSuggestion;
        private ChatColorsFragment ChatColorBoxFragment;
        private ChatRecordSoundFragment ChatRecordSoundBoxFragment;
        private FrameLayout ButtonFragmentHolder;
        public FrameLayout TopFragmentHolder;
        private Holders.MsgPreCachingLayoutManager LayoutManager;
        public MessageAdapter MAdapter;
        private SupportFragment MainFragmentOpened;
        private Methods.AudioRecorderAndPlayer RecorderService;
        private FastOutSlowInInterpolator Interpolation;
        public static string MainChatColor = AppSettings.MainColor;
        private string GifFile, PermissionsType, TypeChat, Notifier, ShowEmpty;
        public string LastSeen, TaskWork, ChatColorButtonTag;
        private Timer Timer;
        private bool IsRecording;
        public static bool ColorChanged;
        public ChatObject DataUser;
        public UserDataObject UserData;
        public string UserId, ChatId, ReplyId = "0"; // to_id
        private static ChatWindowActivity Instance;
        private TabbedMainActivity GlobalContext;
        private LinearLayout FirstLiner, FirstBoxOnButton;
        private RelativeLayout SayHiLayout;
        private RecyclerView SayHiSuggestionsRecycler;
        private EmptySuggestionMessagesAdapter SuggestionAdapter;
        //Action Bar Buttons 
        private ImageView BackButton, AudioCallButton, VideoCallButton, MoreButton;
        public TextView ActionBarTitle, ActionBarSubTitle;
        //Say Hi 
        private TextView SayHiToTextView;
        public AdapterModelsClassMessage SelectedItemPositions;
        public ObservableCollection<AdapterModelsClassMessage> StartedMessageList = new ObservableCollection<AdapterModelsClassMessage>();
        public ObservableCollection<AdapterModelsClassMessage> PinnedMessageList = new ObservableCollection<AdapterModelsClassMessage>();
        private AdView MAdView;

        private RecordView RecordView;
        public RecordButton RecordButton;

        private FloatingActionButton FabScrollDown;

        private LinearLayout PinMessageView;
        private TextView ShortPinMessage;

        private LinearLayout LoadingLayout;

        private LinearLayout RepliedMessageView;
        private TextView TxtOwnerName, TxtMessageType, TxtShortMessage;
        private ImageView MessageFileThumbnail, BtnCloseReply;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                ChatId = Intent?.GetStringExtra("ChatId") ?? "";
                UserId = Intent?.GetStringExtra("UserID") ?? "";
                TypeChat = Intent?.GetStringExtra("TypeChat") ?? "";
                ShowEmpty = Intent?.GetStringExtra("ShowEmpty") ?? "";

                Methods.App.FullScreenApp(this);

                //Set ToolBar and data chat
                FirstLoadData_Item();

                //Window?.SetStatusBarColor(Color.ParseColor(MainChatColor));
                SetTheme(MainChatColor);

                // Set our view from the "ChatWindow" layout resource
                SetContentView(Resource.Layout.ChatWindowLayout);

                Instance = this;
                GlobalContext = TabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();

                GetOneSignalNotification();

                LoadData_ItemUser();

                AdsGoogle.Ad_Interstitial(this);
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
                AddOrRemoveEvent(true);

                if (Timer != null)
                {
                    Timer.Enabled = true;
                    Timer.Start();
                }

                MAdView?.Resume();
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
                MAdView?.Pause();

                AddOrRemoveEvent(false);

                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }
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

        protected override void OnStart()
        {
            try
            {
                ResetMediaPlayer();
                base.OnStart();
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
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                    Timer.Dispose();
                    Timer = null!;
                }

                ResetMediaPlayer();
                MAdView?.Destroy();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        #endregion

        #region Menu

        private void OnMenuPhoneCallIcon_Click()
        {
            try
            {
                bool granted = ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.RecordAudio) == Permission.Granted;
                if (granted)
                {
                    StartCall();
                }
                else
                {
                    RequestPermissions(new[]
                    {
                        Manifest.Permission.RecordAudio,
                        Manifest.Permission.ModifyAudioSettings
                    }, 1106);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartCall()
        {
            try
            {
                Intent intentVideoCall = new Intent(this, typeof(TwilioVideoCallActivity));
                if (AppSettings.UseLibrary == SystemCall.Agora)
                {
                    intentVideoCall = new Intent(this, typeof(AgoraAudioCallActivity));
                    intentVideoCall.PutExtra("type", "Agora_audio_calling_start");
                }
                else if (AppSettings.UseLibrary == SystemCall.Twilio)
                {
                    intentVideoCall = new Intent(this, typeof(TwilioAudioCallActivity));
                    intentVideoCall.PutExtra("type", "Twilio_audio_calling_start");
                }

                if (DataUser != null)
                {
                    var callUserObject = new CallUserObject
                    {
                        UserId = DataUser.UserId,
                        Avatar = DataUser.Avatar,
                        Name = DataUser.Name,
                        Data = new CallUserObject.DataCallUser()
                    };
                    intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));
                }
                else if (UserData != null)
                {
                    var callUserObject = new CallUserObject
                    {
                        UserId = UserData.UserId,
                        Avatar = UserData.Avatar,
                        Name = UserData.Name,
                        Data = new CallUserObject.DataCallUser()
                    };
                    intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));
                }

                StartActivity(intentVideoCall);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartVideoCall()
        {
            try
            {
                string timeNow = DateTime.Now.ToString("hh:mm");
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time = Convert.ToString(unixTimestamp);

                Intent intentVideoCall = new Intent(this, typeof(TwilioVideoCallActivity));
                if (AppSettings.UseLibrary == SystemCall.Agora)
                {
                    intentVideoCall = new Intent(this, typeof(AgoraVideoCallActivity));
                    intentVideoCall.PutExtra("type", "Agora_video_calling_start");
                }
                else if (AppSettings.UseLibrary == SystemCall.Twilio)
                {
                    intentVideoCall = new Intent(this, typeof(TwilioVideoCallActivity));
                    intentVideoCall.PutExtra("type", "Twilio_video_calling_start");
                }

                if (DataUser != null)
                {
                    var callUserObject = new CallUserObject
                    {
                        UserId = DataUser.UserId,
                        Avatar = DataUser.Avatar,
                        Name = DataUser.Name,
                        Data = new CallUserObject.DataCallUser()
                    };
                    intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));

                }
                else if (UserData != null)
                {
                    var callUserObject = new CallUserObject
                    {
                        UserId = UserData.UserId,
                        Avatar = UserData.Avatar,
                        Name = UserData.Name,
                        Data = new CallUserObject.DataCallUser()
                    };
                    intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));
                }

                StartActivity(intentVideoCall);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMenuVideoCallIcon_Click()
        {
            try
            {
                bool granted = ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) == Permission.Granted && ContextCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.RecordAudio) == Permission.Granted;
                if (granted)
                {
                    StartVideoCall();
                }
                else
                {
                    RequestPermissions(new[] {
                        Manifest.Permission.Camera,
                        Manifest.Permission.RecordAudio,
                        Manifest.Permission.ModifyAudioSettings
                    }, 1107);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //view Profile action!
        private void OnMenuViewProfile_Click()
        {
            try
            {
                if (UserData != null)
                {
                    WoWonderTools.OpenProfile(this, UserId, UserData, "Chat");
                }
                else if (DataUser != null)
                {
                    WoWonderTools.OpenProfile(this, UserId, DataUser.UserData, "Chat");
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void OnMenuBlock_Click()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.BlockUserAsync(UserId, true); //true >> "block" 
                    if (apiStatus == 200)
                    {
                        Methods.DisplayReportResultTrack(respond);

                        var dbDatabase = new SqLiteDatabase();
                        dbDatabase.Insert_Or_Replace_OR_Delete_UsersContact(UserData, "Delete");


                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Blocked_successfully), ToastLength.Short);
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMenuClearChat_Click()
        {
            try
            {
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                dialog.Title(GetText(Resource.String.Lbl_Clear_chat));
                dialog.Content(GetText(Resource.String.Lbl_AreYouSureDeleteMessages));
                dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                {
                    try
                    {
                        MAdapter.DifferList.Clear();
                        MAdapter.NotifyDataSetChanged();

                        var userToDelete = GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == UserId);
                        if (userToDelete != null)
                        {
                            var index = GlobalContext.LastChatTab.MAdapter.LastChatsList.IndexOf(userToDelete);
                            if (index != -1)
                            {
                                GlobalContext?.LastChatTab.MAdapter.LastChatsList.Remove(userToDelete);
                                GlobalContext?.LastChatTab.MAdapter.NotifyItemRemoved(index);
                            }
                        }

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, UserId);

                        if (Methods.CheckConnectivity())
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.DeleteConversationAsync(UserId) });
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(new MyMaterialDialog());
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMenuStartedMessages_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(StartedMessagesActivity));
                intent.PutExtra("ChatId", ChatId);
                intent.PutExtra("UserId", UserId);
                intent.PutExtra("MainChatColor", MainChatColor);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMenuMedia_Click()
        {
            try
            {
                var intent = new Intent(this, typeof(SharedFilesActivity));
                intent.PutExtra("UserId", UserId);
                StartActivity(intent);
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
                //Audio FrameWork initialize 
                RecorderService = new Methods.AudioRecorderAndPlayer(UserId);

                Interpolation = new FastOutSlowInInterpolator();

                ChatColorBoxFragment = new ChatColorsFragment();
                Bundle args = new Bundle();
                args.PutString("userid", UserId);
                ChatColorBoxFragment.Arguments = args;

                ChatRecordSoundBoxFragment = new ChatRecordSoundFragment();

                //Say Hi 
                SayHiLayout = FindViewById<RelativeLayout>(Resource.Id.SayHiLayout);
                SayHiSuggestionsRecycler = FindViewById<RecyclerView>(Resource.Id.recylerHiSuggetions);
                SayHiToTextView = FindViewById<TextView>(Resource.Id.toUserText);
                //User Info 
                ActionBarTitle = FindViewById<TextView>(Resource.Id.Txt_Username);
                ActionBarSubTitle = FindViewById<TextView>(Resource.Id.Txt_last_time);
                //ActionBarButtons
                BackButton = FindViewById<ImageView>(Resource.Id.BackButton);
                AudioCallButton = FindViewById<ImageView>(Resource.Id.IconCall);
                VideoCallButton = FindViewById<ImageView>(Resource.Id.IconvideoCall);
                MoreButton = FindViewById<ImageView>(Resource.Id.IconMore);
                UserChatProfile = FindViewById<CircleImageView>(Resource.Id.userProfileImage);
                RootView = FindViewById<LinearLayout>(Resource.Id.rootChatWindowView);
                ChatEmojImage = FindViewById<ImageView>(Resource.Id.emojiicon);
                LayoutEditText = FindViewById<LinearLayout>(Resource.Id.LayoutEditText);
                EmojIconEditTextView = FindViewById<AXEmojiEditText>(Resource.Id.EmojiconEditText5);
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);
                //ChatColorButton = FindViewById<ImageView>(Resource.Id.colorButton);
                //ChatStickerButton = FindViewById<ImageView>(Resource.Id.stickerButton);
                ChatMediaButton = FindViewById<ImageView>(Resource.Id.mediaButton);
                ButtonFragmentHolder = FindViewById<FrameLayout>(Resource.Id.ButtomFragmentHolder);
                TopFragmentHolder = FindViewById<FrameLayout>(Resource.Id.TopFragmentHolder);
                FirstLiner = FindViewById<LinearLayout>(Resource.Id.firstLiner);
                FirstBoxOnButton = FindViewById<LinearLayout>(Resource.Id.firstBoxonButtom);

                RecordView = FindViewById<RecordView>(Resource.Id.record_view);
                RecordButton = FindViewById<RecordButton>(Resource.Id.record_button);

                RecordButton.SetRecordView(RecordView);
                RecordButton.SetOnRecordClickListener(this); //Send Text Messeages

                //Cancel Bounds is when the Slide To Cancel text gets before the timer . default is 8
                RecordView.SetCancelBounds(8);
                RecordView.SetSmallMicColor(Color.ParseColor("#c2185b"));

                //prevent recording under one Second
                RecordView.SetLessThanSecondAllowed(false);
                RecordView.SetSlideToCancelText(GetText(Resource.String.Lbl_SlideToCancelAudio));
                RecordView.SetCustomSounds(Resource.Raw.record_start, Resource.Raw.record_finished, Resource.Raw.record_error);

                RecordView.SetOnRecordListener(this);

                FabScrollDown = FindViewById<FloatingActionButton>(Resource.Id.fab_scroll);
                FabScrollDown.Visibility = ViewStates.Gone;

                PinMessageView = FindViewById<LinearLayout>(Resource.Id.pin_message_view);
                ShortPinMessage = FindViewById<TextView>(Resource.Id.short_pin_message);

                LoadingLayout = FindViewById<LinearLayout>(Resource.Id.Loading_LinearLayout);
                LoadingLayout.Visibility = ViewStates.Gone;

                RepliedMessageView = FindViewById<LinearLayout>(Resource.Id.replied_message_view);
                TxtOwnerName = FindViewById<TextView>(Resource.Id.owner_name);
                TxtMessageType = FindViewById<TextView>(Resource.Id.message_type);
                TxtShortMessage = FindViewById<TextView>(Resource.Id.short_message);
                MessageFileThumbnail = FindViewById<ImageView>(Resource.Id.message_file_thumbnail);
                BtnCloseReply = FindViewById<ImageView>(Resource.Id.clear_btn_reply_view);
                BtnCloseReply.Visibility = ViewStates.Visible;
                MessageFileThumbnail.Visibility = ViewStates.Gone;

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null);

                SupportFragmentManager.BeginTransaction().Add(ButtonFragmentHolder.Id, ChatColorBoxFragment, "ChatColorBoxFragment");
                SupportFragmentManager.BeginTransaction().Add(TopFragmentHolder.Id, ChatRecordSoundBoxFragment, "Chat_Recourd_Sound_Fragment");

                if (ShowEmpty == "no")
                {
                    SayHiLayout.Visibility = ViewStates.Gone;
                    SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                }

                if (AppSettings.EnableAudioVideoCall)
                {
                    var dataSettings = ListUtils.SettingsSiteList;
                    if (dataSettings?.WhoCall == "pro") //just pro user can chat 
                    {
                        var dataUser = ListUtils.MyProfileList?.FirstOrDefault()?.IsPro;
                        if (dataUser == "0") // Not Pro remove call
                        {
                            AudioCallButton.Visibility = ViewStates.Gone;
                            VideoCallButton.Visibility = ViewStates.Gone;
                        }
                    }
                    else //all users can chat
                    {
                        if (dataSettings?.VideoChat == "0" || !AppSettings.EnableVideoCall)
                        {
                            VideoCallButton.Visibility = ViewStates.Gone;
                        }

                        if (dataSettings?.AudioChat == "0" || !AppSettings.EnableAudioCall)
                        {
                            AudioCallButton.Visibility = ViewStates.Gone;
                        }
                    }
                }
                else
                {
                    AudioCallButton.Visibility = ViewStates.Gone;
                    VideoCallButton.Visibility = ViewStates.Gone;
                }

                if (AppSettings.SetTabDarkTheme)
                {
                    TopFragmentHolder.SetBackgroundColor(Color.ParseColor("#282828"));
                    FirstLiner.SetBackgroundColor(Color.ParseColor("#282828"));
                    FirstBoxOnButton.SetBackgroundColor(Color.ParseColor("#282828"));
                }
                else
                {
                    TopFragmentHolder.SetBackgroundColor(Color.White);
                    FirstLiner.SetBackgroundColor(Color.White);
                    FirstBoxOnButton.SetBackgroundColor(Color.White);
                }

                if (AppSettings.ShowButtonRecordSound)
                {
                    //ChatSendButton.LongClickable = true;
                    RecordButton.Tag = "Free";
                    RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                    RecordButton.SetListenForRecord(true);
                }
                else
                {
                    RecordButton.Tag = "Text";
                    RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                    RecordButton.SetListenForRecord(false);
                }

                RecordButton.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(MainChatColor));
                FabScrollDown.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(MainChatColor));
                FabScrollDown.SetRippleColor(ColorStateList.ValueOf(Color.ParseColor(MainChatColor)));

                //if (AppSettings.ShowButtonStickers)
                //{
                //    ChatStickerButton.Visibility = ViewStates.Visible;
                //    ChatStickerButton.Tag = "Closed";
                //}
                //else
                //{
                //    ChatStickerButton.Visibility = ViewStates.Gone;
                //}

                if (AppSettings.ShowButtonColor)
                {
                    //ChatColorButton.Visibility = ViewStates.Visible;
                    ChatColorButtonTag = "Closed";
                }
                else
                {
                    //ChatColorButton.Visibility = ViewStates.Gone;
                }

                Methods.SetColorEditText(EmojIconEditTextView, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                ChatEmojImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#888888"));

                if (AppSettings.SetTabDarkTheme)
                    EmojisViewTools.LoadDarkTheme();
                else
                    EmojisViewTools.LoadTheme(MainChatColor);

                EmojisViewTools.MStickerView = AppSettings.ShowButtonStickers;
                EmojisViewTools.LoadView(this, EmojIconEditTextView, "ChatWindowActivity", ChatEmojImage);
                EmojIconEditTextView.AddTextChangedListener(new MyTextWatcher(this));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new MessageAdapter(this, UserId, false) { DifferList = new ObservableCollection<AdapterModelsClassMessage>() };

                LayoutManager = new Holders.MsgPreCachingLayoutManager(this) { Orientation = LinearLayoutManager.Vertical };
                LayoutManager.SetPreloadItemCount(35);
                LayoutManager.AutoMeasureEnabled = false;
                LayoutManager.SetExtraLayoutSpace(2000);
                LayoutManager.StackFromEnd = true;

                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(20);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                ((SimpleItemAnimator)MRecycler.GetItemAnimator()).SupportsChangeAnimations = false;

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<AdapterModelsClassMessage>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                XamarinRecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new XamarinRecyclerViewOnScrollListener(LayoutManager, FabScrollDown, null);
                xamarinRecyclerViewOnScrollListener.LoadMoreEvent += OnScrollLoadMoreFromTop_Event;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);

                RecyclerHiSuggestion = FindViewById<RecyclerView>(Resource.Id.recylerHiSuggetions);
                SuggestionAdapter = new EmptySuggestionMessagesAdapter(this);
                RecyclerHiSuggestion.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                RecyclerHiSuggestion.SetAdapter(SuggestionAdapter);

                if (AppSettings.EnableReplyMessageSystem)
                {
                    SwipeReply swipeReplyController = new SwipeReply(this, this);
                    ItemTouchHelper itemTouchHelper = new ItemTouchHelper(swipeReplyController);
                    itemTouchHelper.AttachToRecyclerView(MRecycler);
                }

                if (AppSettings.ShowSettingsWallpaper)
                    GetWallpaper();
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
                    ChatMediaButton.Click += ChatMediaButtonOnClick;
                    //ChatStickerButton.Click += ChatStickerButtonOnClick; 
                    //ChatColorButton.Click += ChatColorButtonOnClick;
                    //ActionBar Buttons
                    BackButton.Click += BackButton_Click;
                    AudioCallButton.Click += AudioCallButton_Click;
                    VideoCallButton.Click += VideoCallButton_Click;
                    MoreButton.Click += MoreButton_Click;
                    UserChatProfile.Click += UserChatProfile_Click;
                    ActionBarTitle.Click += UserChatProfile_Click;
                    ActionBarSubTitle.Click += UserChatProfile_Click;
                    SuggestionAdapter.OnItemClick += SuggestionAdapterOnItemClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    MAdapter.ErrorLoadingItemClick += MAdapterOnErrorLoadingItemClick;
                    MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                    MAdapter.DownloadItemClick += MAdapterOnDownloadItemClick;
                    FabScrollDown.Click += FabScrollDownOnClick;
                    BtnCloseReply.Click += BtnCloseReplyOnClick;
                    PinMessageView.Click += PinMessageViewOnClick;
                }
                else
                {
                    ChatMediaButton.Click -= ChatMediaButtonOnClick;
                    //ChatStickerButton.Click -= ChatStickerButtonOnClick; 
                    //ChatColorButton.Click -= ChatColorButtonOnClick;
                    //ActionBar Buttons
                    BackButton.Click -= BackButton_Click;
                    AudioCallButton.Click -= AudioCallButton_Click;
                    VideoCallButton.Click -= VideoCallButton_Click;
                    MoreButton.Click -= MoreButton_Click;
                    UserChatProfile.Click -= UserChatProfile_Click;
                    ActionBarTitle.Click -= UserChatProfile_Click;
                    ActionBarSubTitle.Click -= UserChatProfile_Click;
                    SuggestionAdapter.OnItemClick -= SuggestionAdapterOnItemClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    MAdapter.ErrorLoadingItemClick -= MAdapterOnErrorLoadingItemClick;
                    MAdapter.ItemLongClick -= MAdapterOnItemLongClick;
                    MAdapter.DownloadItemClick -= MAdapterOnDownloadItemClick;
                    FabScrollDown.Click -= FabScrollDownOnClick;
                    BtnCloseReply.Click -= BtnCloseReplyOnClick;
                    PinMessageView.Click -= PinMessageViewOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ChatWindowActivity GetInstance()
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

        private void PinMessageViewOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(PinnedMessageActivity));
                intent.PutExtra("ChatId", ChatId);
                intent.PutExtra("UserId", UserId);
                intent.PutExtra("MainChatColor", MainChatColor);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnCloseReplyOnClick(object sender, EventArgs e)
        {
            try
            {
                CloseReplyUi();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FabScrollDownOnClick(object sender, EventArgs e)
        {
            try
            {
                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                FabScrollDown.Visibility = ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SuggestionAdapterOnItemClick(object sender, AdapterClickEvents e)
        {
            try
            {
                var position = e.Position;
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);
                MessageData m1 = new MessageData
                {
                    Id = time2,
                    FromId = UserDetails.UserId,
                    ToId = UserId,
                    Text = SuggestionAdapter.GetItem(position).RealMessage,
                    Position = "right",
                    Seen = "-1",
                    Time = time2,
                    ModelType = MessageModelType.RightText,
                    TimeText = DateTime.Now.ToShortTimeString(),
                    SendFile = true,
                    ChatColor = MainChatColor,
                    MessageHashId = time2
                };

                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                {
                    TypeView = MessageModelType.RightText,
                    Id = Long.ParseLong(m1.Id),
                    MesData = m1
                });

                var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                MAdapter.NotifyItemInserted(indexMes);

                //Scroll Down >> 
                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                if (Methods.CheckConnectivity())
                {
                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                    {
                        UserDetails.Socket?.EmitAsync_SendMessage(UserId, UserDetails.AccessToken, UserDetails.Username, SuggestionAdapter.GetItem(position).RealMessage, MainChatColor, "", time2);
                    }
                    else
                    {
                        Task.Factory.StartNew(() =>
                        {
                            MessageController.SendMessageTask(this, UserId, ChatId, time2, SuggestionAdapter.GetItem(position).RealMessage).ConfigureAwait(false);
                        });
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }

                SayHiLayout.Visibility = ViewStates.Gone;
                SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;

                CloseReplyUi();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void MAdapterOnErrorLoadingItemClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (e.Position <= -1) return;
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        item.MesData.ErrorSendMessage = false;
                        Update_One_Messages(item.MesData);

                        await Task.Factory.StartNew(() =>
                        {
                            MessageController.SendMessageTask(this, UserId, ChatId, item.MesData.Id, "", "", item.MesData.Media, "", "", "", "", "", "", "", item.MesData.ReplyId).ConfigureAwait(false);
                        });
                    }
                }
                else
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void MAdapterOnDownloadItemClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (e.Position <= -1) return;
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        item.MesData.BtnDownload = true;

                        var fileName = item.MesData.Media.Split('/').Last();
                        switch (e.Type)
                        {
                            case Holders.TypeClick.Sound:
                                {
                                    item.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimSound, fileName, item.MesData.Media, "audio", true);
                                    break;
                                }
                            case Holders.TypeClick.Video:
                                {
                                    item.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimVideo, fileName, item.MesData.Media, "video", true);
                                    break;
                                }
                            case Holders.TypeClick.Image:
                                {
                                    item.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimImage, fileName, item.MesData.Media, "image", true);
                                    break;
                                }
                        }

                        await Task.Delay(1000);

                        Update_One_Messages(item.MesData, true, false);
                    }
                }
                else
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    switch (e.Type)
                    {
                        case Holders.TypeClick.Text:
                        case Holders.TypeClick.Contact:
                            item.MesData.ShowTimeText = !item.MesData.ShowTimeText;
                            MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(item));
                            break;
                        case Holders.TypeClick.File:
                            {
                                var fileName = item.MesData.Media.Split('/').Last();
                                string imageFile = Methods.MultiMedia.CheckFileIfExits(item.MesData.Media);
                                if (imageFile != "File Dont Exists")
                                {
                                    try
                                    {
                                        var extension = fileName.Split('.').Last();
                                        string mimeType = MimeTypeMap.GetMimeType(extension);

                                        Intent openFile = new Intent();
                                        openFile.SetFlags(ActivityFlags.NewTask);
                                        openFile.SetFlags(ActivityFlags.GrantReadUriPermission);
                                        openFile.SetAction(Intent.ActionView);
                                        openFile.SetDataAndType(Uri.Parse(imageFile), mimeType);
                                        StartActivity(openFile);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                }
                                else
                                {
                                    var extension = fileName.Split('.').Last();
                                    string mimeType = MimeTypeMap.GetMimeType(extension);

                                    Intent i = new Intent(Intent.ActionView);
                                    i.SetData(Uri.Parse(item.MesData.Media));
                                    i.SetType(mimeType);
                                    StartActivity(i);
                                    // ToastUtils.ShowToast(MainActivity, MainActivity.GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                                }

                                break;
                            }
                        case Holders.TypeClick.Video:
                            {
                                var fileName = item.MesData.Media.Split('/').Last();
                                var mediaFile = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimVideo, fileName, item.MesData.Media, "video");

                                string imageFile = Methods.MultiMedia.CheckFileIfExits(mediaFile);
                                if (imageFile != "File Dont Exists")
                                {
                                    File file2 = new File(mediaFile);
                                    var mediaUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                    if (AppSettings.OpenVideoFromApp)
                                    {
                                        Intent intent = new Intent(this, typeof(VideoFullScreenActivity));
                                        intent.PutExtra("videoUrl", mediaUri.ToString());
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Intent intent = new Intent();
                                        intent.SetAction(Intent.ActionView);
                                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                                        intent.SetDataAndType(mediaUri, "video/*");
                                        StartActivity(intent);
                                    }
                                }
                                else
                                {
                                    if (AppSettings.OpenVideoFromApp)
                                    {
                                        Intent intent = new Intent(this, typeof(VideoFullScreenActivity));
                                        intent.PutExtra("videoUrl", item.MesData.Media);
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Intent intent = new Intent(Intent.ActionView, Uri.Parse(item.MesData.Media));
                                        StartActivity(intent);
                                    }
                                }

                                break;
                            }
                        case Holders.TypeClick.Image:
                            {
                                if (AppSettings.OpenImageFromApp)
                                {
                                    Intent intent = new Intent(this, typeof(ImageViewerActivity));
                                    intent.PutExtra("Id", UserId);
                                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(item.MesData));
                                    StartActivity(intent);
                                }
                                else
                                {
                                    var fileName = item.MesData.Media.Split('/').Last();
                                    var mediaFile = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimImage, fileName, item.MesData.Media, "image");

                                    string imageFile = Methods.MultiMedia.CheckFileIfExits(mediaFile);
                                    if (imageFile != "File Dont Exists")
                                    {
                                        File file2 = new File(mediaFile);
                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                                        Intent intent = new Intent();
                                        intent.SetAction(Intent.ActionView);
                                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                                        intent.SetDataAndType(photoUri, "image/*");
                                        StartActivity(intent);
                                    }
                                    else
                                    {
                                        Intent intent = new Intent(Intent.ActionView, Uri.Parse(mediaFile));
                                        StartActivity(intent);
                                    }
                                }

                                break;
                            }
                        case Holders.TypeClick.Map:
                            {
                                // Create a Uri from an intent string. Use the result to create an Intent. 
                                var uri = Uri.Parse("geo:" + item.MesData.Lat + "," + item.MesData.Lng);
                                var intent = new Intent(Intent.ActionView, uri);
                                intent.SetPackage("com.google.android.apps.maps");
                                intent.AddFlags(ActivityFlags.NewTask);
                                StartActivity(intent);
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

        private void MAdapterOnItemLongClick(object sender, Holders.MesClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    SelectedItemPositions = MAdapter.GetItem(e.Position);
                    if (SelectedItemPositions != null)
                    {
                        OptionsChatWindowBottomSheet bottomSheet = new OptionsChatWindowBottomSheet();
                        Bundle bundle = new Bundle();
                        bundle.PutString("Type", JsonConvert.SerializeObject(e.Type));
                        bundle.PutString("Page", "ChatWindow");
                        bundle.PutString("ItemObject", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                        bottomSheet.Arguments = bundle;
                        bottomSheet.Show(SupportFragmentManager, bottomSheet.Tag);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UserChatProfile_Click(object sender, EventArgs e)
        {
            OnMenuViewProfile_Click();
        }

        private void MoreButton_Click(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_View_Profile));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Block));

                if (AppSettings.ShowButtonColor)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_ChangeChatTheme));

                if (AppSettings.ShowSettingsWallpaper)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Wallpaper));

                arrayAdapter.Add(GetText(Resource.String.Lbl_Clear_chat));

                if (AppSettings.EnableFavoriteMessageSystem && StartedMessageList?.Count > 0)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_StartedMessages));

                arrayAdapter.Add(GetText(Resource.String.Lbl_Media));

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

        private void VideoCallButton_Click(object sender, EventArgs e)
        {
            try
            {
                OnMenuVideoCallIcon_Click();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AudioCallButton_Click(object sender, EventArgs e)
        {
            try
            {
                OnMenuPhoneCallIcon_Click();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void ChatColorButtonOnClick()
        {
            try
            {
                if (ChatColorButtonTag == "Closed")
                {
                    ResetButtonTags();
                    ChatColorButtonTag = "Opened";
                    //ChatColorButton?.Drawable?.SetTint(Color.ParseColor(AppSettings.MainColor));
                    ReplaceButtonFragment(ChatColorBoxFragment);
                }
                else
                {
                    ResetButtonTags();
                    //ChatColorButton?.Drawable?.SetTint(Color.ParseColor("#888888"));
                    TopFragmentHolder?.Animate()?.SetInterpolator(Interpolation)?.TranslationY(1200)?.SetDuration(300);
                    SupportFragmentManager.BeginTransaction().Remove(ChatColorBoxFragment)?.Commit();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void EmojIconEditTextViewOnTextChanged(/*object sender, TextChangedEventArgs e*/)
        {
            try
            {
                if (AppSettings.ShowButtonRecordSound)
                {
                    if (!ButtonFragmentHolder.TranslationY.Equals(1200))
                        ButtonFragmentHolder.TranslationY = 1200;

                    if (IsRecording && EmojIconEditTextView.Text == GetString(Resource.String.Lbl_Recording))
                    {
                        RecordButton.Tag = "Text";
                        RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                        RecordButton.SetListenForRecord(false);

                        EditTextOpen();
                    }
                    else if (!string.IsNullOrEmpty(EmojIconEditTextView.Text))
                    {
                        RecordButton.Tag = "Text";
                        RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                        RecordButton.SetListenForRecord(false);

                        EditTextOpen();
                    }
                    else if (IsRecording)
                    {
                        RecordButton.Tag = "Text";
                        RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                        RecordButton.SetListenForRecord(false);

                        EditTextOpen();
                    }
                    else
                    {
                        RecordButton.Tag = "Free";
                        RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                        RecordButton.SetListenForRecord(true);

                        EditTextClose();

                        if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        {
                            UserDetails.Socket?.EmitAsync_StoppedEvent(UserId, UserDetails.AccessToken);
                        }
                        else
                        {
                            RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "stopped").ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    RecordButton.Tag = "Text";
                    RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                    RecordButton.SetListenForRecord(false);

                    EditTextOpen();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EditTextClose()
        {
            try
            {
                ChatMediaButton.SetImageResource(Resource.Drawable.icon_attach_vector);
                ChatMediaButton.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#444444"));
                ChatMediaButton.Tag = "attachment";
                ViewGroup.LayoutParams layoutParams = ChatMediaButton.LayoutParameters;
                layoutParams.Width = 62;
                layoutParams.Height = 62;
                ChatMediaButton.LayoutParameters = layoutParams;
                //ChatStickerButton.Visibility = ViewStates.Visible;
                //ChatColorButton.Visibility = ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EditTextOpen()
        {
            try
            {
                ChatMediaButton.SetImageResource(Resource.Drawable.ic_next);
                ChatMediaButton.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                ChatMediaButton.Tag = "arrow";
                //ChatStickerButton.Visibility = ViewStates.Gone;
                //ChatColorButton.Visibility = ViewStates.Gone;
                ViewGroup.LayoutParams layoutParams = ChatMediaButton.LayoutParameters;
                layoutParams.Width = 42;
                layoutParams.Height = 42;
                ChatMediaButton.LayoutParameters = layoutParams;

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    UserDetails.Socket?.EmitAsync_TypingEvent(UserId, UserDetails.AccessToken);
                }
                else
                {
                    RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "typing").ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Show Load More Event when scroll to the top Of Recycle
        private async void OnScrollLoadMoreFromTop_Event(object sender, EventArgs e)
        {
            try
            {
                //Start Loader Get from Database or API Request >>
                //SwipeRefreshLayout.Refreshing = true;
                //SwipeRefreshLayout.Enabled = true;

                await Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        if (RunLoadMore)
                            return;

                        //Code get first Message id where LoadMore >>
                        var local = await LoadMore_Messages_Database();
                        if (local != "1")
                            await LoadMoreMessages_API();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });

                //SwipeRefreshLayout.Refreshing = false;
                //SwipeRefreshLayout.Enabled = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Run Timer
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                RunOnUiThread(MessageUpdater);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Intent Contact (result is 506 , Permissions is 101 )
        private void ChatContactButtonOnClick()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //request code of result is 506
                    new IntentController(this).OpenIntentGetContactNumberPhone();
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (CheckSelfPermission(Manifest.Permission.ReadContacts) == Permission.Granted)
                    {
                        //request code of result is 506
                        new IntentController(this).OpenIntentGetContactNumberPhone();
                    }
                    else
                    {
                        //101 >> ReadContacts && ReadPhoneNumbers
                        new PermissionsController(this).RequestPermission(101);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Intent Location (result is 506 , Permissions is 101 )
        private void ChatLocationButtonOnClick()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //Open intent Location when the request code of result is 502
                    new IntentController(this).OpenIntentLocation();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(105);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Event sent media (image , Camera , video , file , music )
        private void ChatMediaButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (ChatMediaButton?.Tag?.ToString() == "attachment")
                {
                    AttachmentMediaChatWindowBottomSheet bottomSheet = new AttachmentMediaChatWindowBottomSheet();
                    Bundle bundle = new Bundle();
                    bundle.PutString("Page", "ChatWindow");
                    bottomSheet.Arguments = bundle;
                    bottomSheet.Show(SupportFragmentManager, bottomSheet.Tag);
                }
                else if (ChatMediaButton?.Tag?.ToString() == "arrow")
                {
                    EditTextClose();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Send Message type => "right_audio" Or "right_text"
        public async void OnClick_OfSendButton()
        {
            try
            {
                IsRecording = false;

                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);

                if (RecordButton?.Tag?.ToString() == "Audio")
                {
                    var interTortola = new FastOutSlowInInterpolator();
                    TopFragmentHolder.Animate().SetInterpolator(interTortola).TranslationY(1200).SetDuration(300);
                    SupportFragmentManager.BeginTransaction().Remove(ChatRecordSoundBoxFragment)?.Commit();

                    string filePath = RecorderService.GetRecorded_Sound_Path();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            ToId = UserId,
                            Media = filePath,
                            Seen = "-1",
                            Time = time2,
                            Position = "right",
                            TimeText = GetText(Resource.String.Lbl_Uploading),
                            MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(Methods.AudioRecorderAndPlayer.Get_MediaFileDuration(filePath)),
                            ModelType = MessageModelType.RightAudio,
                            SendFile = true,
                            ChatColor = MainChatColor,
                            MessageHashId = time2
                        };

                        MAdapter.DifferList.Add(new AdapterModelsClassMessage
                        {
                            TypeView = MessageModelType.RightAudio,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        //Here on This function will send Selected audio file to the user 
                        if (Methods.CheckConnectivity())
                        {
                            await Task.Factory.StartNew(() =>
                            {
                                MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", filePath, "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        }
                    }

                    RecordButton.Tag = "Free";
                    RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                    RecordButton.SetListenForRecord(true);

                }
                else if (RecordButton?.Tag?.ToString() == "Text")
                {
                    if (string.IsNullOrEmpty(EmojIconEditTextView.Text))
                    {

                    }
                    else
                    {
                        //Hide SayHi And Suggestion
                        SayHiLayout.Visibility = ViewStates.Gone;
                        SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                        //Here on This function will send Text Messages to the user 

                        //remove \n in a string
                        string replacement = Regex.Replace(EmojIconEditTextView.Text, @"\t|\n|\r", "");

                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            ToId = UserId,
                            Text = replacement,
                            Position = "right",
                            Seen = "-1",
                            Time = time2,
                            ModelType = MessageModelType.RightText,
                            TimeText = DateTime.Now.ToShortTimeString(),
                            SendFile = true,
                            ChatColor = MainChatColor,
                            MessageHashId = time2
                        };

                        if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                        {
                            m1.ReplyId = ReplyId;
                            m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                            {
                                ReplyClass = SelectedItemPositions.MesData
                            };
                        }

                        if (AppSettings.EnableFitchOgLink)
                        {
                            //Check if find website in text 
                            foreach (Match item in Regex.Matches(replacement, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                            {
                                Console.WriteLine(item.Value);
                                m1.FitchOgLink = await Methods.OgLink.FitchOgLink(item.Value);
                                break;
                            }
                        }

                        MAdapter.DifferList.Add(new AdapterModelsClassMessage
                        {
                            TypeView = MessageModelType.RightText,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        if (Methods.CheckConnectivity())
                        {
                            if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            {
                                UserDetails.Socket?.EmitAsync_SendMessage(UserId, UserDetails.AccessToken, UserDetails.Username, EmojIconEditTextView.Text, MainChatColor, ReplyId, time2);
                            }
                            else
                            {
                                await Task.Factory.StartNew(() =>
                                {
                                    MessageController.SendMessageTask(this, UserId, ChatId, time2, EmojIconEditTextView.Text, "", "", "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                                });
                            }
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        }

                        EmojIconEditTextView.Text = "";
                    }

                    if (AppSettings.ShowButtonRecordSound)
                    {
                        RecordButton.Tag = "Free";
                        RecordButton.SetTheImageResource(Resource.Drawable.icon_mic_vector);
                        RecordButton.SetListenForRecord(true);
                    }
                    else
                    {
                        RecordButton.Tag = "Text";
                        RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                        RecordButton.SetListenForRecord(false);
                    }
                }

                CloseReplyUi();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                string timeNow = DateTime.Now.ToShortTimeString();
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = Convert.ToString(unixTimestamp);

                if (requestCode == 506 && resultCode == Result.Ok) // right_contact
                {
                    var contact = Methods.PhoneContactManager.Get_ContactInfoBy_Id(data.Data.LastPathSegment);
                    if (contact != null)
                    {
                        var name = contact.UserDisplayName;
                        var phone = contact.PhoneNumber;

                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            ToId = UserId,
                            ContactName = name,
                            ContactNumber = phone,
                            TimeText = timeNow,
                            Position = "right",
                            Seen = "-1",
                            Time = time2,
                            ModelType = MessageModelType.RightContact,
                            SendFile = true,
                            ChatColor = MainChatColor,
                            MessageHashId = time2
                        };

                        if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                        {
                            m1.ReplyId = ReplyId;
                            m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                            {
                                ReplyClass = SelectedItemPositions.MesData
                            };
                        }

                        MAdapter.DifferList.Add(new AdapterModelsClassMessage
                        {
                            TypeView = MessageModelType.RightContact,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        var dictionary = new Dictionary<string, string>();

                        if (!dictionary.ContainsKey(name))
                        {
                            dictionary.Add(name, phone);
                        }

                        string dataContact = JsonConvert.SerializeObject(dictionary.ToArray().FirstOrDefault(a => a.Key == name));

                        if (Methods.CheckConnectivity())
                        {
                            //Send contact function
                            await Task.Factory.StartNew(() =>
                            {
                                MessageController.SendMessageTask(this, UserId, ChatId, time2, dataContact, "1", "", "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        }
                    }
                }
                else if (requestCode == 500 && resultCode == Result.Ok) // right_image 
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            if (info == "AdultImages")
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);
                            }
                            else
                            {
                                //this file not supported on the server , please select another file 
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                            }
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Image")
                        {
                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                ToId = UserId,
                                Media = filepath,
                                Position = "right",
                                Seen = "-1",
                                Time = time2,
                                ModelType = MessageModelType.RightImage,
                                TimeText = timeNow,
                                SendFile = true,
                                ChatColor = MainChatColor,
                                ErrorSendMessage = false,
                                MessageHashId = time2
                            };
                            if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                            {
                                m1.ReplyId = ReplyId;
                                m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                                {
                                    ReplyClass = SelectedItemPositions.MesData
                                };
                            }

                            //Send image function
                            if (Methods.CheckConnectivity())
                            {
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightImage,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                await MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", filepath, "", "", "", "", "", "", "", ReplyId);

                                //await Task.Factory.StartNew(() =>
                                //{

                                //});
                            }
                            else
                            {
                                m1.ErrorSendMessage = true;
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightImage,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                            }

                            //var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            //MAdapter.NotifyItemInserted(indexMes);
                            MAdapter.NotifyDataSetChanged();

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Please_check_your_details), ToastLength.Long);
                        }
                    }
                }
                else if (requestCode == CropImage.CropImageActivityRequestCode && resultCode == Result.Ok) // right_image 
                {
                    var result = CropImage.GetActivityResult(data);
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.Uri;

                        if (!string.IsNullOrEmpty(resultUri.Path))
                        {
                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                ToId = UserId,
                                Media = resultUri.Path,
                                Position = "right",
                                Seen = "-1",
                                Time = time2,
                                ModelType = MessageModelType.RightImage,
                                TimeText = timeNow,
                                SendFile = true,
                                ChatColor = MainChatColor,
                                ErrorSendMessage = false,
                                MessageHashId = time2
                            };
                            if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                            {
                                m1.ReplyId = ReplyId;
                                m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                                {
                                    ReplyClass = SelectedItemPositions.MesData
                                };
                            }

                            //Send image function
                            if (Methods.CheckConnectivity())
                            {
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightImage,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });
                                await Task.Factory.StartNew(() =>
                                {
                                    MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", resultUri.Path).ConfigureAwait(false);
                                });
                            }
                            else
                            {
                                m1.ErrorSendMessage = true;
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightImage,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                            }

                            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            MAdapter.NotifyItemInserted(indexMes);

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                        }
                    }
                }
                else if (requestCode == 503 && resultCode == Result.Ok) // Add right_image using camera   
                {
                    if (string.IsNullOrEmpty(IntentController.CurrentPhotoPath))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                    }
                    else
                    {
                        //var thumbnail = MediaStore.Images.Media.GetBitmap(ContentResolver, IntentController.ImageCameraUri); 
                        //Bitmap bitmap = BitmapFactory.DecodeFile(IntentController.currentPhotoPath);

                        if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentPhotoPath) != "File Dont Exists")
                        {
                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                ToId = UserId,
                                Media = IntentController.CurrentPhotoPath,
                                Position = "right",
                                Seen = "-1",
                                Time = time2,
                                ModelType = MessageModelType.RightImage,
                                TimeText = timeNow,
                                SendFile = true,
                                ChatColor = MainChatColor,
                                ErrorSendMessage = false,
                                MessageHashId = time2
                            };
                            if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                            {
                                m1.ReplyId = ReplyId;
                                m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                                {
                                    ReplyClass = SelectedItemPositions.MesData
                                };
                            }

                            //Send image function
                            if (Methods.CheckConnectivity())
                            {
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightImage,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                await Task.Factory.StartNew(() =>
                                {
                                    MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", IntentController.CurrentPhotoPath, "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                                });
                            }
                            else
                            {
                                m1.ErrorSendMessage = true;
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightImage,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                            }

                            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            MAdapter.NotifyItemInserted(indexMes);

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        }
                        else
                        {
                            //ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load),ToastLength.Short);
                        }
                    }
                }
                else if (requestCode == 501 && resultCode == Result.Ok) // right_video 
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            if (info == "AdultImages")
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);
                            }
                            else
                            {
                                //this file not supported on the server , please select another file 
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                            }
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDiskVideo + UserId;
                            var fullPathFile = new File(Methods.Path.FolderDiskVideo + UserId, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }

                            var newCopyedFilepath = Methods.MultiMedia.CopyMediaFileTo(filepath, Methods.Path.FolderDcimVideo + UserId, false, true);
                            if (newCopyedFilepath != "Path File Dont exits")
                            {
                                Console.WriteLine(newCopyedFilepath);
                            }

                            if (AppSettings.EnableVideoEditor)
                            {
                                var intent = new Intent(this, typeof(VideoEditorActivity));
                                intent.PutExtra("Uri", filepath);
                                intent.PutExtra("Type", "Messages");
                                StartActivityForResult(intent, 2000);
                            }
                            else
                            {
                                MessageData m1 = new MessageData
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    ToId = UserId,
                                    Media = filepath,
                                    Position = "right",
                                    Seen = "-1",
                                    Time = time2,
                                    ModelType = MessageModelType.RightVideo,
                                    TimeText = timeNow,
                                    SendFile = true,
                                    ChatColor = MainChatColor,
                                    ErrorSendMessage = false,
                                    MessageHashId = time2
                                };
                                if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                                {
                                    m1.ReplyId = ReplyId;
                                    m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                                    {
                                        ReplyClass = SelectedItemPositions.MesData
                                    };
                                }

                                //Send Video function
                                if (Methods.CheckConnectivity())
                                {
                                    MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                    {
                                        TypeView = MessageModelType.RightVideo,
                                        Id = Long.ParseLong(m1.Id),
                                        MesData = m1
                                    });

                                    await Task.Factory.StartNew(() =>
                                    {
                                        MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", filepath, "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                                    });
                                }
                                else
                                {
                                    m1.ErrorSendMessage = true;
                                    MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                    {
                                        TypeView = MessageModelType.RightVideo,
                                        Id = Long.ParseLong(m1.Id),
                                        MesData = m1
                                    });

                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                }

                                var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                                MAdapter.NotifyItemInserted(indexMes);

                                //Scroll Down >> 
                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                            }
                        }
                    }
                }
                else if (requestCode == 513 && resultCode == Result.Ok) // right_video camera 
                {
                    if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentVideoPath) != "File Dont Exists" && Build.VERSION.SdkInt <= BuildVersionCodes.OMr1)
                    {
                        var fileName = IntentController.CurrentVideoPath.Split('/').Last();
                        var fileNameWithoutExtension = fileName.Split('.').First();
                        var path = Methods.Path.FolderDiskVideo + "/" + fileNameWithoutExtension + ".png";

                        var newCopyedFilepath = Methods.MultiMedia.CopyMediaFileTo(IntentController.CurrentVideoPath, Methods.Path.FolderDiskVideo + UserId, false, true);
                        if (newCopyedFilepath != "Path File Dont exits")
                        {
                            Console.WriteLine(newCopyedFilepath);
                        }

                        if (AppSettings.EnableVideoEditor)
                        {
                            var intent = new Intent(this, typeof(VideoEditorActivity));
                            intent.PutExtra("Uri", IntentController.CurrentVideoPath);
                            intent.PutExtra("Type", "Messages");
                            StartActivityForResult(intent, 2000);
                        }
                        else
                        {
                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                ToId = UserId,
                                Media = IntentController.CurrentVideoPath,
                                Position = "right",
                                Seen = "-1",
                                Time = time2,
                                ModelType = MessageModelType.RightVideo,
                                TimeText = timeNow,
                                SendFile = true,
                                ChatColor = MainChatColor,
                                ErrorSendMessage = false,
                                MessageHashId = time2
                            };
                            if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                            {
                                m1.ReplyId = ReplyId;
                                m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                                {
                                    ReplyClass = SelectedItemPositions.MesData
                                };
                            }

                            //Send Video function
                            if (Methods.CheckConnectivity())
                            {
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightVideo,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                await Task.Factory.StartNew(() =>
                                {
                                    MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", IntentController.CurrentVideoPath, "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                                });
                            }
                            else
                            {
                                m1.ErrorSendMessage = true;
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightVideo,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                            }

                            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            MAdapter.NotifyItemInserted(indexMes);

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                        }
                    }
                    else
                    {
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                        if (filepath != null)
                        {
                            var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                            if (type == "Video")
                            {
                                var newCopyedFilepath = Methods.MultiMedia.CopyMediaFileTo(filepath, Methods.Path.FolderDcimVideo + UserId, false, true);
                                if (newCopyedFilepath != "Path File Dont exits")
                                {
                                    Console.WriteLine(newCopyedFilepath);
                                }

                                if (AppSettings.EnableVideoEditor)
                                {
                                    var intent = new Intent(this, typeof(VideoEditorActivity));
                                    intent.PutExtra("Uri", filepath);
                                    intent.PutExtra("Type", "Messages");
                                    StartActivityForResult(intent, 2000);
                                }
                                else
                                {
                                    MessageData m1 = new MessageData
                                    {
                                        Id = time2,
                                        FromId = UserDetails.UserId,
                                        ToId = UserId,
                                        Media = filepath,
                                        Position = "right",
                                        Seen = "-1",
                                        Time = time2,
                                        ModelType = MessageModelType.RightVideo,
                                        TimeText = timeNow,
                                        SendFile = true,
                                        ChatColor = MainChatColor,
                                        ErrorSendMessage = false,
                                        MessageHashId = time2
                                    };
                                    if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                                    {
                                        m1.ReplyId = ReplyId;
                                        m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                                        {
                                            ReplyClass = SelectedItemPositions.MesData
                                        };
                                    }

                                    //Send Video function
                                    if (Methods.CheckConnectivity())
                                    {
                                        MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                        {
                                            TypeView = MessageModelType.RightVideo,
                                            Id = Long.ParseLong(m1.Id),
                                            MesData = m1
                                        });

                                        await Task.Factory.StartNew(() =>
                                        {
                                            MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", filepath, "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                                        });
                                    }
                                    else
                                    {
                                        m1.ErrorSendMessage = true;
                                        MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                        {
                                            TypeView = MessageModelType.RightVideo,
                                            Id = Long.ParseLong(m1.Id),
                                            MesData = m1
                                        });

                                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                                    }

                                    var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                                    MAdapter.NotifyItemInserted(indexMes);

                                    //Scroll Down >> 
                                    MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                                }
                            }
                        }
                    }
                }
                else if (requestCode == 504 && resultCode == Result.Ok) // right_file
                {
                    string filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            if (info == "AdultImages")
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);
                            }
                            else
                            {
                                //this file not supported on the server , please select another file 
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                            }
                            return;
                        }

                        string totalSize = Methods.FunString.Format_byte_size(filepath);
                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            ToId = UserId,
                            Media = filepath,
                            FileSize = totalSize,
                            TimeText = timeNow,
                            Seen = "-1",
                            Time = time2,
                            Position = "right",
                            ModelType = MessageModelType.RightFile,
                            SendFile = true,
                            ChatColor = MainChatColor,
                            MessageHashId = time2
                        };
                        if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                        {
                            m1.ReplyId = ReplyId;
                            m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                            {
                                ReplyClass = SelectedItemPositions.MesData
                            };
                        }
                        MAdapter.DifferList.Add(new AdapterModelsClassMessage
                        {
                            TypeView = MessageModelType.RightFile,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        //Send Video function
                        if (Methods.CheckConnectivity())
                        {
                            await Task.Factory.StartNew(() =>
                            {
                                MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", filepath, "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                        }
                    }
                }
                else if (requestCode == 505 && resultCode == Result.Ok) // right_audio
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            if (info == "AdultImages")
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Error_AdultImages), ToastLength.Short);
                            }
                            else
                            {
                                //this file not supported on the server , please select another file 
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short);
                            }
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Audio")
                        {
                            //wael 
                            //var newCopyedFilepath = Methods.MultiMedia.CopyMediaFileTo(filepath, Methods.Path.FolderDcimSound + "/" + UserId, false, true);
                            //if (newCopyedFilepath != "Path File Dont exits")
                            //{
                            string totalSize = Methods.FunString.Format_byte_size(filepath);
                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                ToId = UserId,
                                Media = filepath,
                                FileSize = totalSize,
                                Seen = "-1",
                                Time = time2,
                                Position = "right",
                                TimeText = GetText(Resource.String.Lbl_Uploading),
                                MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(Methods.AudioRecorderAndPlayer.Get_MediaFileDuration(filepath)),
                                ModelType = MessageModelType.RightAudio,
                                SendFile = true,
                                ChatColor = MainChatColor,
                                MessageHashId = time2
                            };

                            if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                            {
                                m1.ReplyId = ReplyId;
                                m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                                {
                                    ReplyClass = SelectedItemPositions.MesData
                                };
                            }
                            MAdapter.DifferList.Add(new AdapterModelsClassMessage
                            {
                                TypeView = MessageModelType.RightAudio,
                                Id = Long.ParseLong(m1.Id),
                                MesData = m1
                            });

                            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            MAdapter.NotifyItemInserted(indexMes);

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                            //Send Video function
                            if (Methods.CheckConnectivity())
                            {
                                await Task.Factory.StartNew(() =>
                                {
                                    MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", filepath, "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                                });
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                            }
                            //}
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short);
                    }
                }
                else if (requestCode == 300 && resultCode == Result.Ok) // right_gif
                {
                    // G_fixed_height_small_url, // UrlGif - view  >>  mediaFileName
                    // G_fixed_height_small_mp4, //MediaGif - sent >>  media

                    var gifLink = data.GetStringExtra("MediaGif") ?? "Data not available";
                    if (gifLink != "Data not available" && !string.IsNullOrEmpty(gifLink))
                    {
                        var gifUrl = data.GetStringExtra("UrlGif") ?? "Data not available";
                        GifFile = gifLink;

                        MessageData m1 = new MessageData
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            ToId = UserId,
                            Media = GifFile,
                            MediaFileName = gifUrl,
                            Seen = "-1",
                            Time = time2,
                            Position = "right",
                            ModelType = MessageModelType.RightGif,
                            TimeText = timeNow,
                            Stickers = gifUrl,
                            SendFile = true,
                            ChatColor = MainChatColor,
                            MessageHashId = time2
                        };

                        if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                        {
                            m1.ReplyId = ReplyId;
                            m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                            {
                                ReplyClass = SelectedItemPositions.MesData
                            };
                        }
                        MAdapter.DifferList.Add(new AdapterModelsClassMessage
                        {
                            TypeView = MessageModelType.RightGif,
                            Id = Long.ParseLong(m1.Id),
                            MesData = m1
                        });

                        var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                        MAdapter.NotifyItemInserted(indexMes);

                        //Scroll Down >> 
                        MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                        //Send image function
                        if (Methods.CheckConnectivity())
                        {
                            await Task.Factory.StartNew(() =>
                            {
                                MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", "", "", "", gifUrl, "", "", "", "", ReplyId).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Please_check_your_details) + " ", ToastLength.Long);
                    }
                }
                else if (requestCode == 502 && resultCode == Result.Ok) // Location
                {
                    //var placeAddress = data.GetStringExtra("Address") ?? "";
                    var placeLatLng = data.GetStringExtra("latLng") ?? "";
                    if (!string.IsNullOrEmpty(placeLatLng))
                    {
                        string[] latLng = placeLatLng.Split(',');
                        if (latLng?.Length > 0)
                        {
                            string lat = latLng[0];
                            string lng = latLng[1];

                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                ToId = UserId,
                                Lat = lat,
                                Lng = lng,
                                Position = "right",
                                Seen = "-1",
                                Time = time2,
                                ModelType = MessageModelType.RightMap,
                                TimeText = timeNow,
                                SendFile = true,
                                ChatColor = MainChatColor,
                                MessageHashId = time2
                            };
                            if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                            {
                                m1.ReplyId = ReplyId;
                                m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                                {
                                    ReplyClass = SelectedItemPositions.MesData
                                };
                            }
                            MAdapter.DifferList.Add(new AdapterModelsClassMessage
                            {
                                TypeView = MessageModelType.RightMap,
                                Id = Long.ParseLong(m1.Id),
                                MesData = m1
                            });
                            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            MAdapter.NotifyItemInserted(indexMes);

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                            //Send Map function
                            if (Methods.CheckConnectivity())
                            {
                                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                                {
                                    UserDetails.Socket?.EmitAsync_SendMessage(UserId, UserDetails.AccessToken, UserDetails.Username, EmojIconEditTextView.Text, MainChatColor, ReplyId, time2, "", lat, lng);
                                }
                                else
                                {
                                    await Task.Factory.StartNew(() =>
                                    {
                                        MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", "", "", "", "", "", lat, lng, "", ReplyId).ConfigureAwait(false);
                                    });
                                }
                            }
                            else
                            {
                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                            }
                        }
                    }
                }
                else if (requestCode == 2000 && resultCode == Result.Ok)
                {
                    var videoPath = data.GetStringExtra("VideoPath") ?? "";
                    if (!string.IsNullOrEmpty(videoPath))
                    {
                        try
                        {
                            MessageData m1 = new MessageData
                            {
                                Id = time2,
                                FromId = UserDetails.UserId,
                                ToId = UserId,
                                Media = videoPath,
                                Position = "right",
                                Seen = "-1",
                                Time = time2,
                                ModelType = MessageModelType.RightVideo,
                                TimeText = timeNow,
                                SendFile = true,
                                ChatColor = MainChatColor,
                                ErrorSendMessage = false,
                                MessageHashId = time2
                            };
                            if (SelectedItemPositions?.MesData != null && !string.IsNullOrEmpty(ReplyId) && ReplyId != "0")
                            {
                                m1.ReplyId = ReplyId;
                                m1.Reply = new WoWonderClient.Classes.Message.MessageData.ReplyUnion
                                {
                                    ReplyClass = SelectedItemPositions.MesData
                                };
                            }

                            //Send Video function
                            if (Methods.CheckConnectivity())
                            {
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightVideo,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                await Task.Factory.StartNew(() =>
                                {
                                    MessageController.SendMessageTask(this, UserId, ChatId, time2, "", "", videoPath, "", "", "", "", "", "", "", ReplyId).ConfigureAwait(false);
                                });
                            }
                            else
                            {
                                m1.ErrorSendMessage = true;
                                MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                {
                                    TypeView = MessageModelType.RightVideo,
                                    Id = Long.ParseLong(m1.Id),
                                    MesData = m1
                                });

                                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                            }

                            var indexMes = MAdapter.DifferList.IndexOf(MAdapter.DifferList.Last());
                            MAdapter.NotifyItemInserted(indexMes);

                            //Scroll Down >> 
                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    }
                }
                else if (requestCode == 23154 && resultCode == Result.Ok)
                {
                    GetWallpaper();
                }

                CloseReplyUi();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 111)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder(UserId);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
                else if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        switch (PermissionsType)
                        {
                            //requestCode >> 500 => Image Gallery
                            //case "Image" when AppSettings.ImageCropping:
                            //    OpenDialogGallery("Image");
                            //    break;
                            case "Image": //requestCode >> 500 => Image Gallery
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false);
                                break;
                            case "Video":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "Video_camera":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                            case "Camera":
                                //requestCode >> 503 => Camera
                                new IntentController(this).OpenIntentCamera();
                                break;
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
                else if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        switch (PermissionsType)
                        {
                            case "File":
                                //requestCode >> 504 => File
                                new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                                break;
                            case "Music":
                                //requestCode >> 505 => Music
                                new IntentController(this).OpenIntentAudio();
                                break;
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
                else if (requestCode == 101)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //request code of result is 506
                        new IntentController(this).OpenIntentGetContactNumberPhone();
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
                else if (requestCode == 105)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
                else if (requestCode == 102)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        if (RecordButton?.Tag?.ToString() == "Free")
                        {
                            //Set Record Style
                            IsRecording = true;

                            EmojIconEditTextView.Visibility = ViewStates.Invisible;

                            ResetMediaPlayer();

                            RecorderService = new Methods.AudioRecorderAndPlayer(UserId);
                            //Start Audio record
                            await Task.Delay(600);
                            RecorderService.StartRecording();

                            if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            {
                                UserDetails.Socket?.EmitAsync_RecordingEvent(UserId, UserDetails.AccessToken);
                            }
                            else
                                await RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "recording").ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
                else if (requestCode == 1106) //Audio Call
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        StartCall();
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
                else if (requestCode == 1107) //Video call
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        StartVideoCall();
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
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
                if (itemString == GetText(Resource.String.Lbl_ImageGallery)) // image 
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //if (AppSettings.ImageCropping)
                        //    OpenDialogGallery("Image"); //requestCode >> 500 => Image Gallery
                        //else
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage()
                                                                                                  )
                        {
                            //if (AppSettings.ImageCropping)
                            //    OpenDialogGallery("Image"); //requestCode >> 500 => Image Gallery
                            //else
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures), false); //requestCode >> 500 => Image Gallery
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_TakeImageFromCamera)) // Camera 
                {
                    PermissionsType = "Camera";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 503 => Camera
                        new IntentController(this).OpenIntentCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage()
                                                                                                  )
                        {
                            //requestCode >> 503 => Camera
                            new IntentController(this).OpenIntentCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_VideoGallery)) // video  
                {
                    PermissionsType = "Video";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 501 => video Gallery
                        new IntentController(this).OpenIntentVideoGallery();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage()
                                                                                                  )
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
                else if (itemString == GetText(Resource.String.Lbl_RecordVideoFromCamera)) // video camera
                {
                    PermissionsType = "Video_camera";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 513 => video camera
                        new IntentController(this).OpenIntentVideoCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && PermissionsController.CheckPermissionStorage()
                                                                                                  )
                        {
                            //requestCode >> 513 => video camera
                            new IntentController(this).OpenIntentVideoCamera();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_File)) // File  
                {
                    PermissionsType = "File";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 504 => File
                        new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                    }
                    else
                    {
                        if (PermissionsController.CheckPermissionStorage())
                        {
                            //requestCode >> 504 => File
                            new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(100);
                        }
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_Music)) // Music  
                {
                    PermissionsType = "Music";

                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                        new IntentController(this).OpenIntentAudio(); //505
                    else
                    {
                        if (PermissionsController.CheckPermissionStorage())
                            new IntentController(this).OpenIntentAudio(); //505
                        else
                            new PermissionsController(this).RequestPermission(100);
                    }
                }
                else if (itemString == GetText(Resource.String.Lbl_Gif)) // Gif  
                {
                    StartActivityForResult(new Intent(this, typeof(GifActivity)), 300);
                }
                else if (itemString == GetText(Resource.String.Lbl_Contact)) // Contact  
                {
                    ChatContactButtonOnClick();
                }
                else if (itemString == GetText(Resource.String.Lbl_Location)) // Location  
                {
                    ChatLocationButtonOnClick();
                }
                else if (itemString == GetText(Resource.String.Lbl_View_Profile)) // Menu View profile
                {
                    OnMenuViewProfile_Click();
                }
                else if (itemString == GetText(Resource.String.Lbl_Block)) // Menu Block
                {
                    OnMenuBlock_Click();
                }
                else if (itemString == GetText(Resource.String.Lbl_Clear_chat)) // Menu Clear Chat
                {
                    OnMenuClearChat_Click();
                }
                else if (itemString == GetText(Resource.String.Lbl_StartedMessages)) // Menu Started Messages
                {
                    OnMenuStartedMessages_Click();
                }
                else if (itemString == GetText(Resource.String.Lbl_Media)) // Menu Media (Shared Files)
                {
                    OnMenuMedia_Click();
                }
                else if (itemString == GetText(Resource.String.Lbl_ChangeChatTheme)) // Change Chat Theme (color)
                {
                    ChatColorButtonOnClick();
                }
                else if (itemString == GetText(Resource.String.Lbl_Wallpaper)) // Wallpaper
                {
                    var intent = new Intent(this, typeof(WallpaperActivity));
                    StartActivityForResult(intent, 23154);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region loadData

        private void FirstLoadData_Item()
        {
            try
            {
                if (TypeChat == "LastMessenger")
                {
                    DataUser = JsonConvert.DeserializeObject<ChatObject>(Intent?.GetStringExtra("UserItem") ?? "");
                    if (DataUser != null)
                    {
                        DataUser.LastMessage.LastMessageClass.ChatColor ??= AppSettings.MainColor;

                        if (!ColorChanged)
                            MainChatColor = !string.IsNullOrEmpty(DataUser.LastMessage.LastMessageClass?.ChatColor) ? DataUser.LastMessage.LastMessageClass.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(DataUser.LastMessage.LastMessageClass?.ChatColor) : DataUser.LastMessage.LastMessageClass?.ChatColor : AppSettings.MainColor ?? AppSettings.MainColor;
                    }
                    else
                    {
                        if (!ColorChanged)
                            MainChatColor = AppSettings.MainColor;
                    }
                }
                else if (TypeChat == "OneSignalNotification")
                {
                    if (!ColorChanged)
                        MainChatColor = AppSettings.MainColor;
                }
                else
                {
                    UserData = JsonConvert.DeserializeObject<UserDataObject>(Intent?.GetStringExtra("UserItem") ?? "");
                    if (UserData != null)
                    {
                        UserData.ChatColor ??= AppSettings.MainColor;

                        if (!ColorChanged)
                            MainChatColor = UserData.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(UserData.ChatColor) : UserData.ChatColor ?? AppSettings.MainColor;
                    }
                    else
                    {
                        if (!ColorChanged)
                            MainChatColor = AppSettings.MainColor;
                    }
                }

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt >= 23)
                {
                    if (PermissionsController.CheckPermissionStorage() && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                        && CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder(UserId);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(111);
                    }
                }
                else
                {
                    Methods.Path.Chack_MyFolder(UserId);
                }

                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadData_ItemUser()
        {
            try
            {
                if (TypeChat == "LastMessenger")
                {
                    if (DataUser != null)
                    {
                        GlideImageLoader.LoadImage(this, DataUser.Avatar, UserChatProfile, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        ActionBarTitle.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(DataUser.Name), 25);
                        SayHiToTextView.Text = Methods.FunString.DecodeString(DataUser.Name);

                        //Online Or offline
                        if (DataUser.Lastseen == "on")
                        {
                            ActionBarSubTitle.Text = GetString(Resource.String.Lbl_Online);
                            LastSeen = GetString(Resource.String.Lbl_Online);
                        }
                        else
                        {
                            ActionBarSubTitle.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(DataUser.LastseenUnixTime), false);
                            LastSeen = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(DataUser.LastseenUnixTime), false);
                        }
                    }
                }
                else if (TypeChat == "OneSignalNotification")
                {
                    //Get Data Profile API
                    Task.Factory.StartNew(StartApiService);

                    if (!ColorChanged)
                        MainChatColor = AppSettings.MainColor;
                }
                else
                {
                    if (UserData != null)
                    {
                        GlideImageLoader.LoadImage(this, UserData.Avatar, UserChatProfile, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        ActionBarTitle.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(UserData.Name), 25);
                        SayHiToTextView.Text = Methods.FunString.DecodeString(UserData.Name);

                        //Online Or offline
                        if (UserData.Lastseen == "on")
                        {
                            ActionBarSubTitle.Text = GetString(Resource.String.Lbl_Online);
                            LastSeen = GetString(Resource.String.Lbl_Online);
                        }
                        else
                        {
                            ActionBarSubTitle.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserData.LastseenUnixTime), false);
                            LastSeen = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserData.LastseenUnixTime), false);
                        }
                    }
                }

                GetMessages();
            }
            catch (Exception e)
            {
                GetMessages();
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void GetMessages()
        {
            try
            {
                SqLiteDatabase dbDatabase = new SqLiteDatabase();

                var localList = dbDatabase.GetMessages_List(UserDetails.UserId, UserId, "0");
                if (localList?.Count > 0) //Database.. Get Messages Local
                {
                    MAdapter.DifferList = new ObservableCollection<AdapterModelsClassMessage>(localList);
                    MAdapter.NotifyDataSetChanged();

                    //Scroll Down >> 
                    MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                    //SwipeRefreshLayout.Refreshing = false;
                    //SwipeRefreshLayout.Enabled = false; 
                }
                else //Or server.. Get Messages Api
                {
                    //SwipeRefreshLayout.Refreshing = true;
                    //SwipeRefreshLayout.Enabled = true;

                    LoadingLayout.Visibility = ViewStates.Visible;
                    await GetMessages_Api();
                }

                if (MAdapter.DifferList.Count > 0)
                {
                    LoadingLayout.Visibility = ViewStates.Gone;

                    SayHiLayout.Visibility = ViewStates.Gone;
                    SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                }
                else if (MAdapter.DifferList.Count == 0 && ShowEmpty != "no")
                {
                    LoadingLayout.Visibility = ViewStates.Gone;
                    SayHiLayout.Visibility = ViewStates.Visible;
                    SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
                }

                TaskWork = "Working";

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    MessageUpdater();
                    UserDetails.Socket?.EmitAsync_SendSeenMessages(UserId, UserDetails.AccessToken, UserDetails.UserId);
                }
                else
                {
                    //Run timer
                    Timer = new Timer { Interval = AppSettings.MessageRequestSpeed };
                    Timer.Elapsed += TimerOnElapsed;
                    Timer.Enabled = true;
                    Timer.Start();
                }

                LoadPinnedMessage();

                var list = ListUtils.MessageUnreadList?.Where(a => a.Sender == UserId)?.ToList();
                if (list?.Count > 0)
                {
                    foreach (var messageObject in list)
                    {
                        ListUtils.MessageUnreadList.Remove(messageObject);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task GetMessages_Api()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    RunLoadMore = true;
                    var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId);
                    if (apiStatus == 200)
                    {
                        if (respond is UserMessagesObject result)
                        {
                            var countList = MAdapter.DifferList.Count;
                            var respondList = result.Messages.Count;
                            if (respondList > 0)
                            {
                                result.Messages.Reverse();

                                bool add = false;
                                foreach (var item in from item in result.Messages let check = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                                {
                                    var type = Holders.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    add = true;
                                    MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                    {
                                        TypeView = type,
                                        Id = Long.ParseLong(item.Id),
                                        MesData = WoWonderTools.MessageFilter(UserId, item, type, true)
                                    });
                                }

                                if (add)
                                {
                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    // Insert data user in database
                                    dbDatabase.Insert_Or_Replace_MessagesTable(MAdapter.DifferList);

                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            if (countList > 0)
                                                MAdapter.NotifyItemRangeInserted(countList, MAdapter.DifferList.Count - countList);
                                            else
                                                MAdapter.NotifyDataSetChanged();

                                            //Scroll Down >> 
                                            MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }

                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    //SwipeRefreshLayout.Refreshing = false;
                    //SwipeRefreshLayout.Enabled = false;

                    RunLoadMore = false;
                }
                else ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void MessageUpdater()
        {
            try
            {
                if (TaskWork == "Working")
                {
                    TaskWork = "Stop";

                    if (Methods.CheckConnectivity())
                    {
                        //var data = MAdapter.DifferList.LastOrDefault();
                        //var lastMessageId = data?.MesData?.Id ?? "0";
                        var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId, "0", "0", "35");
                        if (apiStatus == 200)
                        {
                            if (respond is UserMessagesObject result)
                            {
                                try
                                {
                                    var typing = result.Typing.ToString();
                                    ActionBarSubTitle.Text = typing == "1" ? GetString(Resource.String.Lbl_Typping) : LastSeen ?? LastSeen;

                                    var isRecording = result.IsRecording.ToString();
                                    ActionBarSubTitle.Text = isRecording == "1" ? GetString(Resource.String.Lbl_Recording) : LastSeen ?? LastSeen;
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }

                                var countList = MAdapter.DifferList.Count;
                                var respondList = result.Messages.Count;
                                if (respondList > 0)
                                {
                                    foreach (var item in result.Messages)
                                    {
                                        var type = Holders.GetTypeModel(item);
                                        if (type == MessageModelType.None)
                                            continue;

                                        var check = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == item.Id);
                                        if (check == null)
                                        {
                                            MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                            {
                                                TypeView = type,
                                                Id = Long.ParseLong(item.Id),
                                                MesData = WoWonderTools.MessageFilter(UserId, item, type, true)
                                            });

                                            RunOnUiThread(() =>
                                            {
                                                MAdapter.NotifyItemInserted(MAdapter.DifferList.Count - 1);

                                                //Scroll Down >> 
                                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                                            });

                                            #region Last chat

                                            //if (AppSettings.LastChatSystem == SystemApiGetLastChat.New)
                                            //{
                                            //    var dataUser = GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.UserId == item.FromId);
                                            //    if (dataUser != null)
                                            //    {
                                            //        if (item.UserData != null)
                                            //        {
                                            //            dataUser.LastChat.UserId = item.UserData.UserId;
                                            //            dataUser.LastChat.Avatar = item.UserData.Avatar;
                                            //        }

                                            //        dataUser.LastChat.LastMessage = new LastMessageUnion
                                            //        {
                                            //            LastMessageClass = item,
                                            //        };
                                            //        dataUser.LastChat.LastMessage.LastMessageClass.ChatColor = MainChatColor;

                                            //        var index = GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.IndexOf(dataUser);
                                            //        if (index > -1 && index != 0)
                                            //        {
                                            //            GlobalContext?.LastChatTab.MAdapter.LastChatsList.Move(Convert.ToInt32(index), 0);
                                            //            GlobalContext?.LastChatTab.MAdapter.NotifyItemMoved(Convert.ToInt32(index), 0);
                                            //        }
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    var dataUser = GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastMessagesUser?.UserId == item.FromId);
                                            //    if (dataUser != null)
                                            //    {
                                            //        if (item.UserData != null)
                                            //        {
                                            //            dataUser.LastMessagesUser.UserId = item.UserData.UserId;
                                            //            dataUser.LastMessagesUser.Avatar = item.UserData.Avatar;
                                            //        }

                                            //        dataUser.LastMessagesUser.ChatColor = MainChatColor;

                                            //        //last_message
                                            //        dataUser.LastMessagesUser.LastMessage = new MessageData
                                            //        {
                                            //            Id = item.Id,
                                            //            FromId = item.FromId,
                                            //            GroupId = item.GroupId,
                                            //            ToId = item.ToId,
                                            //            Text = item.Text,
                                            //            Media = item.Media,
                                            //            MediaFileName = item.MediaFileName,
                                            //            MediaFileNames = item.MediaFileNames,
                                            //            Time = item.Time,
                                            //            Seen = "1",
                                            //            DeletedOne = item.DeletedOne,
                                            //            DeletedTwo = item.DeletedTwo,
                                            //            SentPush = item.SentPush,
                                            //            NotificationId = item.NotificationId,
                                            //            TypeTwo = item.TypeTwo,
                                            //            Stickers = item.Stickers,
                                            //            Lat = item.Lat,
                                            //            Lng = item.Lng, 
                                            //            ProductId = item.ProductId,

                                            //            // DateTime = dateTime,
                                            //        };

                                            //        var index = GlobalContext?.LastChatTab?.MAdapter?.LastChatsList?.IndexOf(dataUser);
                                            //        if (index > -1 && index != 0)
                                            //        {
                                            //            GlobalContext?.LastChatTab.MAdapter.LastChatsList.Move(Convert.ToInt32(index), 0);
                                            //            GlobalContext?.LastChatTab.MAdapter.NotifyItemMoved(Convert.ToInt32(index), 0);
                                            //        }
                                            //    }
                                            //}

                                            #endregion

                                            if (UserDetails.SoundControl)
                                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");
                                        }
                                        else if (check.MesData.FromId == UserDetails.UserId && check.MesData.Seen != item.Seen) // right
                                        {
                                            check.Id = Convert.ToInt32(item.Id);
                                            check.MesData.Seen = item.Seen;
                                            check.MesData = WoWonderTools.MessageFilter(UserId, item, type, true);
                                            check.TypeView = type;

                                            RunOnUiThread(() =>
                                            {
                                                if (check.MesData.Position == "right")
                                                    MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(check));
                                            });

                                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                            // Insert data user in database
                                            dbDatabase.Insert_Or_Update_To_one_MessagesTable(check.MesData);

                                        }
                                    }

                                    if (MAdapter.DifferList.Count > countList)
                                    {
                                        SqLiteDatabase liteDatabase = new SqLiteDatabase();
                                        // Insert data user in database
                                        liteDatabase.Insert_Or_Replace_MessagesTable(MAdapter.DifferList);
                                    }

                                    if (MAdapter.DifferList.Count > 0)
                                    {
                                        SayHiLayout.Visibility = ViewStates.Gone;
                                        SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                                    }
                                    else if (MAdapter.DifferList.Count == 0 && ShowEmpty != "no")
                                    {
                                        SayHiLayout.Visibility = ViewStates.Visible;
                                        SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
                                    }
                                }
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);

                    TaskWork = "Working";
                }
            }
            catch (Exception e)
            {
                TaskWork = "Working";
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task<string> LoadMore_Messages_Database()
        {
            try
            {
                if (RunLoadMore)
                    return "1";

                RunLoadMore = true;

                SqLiteDatabase dbDatabase = new SqLiteDatabase();

                var data = MAdapter.DifferList.FirstOrDefault();
                var firstMessageId = data?.MesData?.Id;
                var countList = MAdapter.DifferList.Count;

                var localList = dbDatabase.GetMessageList(UserDetails.UserId, UserId, firstMessageId);
                if (localList?.Count > 0) //Database.. Get Messages Local
                {
                    bool add = false;
                    foreach (var item in from item in localList let check = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == item.MesData?.Id) where check == null select item)
                    {
                        var type = Holders.GetTypeModel(item.MesData);
                        if (type == MessageModelType.None)
                            continue;

                        add = true;
                        var mes = new AdapterModelsClassMessage
                        {
                            TypeView = type,
                            Id = Long.ParseLong(item.MesData.Id),
                            MesData = WoWonderTools.MessageFilter(UserId, item.MesData, type, true)
                        };

                        MAdapter.DifferList.Insert(0, mes);

                    }

                    if (add)
                    {
                        RunOnUiThread(() =>
                        {
                            MAdapter?.NotifyItemRangeInserted(0, MAdapter.DifferList.Count - countList);
                        });
                    }

                    RunLoadMore = false;
                    return "1";
                }

                //if (SwipeRefreshLayout.Refreshing)
                //{
                //    SwipeRefreshLayout.Refreshing = false;
                //    SwipeRefreshLayout.Enabled = false;  
                //}

                RunLoadMore = false;
                return "0";
            }
            catch (Exception e)
            {
                RunLoadMore = false;
                Methods.DisplayReportResultTrack(e);
                await Task.Delay(0);
                return "0";
            }
        }

        private bool RunLoadMore;
        private async Task LoadMoreMessages_API()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (RunLoadMore)
                        return;

                    RunLoadMore = true;

                    var data = MAdapter.DifferList.FirstOrDefault();
                    var firstMessageId = data?.MesData?.Id;
                    var countList = MAdapter.DifferList.Count;

                    var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId, firstMessageId);
                    if (apiStatus == 200)
                    {
                        if (respond is UserMessagesObject result)
                        {
                            var respondList = result.Messages.Count;
                            if (respondList > 0)
                            {
                                bool add = false;
                                foreach (var item in from item in result.Messages let check = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                                {
                                    var type = Holders.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    add = true;
                                    var mes = new AdapterModelsClassMessage
                                    {
                                        TypeView = type,
                                        Id = Long.ParseLong(item.Id),
                                        MesData = WoWonderTools.MessageFilter(UserId, item, type, true)
                                    };

                                    MAdapter.DifferList.Insert(0, mes);

                                }

                                if (add)
                                {
                                    RunOnUiThread(() =>
                                    {
                                        MAdapter?.NotifyItemRangeInserted(0, MAdapter.DifferList.Count - countList);
                                    });

                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    // Insert data user in database
                                    dbDatabase.Insert_Or_Replace_MessagesTable(MAdapter.DifferList);
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    //if (SwipeRefreshLayout.Refreshing)
                    //{
                    //    SwipeRefreshLayout.Refreshing = false;
                    //    SwipeRefreshLayout.Enabled = false;
                    //}

                    RunLoadMore = false;
                }
                else ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
            catch (Exception e)
            {
                RunLoadMore = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task GetMessagesById(string id)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Message.FetchUserMessagesAsync(UserId, "0", "0", "1", id);
                    if (apiStatus == 200)
                    {
                        if (respond is UserMessagesObject result)
                        {
                            var countList = MAdapter.DifferList.Count;
                            var respondList = result.Messages.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in result.Messages)
                                {
                                    var type = Holders.GetTypeModel(item);
                                    if (type == MessageModelType.None)
                                        continue;

                                    var check = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == item.Id);
                                    if (check == null)
                                    {
                                        MAdapter.DifferList.Add(new AdapterModelsClassMessage
                                        {
                                            TypeView = type,
                                            Id = Long.ParseLong(item.Id),
                                            MesData = WoWonderTools.MessageFilter(UserId, item, type, true)
                                        });

                                        RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                //if (countList > 0)
                                                //    MAdapter.NotifyItemRangeInserted(countList, MAdapter.DifferList.Count - countList);
                                                //else
                                                MAdapter.NotifyDataSetChanged();

                                                //Scroll Down >> 
                                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        check.Id = Convert.ToInt32(item.Id);
                                        check.MesData = WoWonderTools.MessageFilter(UserId, item, type, true);
                                        check.TypeView = type;

                                        RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(check));
                                            }
                                            catch (Exception e)
                                            {
                                                Methods.DisplayReportResultTrack(e);
                                            }
                                        });

                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                        // Insert data user in database
                                        dbDatabase.Insert_Or_Update_To_one_MessagesTable(check.MesData);
                                    }
                                }

                                if (MAdapter.DifferList.Count > countList)
                                {
                                    SqLiteDatabase liteDatabase = new SqLiteDatabase();
                                    // Insert data user in database
                                    liteDatabase.Insert_Or_Replace_MessagesTable(MAdapter.DifferList);
                                }

                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        if (MAdapter.DifferList.Count > 0)
                                        {
                                            SayHiLayout.Visibility = ViewStates.Gone;
                                            SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                                        }
                                        else if (MAdapter.DifferList.Count == 0 && ShowEmpty != "no")
                                        {
                                            SayHiLayout.Visibility = ViewStates.Visible;
                                            SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Methods.DisplayReportResultTrack(e);
                                    }
                                });
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                else ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Mic Record 

        public void OnClick(View v)
        {
            try
            {
                //ToastUtils.ShowToast(this, "RECORD BUTTON CLICKED", ToastLength.Short);
                OnClick_OfSendButton();

                //ChatMediaButton.Enabled = true;
                //EmojIconEditTextView.Enabled = true;
                //ChatEmojImage.Enabled = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnStartRecord()
        {
            //ToastUtils.ShowToast(this, "OnStartRecord", ToastLength.Short);

            //record voices ( Permissions is 102 )
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (RecordButton?.Tag?.ToString() == "Free")
                    {
                        //Set Record Style
                        IsRecording = true;

                        LayoutEditText.Visibility = ViewStates.Invisible;
                        //ChatColorButton.Visibility = ViewStates.Invisible;
                        //ChatStickerButton.Visibility = ViewStates.Invisible;
                        ChatMediaButton.Visibility = ViewStates.Invisible;

                        ResetMediaPlayer();

                        RecorderService = new Methods.AudioRecorderAndPlayer(UserId);
                        //Start Audio record
                        await Task.Delay(600);
                        RecorderService.StartRecording();

                        if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        {
                            UserDetails.Socket?.EmitAsync_RecordingEvent(UserId, UserDetails.AccessToken);
                        }
                        else
                            await RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "recording").ConfigureAwait(false);
                    }
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                    {
                        if (RecordButton?.Tag?.ToString() == "Free")
                        {
                            //Set Record Style
                            IsRecording = true;

                            LayoutEditText.Visibility = ViewStates.Invisible;
                            //ChatColorButton.Visibility = ViewStates.Invisible;
                            //ChatStickerButton.Visibility = ViewStates.Invisible;
                            ChatMediaButton.Visibility = ViewStates.Invisible;

                            ResetMediaPlayer();

                            RecorderService = new Methods.AudioRecorderAndPlayer(UserId);
                            //Start Audio record
                            await Task.Delay(600);
                            RecorderService.StartRecording();

                            if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                            {
                                UserDetails.Socket?.EmitAsync_RecordingEvent(UserId, UserDetails.AccessToken);
                            }
                            else
                                await RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "recording").ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(102);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void OnCancelRecord()
        {
            try
            {
                RecorderService.StopRecording();

                await Task.Delay(1000);

                //ToastUtils.ShowToast(this, "OnCancelRecord", ToastLength.Short);
                // reset mic nd show edittext
                LayoutEditText.Visibility = ViewStates.Visible;
                //ChatColorButton.Visibility = ViewStates.Visible;
                //ChatStickerButton.Visibility = ViewStates.Visible;
                ChatMediaButton.Visibility = ViewStates.Visible;

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    UserDetails.Socket?.EmitAsync_StoppedEvent(UserId, UserDetails.AccessToken);
                }
                else
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "stopped") });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnFinishRecord(long recordTime)
        {
            //ToastUtils.ShowToast(this, "OnFinishRecord " + recordTime, ToastLength.Short);
            //open fragemt recoud and show edittext
            try
            {
                if (IsRecording)
                {
                    RecorderService.StopRecording();
                    var filePath = RecorderService.GetRecorded_Sound_Path();

                    RecordButton.Tag = "Text";
                    RecordButton.SetTheImageResource(Resource.Drawable.ic_send_vector);
                    RecordButton.SetListenForRecord(false);

                    if (recordTime > 0)
                    {
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            Bundle bundle = new Bundle();
                            bundle.PutString("FilePath", filePath);
                            ChatRecordSoundBoxFragment.Arguments = bundle;
                            ReplaceTopFragment(ChatRecordSoundBoxFragment);
                        }
                    }

                    IsRecording = false;
                }

                await Task.Delay(1000);

                LayoutEditText.Visibility = ViewStates.Visible;
                //ChatColorButton.Visibility = ViewStates.Visible;
                //ChatStickerButton.Visibility = ViewStates.Visible;
                ChatMediaButton.Visibility = ViewStates.Visible;

                //ChatMediaButton.Enabled = false;
                //EmojIconEditTextView.Enabled = false;
                //ChatEmojImage.Enabled = false;

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    UserDetails.Socket?.EmitAsync_StoppedEvent(UserId, UserDetails.AccessToken);
                }
                else
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.SetChatTypingStatusAsync(UserId, "stopped") });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnLessThanSecond()
        {
            //ToastUtils.ShowToast(this, "OnLessThanSecond", ToastLength.Short); 
        }

        #endregion

        private void ResetMediaPlayer()
        {
            try
            {
                var list = MAdapter.DifferList.Where(a => a.TypeView == MessageModelType.LeftAudio || a.TypeView == MessageModelType.RightAudio && a.MesData.MediaPlayer != null).ToList();
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        item.MesData.MediaIsPlaying = false;

                        if (item.MesData.MediaPlayer != null)
                        {
                            item.MesData.MediaPlayer.Stop();
                            item.MesData.MediaPlayer.Reset();
                        }
                        item.MesData.MediaPlayer?.Release();
                        item.MesData.MediaPlayer = null!;
                        item.MesData.MediaTimer = null!;
                    }
                    MAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ResetButtonTags()
        {
            try
            {
                //ChatStickerButton.Tag = "Closed";
                ChatColorButtonTag = "Closed";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnBackPressed()
        {
            try
            {
                if (SupportFragmentManager.BackStackEntryCount > 0)
                {
                    RemoveButtonFragment();
                }
                else
                {
                    MainChatColor = AppSettings.MainColor;
                    ColorChanged = false;
                    base.OnBackPressed();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            try
            {
                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Fragment

        private void ReplaceTopFragment(SupportFragment fragmentView)
        {
            try
            {
                if (fragmentView.IsVisible)
                    return;

                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(TopFragmentHolder.Id, fragmentView);

                if (SupportFragmentManager.BackStackEntryCount == 0)
                {
                    trans.AddToBackStack(null);
                }

                trans.Commit();

                TopFragmentHolder.TranslationY = 1200;
                TopFragmentHolder?.Animate()?.SetInterpolator(new FastOutSlowInInterpolator())?.TranslationYBy(-1200)?.SetDuration(500);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ReplaceButtonFragment(SupportFragment fragmentView)
        {
            try
            {
                if (fragmentView != MainFragmentOpened)
                {
                    if (MainFragmentOpened == ChatColorBoxFragment)
                    {
                        //ChatColorButton?.Drawable?.SetTint(Color.ParseColor("#888888"));
                    }
                }

                HideKeyboard();

                if (fragmentView.IsVisible)
                    return;

                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(ButtonFragmentHolder.Id, fragmentView);

                if (SupportFragmentManager.BackStackEntryCount == 0)
                {
                    trans.AddToBackStack(null);
                }

                trans.Commit();

                ButtonFragmentHolder.TranslationY = 1200;
                ButtonFragmentHolder?.Animate()?.SetInterpolator(new FastOutSlowInInterpolator())?.TranslationYBy(-1200)?.SetDuration(500);
                MainFragmentOpened = fragmentView;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveButtonFragment()
        {
            try
            {
                if (SupportFragmentManager.BackStackEntryCount > 0)
                {
                    SupportFragmentManager.PopBackStack();
                    ResetButtonTags();
                    //ChatColorButton.Drawable?.SetTint(Color.ParseColor("#888888"));
                    //ChatStickerButton.Drawable?.SetTint(Color.ParseColor("#888888"));

                    if (SupportFragmentManager.Fragments.Count > 0)
                    {
                        var fragmentManager = SupportFragmentManager.BeginTransaction();
                        foreach (var vrg in SupportFragmentManager.Fragments)
                        {
                            Console.WriteLine(vrg);
                            if (SupportFragmentManager.Fragments.Contains(ChatColorBoxFragment))
                            {
                                fragmentManager.Remove(ChatColorBoxFragment);
                            }
                        }

                        fragmentManager.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Selected

        //Message Info 
        public void MessageInfoItems()
        {
            try
            {
                var intent = new Intent(this, typeof(MessageInfoActivity));
                intent.PutExtra("UserId", UserId);
                intent.PutExtra("MainChatColor", MainChatColor);
                intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Forward Messages
        public void ForwardItems()
        {
            try
            {
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }

                if (SelectedItemPositions != null)
                {
                    var intent = new Intent(this, typeof(ForwardMessagesActivity));
                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(SelectedItemPositions.MesData));
                    StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Reply Messages
        public void ReplyItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    RepliedMessageView.Visibility = ViewStates.Visible;
                    var animation = new TranslateAnimation(0, 0, RepliedMessageView.Height, 0) { Duration = 300 };

                    RepliedMessageView.StartAnimation(animation);

                    ReplyId = SelectedItemPositions.MesData.Id;

                    TxtOwnerName.Text = SelectedItemPositions.MesData.MessageUser?.User?.UserId == UserDetails.UserId ? GetText(Resource.String.Lbl_You) : ActionBarTitle.Text;

                    if (SelectedItemPositions.TypeView == MessageModelType.LeftText || SelectedItemPositions.TypeView == MessageModelType.RightText)
                    {
                        MessageFileThumbnail.Visibility = ViewStates.Gone;
                        TxtMessageType.Visibility = ViewStates.Gone;
                        TxtShortMessage.Text = SelectedItemPositions.MesData.Text;
                    }
                    else
                    {
                        MessageFileThumbnail.Visibility = ViewStates.Visible;
                        var fileName = SelectedItemPositions.MesData.Media.Split('/').Last();
                        switch (SelectedItemPositions.TypeView)
                        {
                            case MessageModelType.LeftVideo:
                            case MessageModelType.RightVideo:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.video);

                                    var fileNameWithoutExtension = fileName.Split('.').First();

                                    var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDiskVideo + UserId, fileNameWithoutExtension + ".png");
                                    if (videoImage == "File Dont Exists")
                                    {
                                        var mediaFile = WoWonderTools.GetFile(UserId, Methods.Path.FolderDiskVideo, fileName, SelectedItemPositions.MesData.Media, "video");
                                        File file2 = new File(mediaFile);
                                        try
                                        {
                                            Uri photoUri = SelectedItemPositions.MesData.Media.Contains("http") ? Uri.Parse(SelectedItemPositions.MesData.Media) : FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                            Glide.With(this)
                                                .AsBitmap()
                                                .Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable))
                                                .Load(photoUri) // or URI/path
                                                .Into(MessageFileThumbnail);  //image view to set thumbnail to 
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                            Glide.With(this)
                                                .AsBitmap()
                                                .Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable))
                                                .Load(file2) // or URI/path
                                                .Into(MessageFileThumbnail);  //image view to set thumbnail to 
                                        }
                                    }
                                    else
                                    {
                                        File file = new File(videoImage);
                                        try
                                        {
                                            Uri photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file);
                                            Glide.With(this).Load(photoUri).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(MessageFileThumbnail);
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                            Glide.With(this).Load(file).Apply(GlideImageLoader.GetRequestOptions(ImageStyle.RoundedCrop, ImagePlaceholders.Drawable)).Into(MessageFileThumbnail);
                                        }
                                    }
                                    break;
                                }
                            case MessageModelType.LeftGif:
                            case MessageModelType.RightGif:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_Gif);
                                    SelectedItemPositions.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDiskGif, fileName, SelectedItemPositions.MesData.Media, "image");

                                    if (SelectedItemPositions.MesData.Media.Contains("http"))
                                    {
                                        GlideImageLoader.LoadImage(this, SelectedItemPositions.MesData.Media, MessageFileThumbnail, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        var file = Uri.FromFile(new File(SelectedItemPositions.MesData.Media));
                                        Glide.With(this).Load(file.Path).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    }
                                    break;
                                }
                            case MessageModelType.LeftSticker:
                            case MessageModelType.RightSticker:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_Sticker);
                                    SelectedItemPositions.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDiskSticker, fileName, SelectedItemPositions.MesData.Media, "sticker");

                                    if (SelectedItemPositions.MesData.Media.Contains("http"))
                                    {
                                        GlideImageLoader.LoadImage(this, SelectedItemPositions.MesData.Media, MessageFileThumbnail, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        var file = Uri.FromFile(new File(SelectedItemPositions.MesData.Media));
                                        Glide.With(this).Load(file.Path).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    }
                                    break;
                                }
                            case MessageModelType.LeftImage:
                            case MessageModelType.RightImage:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.image);

                                    SelectedItemPositions.MesData.Media = WoWonderTools.GetFile(UserId, Methods.Path.FolderDcimImage, fileName, SelectedItemPositions.MesData.Media, "image");

                                    if (SelectedItemPositions.MesData.Media.Contains("http"))
                                    {
                                        GlideImageLoader.LoadImage(this, SelectedItemPositions.MesData.Media, MessageFileThumbnail, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                    }
                                    else
                                    {
                                        var file = Uri.FromFile(new File(SelectedItemPositions.MesData.Media));
                                        Glide.With(this).Load(file.Path).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    }
                                    break;
                                }
                            case MessageModelType.LeftAudio:
                            case MessageModelType.RightAudio:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_VoiceMessage) + " (" + SelectedItemPositions.MesData.MediaDuration + ")";
                                    Glide.With(this).Load(GetDrawable(Resource.Drawable.Audio_File)).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftFile:
                            case MessageModelType.RightFile:
                                {
                                    TxtMessageType.Text = GetText(Resource.String.Lbl_File);

                                    var fileNameWithoutExtension = fileName.Split('.').First();
                                    var fileNameExtension = fileName.Split('.').Last();

                                    TxtShortMessage.Text = Methods.FunString.SubStringCutOf(fileNameWithoutExtension, 10) + fileNameExtension;
                                    Glide.With(this).Load(GetDrawable(Resource.Drawable.Image_File)).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftMap:
                            case MessageModelType.RightMap:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_Location);
                                    Glide.With(this).Load(SelectedItemPositions.MesData.MessageMap).Apply(new RequestOptions().Placeholder(Resource.Drawable.Image_Map).Error(Resource.Drawable.Image_Map)).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftContact:
                            case MessageModelType.RightContact:
                                {
                                    TxtMessageType.Text = GetText(Resource.String.Lbl_Contact);
                                    TxtShortMessage.Text = SelectedItemPositions.MesData.ContactName;
                                    Glide.With(this).Load(Resource.Drawable.no_profile_image).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftProduct:
                            case MessageModelType.RightProduct:
                                {
                                    TxtMessageType.Visibility = ViewStates.Gone;
                                    TxtShortMessage.Text = GetText(Resource.String.Lbl_Product);
                                    string imageUrl = !string.IsNullOrEmpty(SelectedItemPositions.MesData.Media) ? SelectedItemPositions.MesData.Media : SelectedItemPositions.MesData.Product?.ProductClass?.Images[0]?.Image;
                                    Glide.With(this).Load(imageUrl).Apply(new RequestOptions()).Into(MessageFileThumbnail);
                                    break;
                                }
                            case MessageModelType.LeftText:
                            case MessageModelType.RightText:
                            case MessageModelType.None:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Delete Message
        public void DeleteMessageItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    if (Methods.CheckConnectivity())
                    {
                        if (Timer != null)
                        {
                            Timer.Enabled = false;
                            Timer.Stop();
                        }

                        var index = MAdapter.DifferList.IndexOf(SelectedItemPositions);
                        if (index != -1)
                        {
                            MAdapter.DifferList.Remove(SelectedItemPositions);

                            MAdapter.NotifyItemRemoved(index);
                            MAdapter.NotifyDataSetChanged();
                        }

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Delete_OneMessageUser(SelectedItemPositions.Id.ToString());

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.DeleteMessageAsync(SelectedItemPositions.Id.ToString()) });

                        if (Timer != null)
                        {
                            Timer.Enabled = true;
                            Timer.Start();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Star Message 
        public void StarMessageItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = false;
                        Timer.Stop();
                    }

                    SelectedItemPositions.MesData.Fav = SelectedItemPositions.MesData.Fav == "no" ? "yes" : "no";

                    var index = MAdapter.DifferList.IndexOf(SelectedItemPositions);
                    if (index != -1)
                    {
                        MAdapter.NotifyItemChanged(index);
                    }

                    if (SelectedItemPositions.MesData.Fav == "yes")
                    {
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.FavoriteMessageAsync(SelectedItemPositions.MesData.Id, ChatId, "yes", "user") });
                    }
                    else
                    {
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.FavoriteMessageAsync(SelectedItemPositions.MesData.Id, ChatId, "no", "user") });
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Pin Message 
        public void PinMessageItems()
        {
            try
            {
                if (SelectedItemPositions != null)
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = false;
                        Timer.Stop();
                    }

                    SelectedItemPositions.MesData.IsPinned = !SelectedItemPositions.MesData.IsPinned;

                    var index = MAdapter.DifferList.IndexOf(SelectedItemPositions);
                    if (index != -1)
                    {
                        MAdapter.NotifyItemChanged(index);
                    }

                    if (SelectedItemPositions.MesData.IsPinned)
                    {
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.PinMessageAsync(SelectedItemPositions.MesData.Id, ChatId, "yes", "user") });
                    }
                    else
                    {
                        if (Methods.CheckConnectivity())
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.PinMessageAsync(SelectedItemPositions.MesData.Id, ChatId, "no", "user") });
                    }

                    LoadPinnedMessage();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadPinnedMessage()
        {
            try
            {
                if (PinnedMessageList?.Count > 0)
                {
                    PinMessageView.Visibility = ViewStates.Visible;
                    var lastChat = PinnedMessageList.LastOrDefault();
                    if (lastChat != null)
                    {
                        switch (lastChat?.TypeView)
                        {
                            case MessageModelType.LeftText:
                            case MessageModelType.RightText:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + lastChat.MesData.Text;
                                break;
                            case MessageModelType.LeftVideo:
                            case MessageModelType.RightVideo:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.video);
                                break;
                            case MessageModelType.LeftGif:
                            case MessageModelType.RightGif:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Gif);
                                break;
                            case MessageModelType.LeftSticker:
                            case MessageModelType.RightSticker:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Sticker);
                                break;
                            case MessageModelType.LeftImage:
                            case MessageModelType.RightImage:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.image);
                                break;
                            case MessageModelType.LeftAudio:
                            case MessageModelType.RightAudio:
                                if (string.IsNullOrEmpty(SelectedItemPositions?.MesData?.MediaDuration))
                                    ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_VoiceMessage);
                                else
                                    ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_VoiceMessage) + " (" + SelectedItemPositions?.MesData?.MediaDuration + ")";
                                break;
                            case MessageModelType.LeftFile:
                            case MessageModelType.RightFile:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_File);
                                break;
                            case MessageModelType.LeftMap:
                            case MessageModelType.RightMap:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Location);
                                break;
                            case MessageModelType.LeftContact:
                            case MessageModelType.RightContact:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Contact);
                                break;
                            case MessageModelType.LeftProduct:
                            case MessageModelType.RightProduct:
                                ShortPinMessage.Text = GetText(Resource.String.Lbl_LastPinnedMessage) + ": " + GetText(Resource.String.Lbl_Product);
                                break;
                            case MessageModelType.None:
                                break;
                        }
                    }
                }
                else
                {
                    PinMessageView.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Get Data User & Favorite & Pin >> Api 

        //Get Data Profile API
        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetProfileApi, GetPinMessageApi, GetFavoriteMessageApi });
        }

        private async Task GetFavoriteMessageApi()
        {
            if (!AppSettings.EnableFavoriteMessageSystem)
                return;

            var (apiStatus, respond) = await RequestsAsync.Message.GetFavoriteMessageAsync(ChatId);
            if (apiStatus != 200 || respond is not DetailsMessagesObject result || result.Data == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                var respondList = result.Data.Count;
                if (respondList > 0)
                {
                    foreach (var item in from item in result.Data let check = StartedMessageList.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                    {
                        var type = Holders.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        StartedMessageList.Add(new AdapterModelsClassMessage
                        {
                            TypeView = type,
                            Id = Long.ParseLong(item.Id),
                            MesData = WoWonderTools.MessageFilter(UserId, item, type, true)
                        });
                    }
                }
            }
        }

        private async Task GetPinMessageApi()
        {
            if (!AppSettings.EnablePinMessageSystem)
                return;

            var (apiStatus, respond) = await RequestsAsync.Message.GetPinMessageAsync(ChatId);
            if (apiStatus != 200 || respond is not DetailsMessagesObject result || result.Data == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                var respondList = result.Data.Count;
                if (respondList > 0)
                {
                    foreach (var item in from item in result.Data let check = PinnedMessageList.FirstOrDefault(a => a.MesData.Id == item.Id) where check == null select item)
                    {
                        var type = Holders.GetTypeModel(item);
                        if (type == MessageModelType.None)
                            continue;

                        PinnedMessageList.Add(new AdapterModelsClassMessage
                        {
                            TypeView = type,
                            Id = Long.ParseLong(item.Id),
                            MesData = WoWonderTools.MessageFilter(UserId, item, type, true)
                        });
                    }
                }
            }
        }

        private async Task GetProfileApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(UserId, "user_data");
            if (apiStatus != 200 || respond is not GetUserDataObject result || result.UserData == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        UserData = result.UserData;

                        if (DataUser != null)
                            DataUser.UserData = result.UserData;

                        GlideImageLoader.LoadImage(this, result.UserData.Avatar, UserChatProfile, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        ActionBarTitle.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(result.UserData.Name), 25);
                        SayHiToTextView.Text = Methods.FunString.DecodeString(result.UserData.Name);

                        //Online Or offline
                        if (result.UserData.LastseenStatus == "on")
                        {
                            ActionBarSubTitle.Text = GetString(Resource.String.Lbl_Online);
                            LastSeen = GetString(Resource.String.Lbl_Online);
                        }
                        else
                        {
                            ActionBarSubTitle.Text = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(result.UserData.LastseenUnixTime), false);
                            LastSeen = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(result.UserData.LastseenUnixTime), false);
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
        }

        #endregion

        public void Update_One_Messages(MessageData message, bool withBlob = false, bool withScroll = true)
        {
            try
            {
                var type = Holders.GetTypeModel(message);
                if (type == MessageModelType.None)
                    return;

                var checker = MAdapter.DifferList.FirstOrDefault(a => a.MesData.Id == message.Id);
                if (checker != null)
                {
                    checker.Id = Convert.ToInt32(message.Id);
                    checker.MesData = WoWonderTools.MessageFilter(UserId, message, type, true);
                    checker.TypeView = type;

                    RunOnUiThread(() =>
                    {
                        try
                        {
                            if (withBlob)
                                MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker));
                            else
                            {
                                switch (checker.TypeView)
                                {
                                    case MessageModelType.RightGif:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobGIF");
                                        break;
                                    case MessageModelType.RightText:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker));
                                        break;
                                    case MessageModelType.RightSticker:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobSticker");
                                        break;
                                    case MessageModelType.RightContact:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker));
                                        break;
                                    case MessageModelType.RightFile:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobFile");
                                        break;
                                    case MessageModelType.RightVideo:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobVideo");
                                        break;
                                    case MessageModelType.RightImage:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobImage");
                                        break;
                                    case MessageModelType.RightAudio:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobAudio");
                                        break;
                                    case MessageModelType.RightMap:
                                        MAdapter.NotifyItemChanged(MAdapter.DifferList.IndexOf(checker), "WithoutBlobMap");
                                        break;
                                }
                            }

                            //Scroll Down >> 
                            if (withScroll)
                                MRecycler.ScrollToPosition(MAdapter.ItemCount - 1);

                            if (MAdapter.DifferList.Count > 0)
                            {
                                SayHiLayout.Visibility = ViewStates.Gone;
                                SayHiSuggestionsRecycler.Visibility = ViewStates.Gone;
                            }
                            else if (MAdapter.DifferList.Count == 0 && ShowEmpty != "no")
                            {
                                SayHiLayout.Visibility = ViewStates.Visible;
                                SayHiSuggestionsRecycler.Visibility = ViewStates.Visible;
                            }

                            CloseReplyUi();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetTheme(string color)
        {
            try
            {
                if (color.Contains("b582af"))
                {
                    SetTheme(Resource.Style.Chatththemeb582af);
                }
                else if (color.Contains("a84849") || color.Contains("a52729"))
                {
                    SetTheme(Resource.Style.Chatththemea84849);
                }
                else if (color.Contains("f9c270"))
                {
                    SetTheme(Resource.Style.Chatththemef9c270);
                }
                else if (color.Contains("70a0e0"))
                {
                    SetTheme(Resource.Style.Chatththeme70a0e0);
                }
                else if (color.Contains("56c4c5"))
                {
                    SetTheme(Resource.Style.Chatththeme56c4c5);
                }
                else if (color.Contains("f33d4c"))
                {
                    SetTheme(Resource.Style.Chatththemef33d4c);
                }
                else if (color.Contains("a1ce79"))
                {
                    SetTheme(Resource.Style.Chatththemea1ce79);
                }
                else if (color.Contains("a085e2"))
                {
                    SetTheme(Resource.Style.Chatththemea085e2);
                }
                else if (color.Contains("ed9e6a"))
                {
                    SetTheme(Resource.Style.Chatththemeed9e6a);
                }
                else if (color.Contains("2b87ce"))
                {
                    SetTheme(Resource.Style.Chatththeme2b87ce);
                }
                else if (color.Contains("f2812b"))
                {
                    SetTheme(Resource.Style.Chatththemef2812b);
                }
                else if (color.Contains("0ba05d"))
                {
                    SetTheme(Resource.Style.Chatththeme0ba05d);
                }
                else if (color.Contains("0e71ea"))
                {
                    SetTheme(Resource.Style.Chatththeme0e71ea);
                }
                else if (color.Contains("aa2294"))
                {
                    SetTheme(Resource.Style.Chatththemeaa2294);
                }
                else if (color.Contains("f9a722"))
                {
                    SetTheme(Resource.Style.Chatththemef9a722);
                }
                else if (color.Contains("008484"))
                {
                    SetTheme(Resource.Style.Chatththeme008484);
                }
                else if (color.Contains("5462a5"))
                {
                    SetTheme(Resource.Style.Chatththeme5462a5);
                }
                else if (color.Contains("fc9cde"))
                {
                    SetTheme(Resource.Style.Chatththemefc9cde);
                }
                else if (color.Contains("fc9cde"))
                {
                    SetTheme(Resource.Style.Chatththemefc9cde);
                }
                else if (color.Contains("51bcbc"))
                {
                    SetTheme(Resource.Style.Chatththeme51bcbc);
                }
                else if (color.Contains("c9605e"))
                {
                    SetTheme(Resource.Style.Chatththemec9605e);
                }
                else if (color.Contains("01a5a5"))
                {
                    SetTheme(Resource.Style.Chatththeme01a5a5);
                }
                else if (color.Contains("056bba"))
                {
                    SetTheme(Resource.Style.Chatththeme056bba);
                }
                else
                {
                    //Default Color >> AppSettings.MainColor
                    SetTheme(Resource.Style.Chatththemedefault);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetWallpaper()
        {
            try
            {
                string path = MainSettings.SharedData?.GetString("Wallpaper_key", string.Empty);
                if (!string.IsNullOrEmpty(path))
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(path);
                    if (type == "Image")
                        RootView.Background = Drawable.CreateFromPath(path);
                    else if (path.Contains("#"))
                        RootView.Background = new ColorDrawable(Color.ParseColor(path));
                }
                else
                {
                    RootView.Background = AppSettings.SetTabDarkTheme ? new ColorDrawable(Color.ParseColor("#282828")) : new ColorDrawable(Color.ParseColor("#F8F8F7"));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenDialogGallery(string typeImage)
        {
            try
            {
                PermissionsType = typeImage;
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder(UserId);

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage + "/" + UserId, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
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
                        Methods.Path.Chack_MyFolder(UserId);

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage + "/" + UserId, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
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

        private class XamarinRecyclerViewOnScrollListener : RecyclerView.OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            private readonly Holders.MsgPreCachingLayoutManager LayoutManager;
            //private readonly SwipeRefreshLayout SwipeRefreshLayout;
            private readonly FloatingActionButton FabScrollDown;
            private static readonly int HideThreshold = 20;
            private int ScrolledDistance;
            private bool ControlsVisible = true;

            public XamarinRecyclerViewOnScrollListener(Holders.MsgPreCachingLayoutManager layoutManager, FloatingActionButton fabScrollDown, SwipeRefreshLayout swipeRefreshLayout)
            {
                LayoutManager = layoutManager;
                FabScrollDown = fabScrollDown;
                Console.WriteLine(swipeRefreshLayout);
                //SwipeRefreshLayout = swipeRefreshLayout;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                try
                {
                    base.OnScrolled(recyclerView, dx, dy);

                    var visibleItemCount = recyclerView.ChildCount;
                    var totalItemCount = recyclerView.GetAdapter().ItemCount;

                    if (ScrolledDistance > HideThreshold && ControlsVisible)
                    {
                        FabScrollDown.Visibility = ViewStates.Gone;
                        ControlsVisible = false;
                        ScrolledDistance = 0;
                    }
                    else if (ScrolledDistance < -HideThreshold && !ControlsVisible)
                    {
                        FabScrollDown.Visibility = ViewStates.Visible;
                        ControlsVisible = true;
                        ScrolledDistance = 0;
                    }

                    if (ControlsVisible && dy > 0 || !ControlsVisible && dy < 0)
                    {
                        ScrolledDistance += dy;
                    }

                    var pastVisibleItems = LayoutManager.FindFirstVisibleItemPosition();
                    if (pastVisibleItems == 0 && visibleItemCount != totalItemCount)
                    {
                        //Load More  from API Request
                        LoadMoreEvent?.Invoke(this, null);
                        //Start Load More messages From Database
                    }
                    else
                    {
                        //if (SwipeRefreshLayout.Refreshing)
                        //{
                        //    SwipeRefreshLayout.Refreshing = false;
                        //    SwipeRefreshLayout.Enabled = false;

                        //}
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

        }

        private class MyTextWatcher : Java.Lang.Object, ITextWatcher
        {
            private readonly ChatWindowActivity ChatWindow;
            public MyTextWatcher(ChatWindowActivity chatWindow)
            {
                ChatWindow = chatWindow;
            }
            public void AfterTextChanged(IEditable s)
            {
                ChatWindow.EmojIconEditTextViewOnTextChanged();
            }

            public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
            {

            }

            public void OnTextChanged(ICharSequence s, int start, int before, int count)
            {

            }
        }

        private void HideKeyboard()
        {
            try
            {
                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(CurrentFocus?.WindowToken, HideSoftInputFlags.None);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ShowReplyUi(int position)
        {
            try
            {
                if (position > -1)
                {
                    SelectedItemPositions = MAdapter.GetItem(position);
                    ReplyItems();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CloseReplyUi()
        {
            try
            {
                if (RepliedMessageView.Visibility == ViewStates.Visible)
                {
                    Animation animation = new TranslateAnimation(0, 0, 0, RepliedMessageView.Top + RepliedMessageView.Height);
                    animation.Duration = 300;
                    animation.AnimationEnd += (o, args) =>
                    {
                        try
                        {
                            RepliedMessageView.Visibility = ViewStates.Gone;
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    };
                    RepliedMessageView.StartAnimation(animation);
                    SelectedItemPositions = null;
                    ReplyId = "0";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetOneSignalNotification()
        {
            try
            {
                var dataNotifier = Intent?.GetStringExtra("Notifier") ?? "Data not available";
                if (dataNotifier != "Data not available" && !string.IsNullOrEmpty(dataNotifier))
                {
                    Notifier = dataNotifier;
                    if (Notifier == "Notifier")
                    {
                        string dataApp = Intent?.GetStringExtra("App") ?? "";
                        if (dataApp == "Timeline")
                        {
                            string name = Intent?.GetStringExtra("Name");
                            UserId = Intent?.GetStringExtra("UserID");
                            string avatar = Intent?.GetStringExtra("Avatar");

                            //string time = Intent?.GetStringExtra("Time");
                            //LastSeen = Intent?.GetStringExtra("LastSeen") ?? "off";

                            ActionBarTitle.Text = name; // user name
                            SayHiToTextView.Text = name;

                            GlideImageLoader.LoadImage(this, avatar, UserChatProfile, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                            if (TypeChat == "SendMsgProduct")
                            {
                                //intent.GetStringExtra("ProductId", itemObject.LastMessage.LastMessageClass.ProductId);
                                //intent.GetStringExtra("ProductClass", JsonConvert.SerializeObject(itemObject.LastMessage.LastMessageClass.Product?.ProductClass));

                                string productId = Intent?.GetStringExtra("ProductId");

                                if (!Methods.CheckConnectivity())
                                {
                                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                                    return;
                                }

                                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                var time = unixTimestamp.ToString();

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.SendMessageAsync(UserId, time, "", "", "", "", "", "", productId) });
                                //ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_MessageSentSuccessfully),ToastLength.Short);
                            }
                        }

                        //Get Data Profile API
                        Task.Factory.StartNew(StartApiService);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}