using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.DefaultUser.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using SearchView = AndroidX.AppCompat.Widget.SearchView;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", Exported = false, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    [IntentFilter(new[] { Intent.ActionSend, Intent.ActionSendMultiple }, Categories = new[] { Intent.CategoryDefault }, DataMimeTypes = new[] { "application/*", "image/*", "video/*", "audio/*", "text/*" })]
    public class ReceivingSharingActivity : BaseActivity
    {
        #region Variables Basic

        private ContactsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private AdView MAdView;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private Toolbar ToolBar;
        private SearchView SearchView;
        private string SearchText;
        private List<MessageDataExtra> AllItem = new List<MessageDataExtra>();

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                if (CheckAccess())
                {
                    InitTransferMessageContacts();

                    //Get Value And Set Toolbar
                    InitComponent();
                    InitToolbar();
                    SetRecyclerViewAdapters();

                    LoadContacts();
                }
                else
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_ErrorFileSharing), ToastLength.Long);
                    Finish();
                }
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
                MAdView?.Resume();
                base.OnResume();
                AddOrRemoveEvent(true);
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
                base.OnPause();
                AddOrRemoveEvent(false);
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
                MAdView?.Destroy();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Menu 

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.SearchGif_Menu, menu);

            IMenuItem item = menu.FindItem(Resource.Id.searchUserBar);
            SearchView searchItem = (SearchView)item.ActionView;

            SearchView = searchItem.JavaCast<SearchView>();
            SearchView.SetIconifiedByDefault(true);
            SearchView.QueryTextChange += SearchView_OnTextChange;
            SearchView.QueryTextSubmit += SearchView_OnTextSubmit;

            return base.OnCreateOptionsMenu(menu);
        }

        private void SearchView_OnTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs e)
        {
            try
            {
                SearchText = e.NewText;

                SearchView.ClearFocus();

                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                StartSearchRequest();

                //Hide keyboard programmatically in MonoDroid
                e.Handled = true;

                SearchView.ClearFocus();

                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager?.HideSoftInputFromWindow(ToolBar.WindowToken, 0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchView_OnTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            try
            {
                SearchText = e.NewText;
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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_Select_contact);
                    ToolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(ToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
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
                MAdapter = new ContactsAdapter(this, false, ContactsAdapter.TypeTextSecondary.About)
                {
                    UserList = new ObservableCollection<UserDataObject>(),
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MainScrollEvent.IsLoading = false;

                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.UserId))
                    Task.Factory.StartNew(StartApiService);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, ContactsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var itemMes = MAdapter.GetItem(position);
                    if (itemMes != null)
                    {
                        var userid = itemMes.UserId;

                        foreach (var item in AllItem)
                        {
                            if (item.Product?.ProductClass != null && !string.IsNullOrEmpty(item.ProductId) && item.ProductId != "0" && (item.ModelType == MessageModelType.RightProduct || item.ModelType == MessageModelType.LeftProduct))
                            {
                                string imageUrl = item.Product?.ProductClass?.Images[0]?.Image ?? "";
                                Console.WriteLine(imageUrl);
                                //Not have api send Product
                            }
                            else if (!string.IsNullOrEmpty(item.Stickers) && (item.ModelType == MessageModelType.RightGif || item.ModelType == MessageModelType.LeftGif) && item.Stickers.Contains(".gif"))
                            {
                                string imageUrl = "";
                                if (!string.IsNullOrEmpty(item.Stickers))
                                    imageUrl = item.Stickers;
                                else if (!string.IsNullOrEmpty(item.Media))
                                    imageUrl = item.Media;

                                Task.Factory.StartNew(() => { SendMess(userid, "", "", "", "", "", imageUrl); });
                            }
                            else switch (item.ModelType)
                                {
                                    case MessageModelType.RightText:
                                    case MessageModelType.LeftText:
                                        Task.Factory.StartNew(() => { SendMess(userid, item.Text); });
                                        break;
                                    case MessageModelType.RightImage:
                                    case MessageModelType.LeftImage:
                                        Task.Factory.StartNew(() => { SendMess(userid, "", "", item.Media); });
                                        break;
                                    case MessageModelType.RightMap:
                                    case MessageModelType.LeftMap:
                                        Task.Factory.StartNew(() => { SendMess(userid, "", "", "", "", "", "", item.Lat, item.Lng); });
                                        break;
                                    case MessageModelType.RightAudio:
                                    case MessageModelType.LeftAudio:
                                        Task.Factory.StartNew(() => { SendMess(userid, "", "", item.Media); });
                                        break;
                                    case MessageModelType.RightContact:
                                    case MessageModelType.LeftContact:
                                        {
                                            if (string.IsNullOrEmpty(item.ContactName) && string.IsNullOrEmpty(item.ContactName) && item.Text.Contains("{&quot;Key&quot;") || item.Text.Contains("{key:") || item.Text.Contains("{key:^qu") || item.Text.Contains("{^key:^qu") || item.Text.Contains("{Key:") || item.Text.Contains("&quot;"))
                                            {
                                                string[] stringSeparators = { "," };
                                                var name = item.Text.Split(stringSeparators, StringSplitOptions.None);
                                                var stringName = name[0].Replace("{key:", "").Replace("{Key:", "").Replace("Value:", "");
                                                var stringNumber = name[1].Replace("{key:", "").Replace("{Key:", "").Replace("Value:", "");
                                                item.ContactName = stringName;
                                                item.ContactNumber = stringNumber;
                                            }

                                            var dictionary = new Dictionary<string, string>();
                                            if (!dictionary.ContainsKey(item.ContactName))
                                                dictionary.Add(item.ContactName, item.ContactNumber);

                                            string dataContact = JsonConvert.SerializeObject(dictionary.ToArray().FirstOrDefault(a => a.Key == item.ContactName));

                                            Task.Factory.StartNew(() => { SendMess(userid, dataContact, "1"); });
                                            break;
                                        }
                                    case MessageModelType.RightVideo:
                                    case MessageModelType.LeftVideo:
                                        Task.Factory.StartNew(() => { SendMess(userid, "", "", item.Media); });
                                        break;
                                    case MessageModelType.RightSticker:
                                    case MessageModelType.LeftSticker:
                                        {
                                            var fileNameWithoutExtension = item.Media.Split('/').Last().Split('.').First().Replace("sticker", "");
                                            Task.Factory.StartNew(() => { SendMess(userid, "", "", "", item.Media, "sticker" + fileNameWithoutExtension); });
                                            break;
                                        }
                                    case MessageModelType.RightFile:
                                    case MessageModelType.LeftFile:
                                        Task.Factory.StartNew(() => { SendMess(userid, "", "", item.Media); });
                                        break;
                                }
                        }

                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_MessagesSent), ToastLength.Short);
                        Finish();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Contacts 

        private void LoadContacts()
        {
            try
            {
                if (ListUtils.UserList.Count > 0)
                {
                    foreach (var item in ListUtils.UserList)
                    {
                        MAdapter.UserList.Add(new UserDataObject
                        {
                            UserId = item.UserId,
                            Username = item.Username,
                            Email = item.Email,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Avatar = item.Avatar,
                            Cover = item.Cover,
                            BackgroundImage = item.BackgroundImage,
                            RelationshipId = item.RelationshipId,
                            Address = item.Address,
                            Working = item.Working,
                            Gender = item.Gender,
                            Facebook = item.Facebook,
                            Google = item.Google,
                            Twitter = item.Twitter,
                            Linkedin = item.Linkedin,
                            Website = item.Website,
                            Instagram = item.Instagram,
                            WebDeviceId = item.WebDeviceId,
                            Language = item.Language,
                            IpAddress = item.IpAddress,
                            PhoneNumber = item.PhoneNumber,
                            Timezone = item.Timezone,
                            Lat = item.Lat,
                            Lng = item.Lng,
                            About = item.About,
                            Birthday = item.Birthday,
                            Registered = item.Registered,
                            Lastseen = item.Lastseen,
                            LastLocationUpdate = item.LastLocationUpdate,
                            Balance = item.Balance,
                            Verified = item.Verified,
                            Status = item.Status,
                            Active = item.Active,
                            Admin = item.Admin,
                            IsPro = item.IsPro,
                            ProType = item.ProType,
                            School = item.School,
                            Name = item.Name,
                            AndroidMDeviceId = item.AndroidMDeviceId,
                            ECommented = item.ECommented,
                            AndroidNDeviceId = item.AndroidMDeviceId,
                            BirthPrivacy = item.BirthPrivacy,
                            ConfirmFollowers = item.ConfirmFollowers,
                            CountryId = item.CountryId,
                            EAccepted = item.EAccepted,
                            EFollowed = item.EFollowed,
                            EJoinedGroup = item.EJoinedGroup,
                            ELastNotif = item.ELastNotif,
                            ELiked = item.ELiked,
                            ELikedPage = item.ELikedPage,
                            EMentioned = item.EMentioned,
                            EProfileWallPost = item.EProfileWallPost,
                            ESentmeMsg = item.ESentmeMsg,
                            EShared = item.EShared,
                            EVisited = item.EVisited,
                            EWondered = item.EWondered,
                            EmailNotification = item.EmailNotification,
                            FollowPrivacy = item.FollowPrivacy,
                            FriendPrivacy = item.FriendPrivacy,
                            InfoFile = item.InfoFile,
                            IosMDeviceId = item.IosMDeviceId,
                            IosNDeviceId = item.IosNDeviceId,
                            LastAvatarMod = item.LastAvatarMod,
                            LastCoverMod = item.LastCoverMod,
                            LastDataUpdate = item.LastDataUpdate,
                            LastFollowId = item.LastFollowId,
                            LastseenStatus = item.LastseenStatus,
                            LastseenUnixTime = item.LastseenUnixTime,
                            MessagePrivacy = item.MessagePrivacy,
                            NewEmail = item.NewEmail,
                            NewPhone = item.NewPhone,
                            NotificationSettings = item.NotificationSettings,
                            NotificationsSound = item.NotificationsSound,
                            OrderPostsBy = item.OrderPostsBy,
                            PaypalEmail = item.PaypalEmail,
                            PostPrivacy = item.PostPrivacy,
                            Referrer = item.Referrer,
                            ShareMyData = item.ShareMyData,
                            ShareMyLocation = item.ShareMyLocation,
                            ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                            TwoFactor = item.TwoFactor,
                            TwoFactorVerified = item.TwoFactorVerified,
                            Url = item.Url,
                            VisitPrivacy = item.VisitPrivacy,
                            Vk = item.Vk,
                            Wallet = item.Wallet,
                            WorkingLink = item.WorkingLink,
                            Youtube = item.Youtube,
                            City = item.City,
                            Points = item.Points,
                            DailyPoints = item.DailyPoints,
                            PointDayExpire = item.PointDayExpire,
                            State = item.State,
                            Zip = item.Zip,
                            Details = new DetailsUnion { DetailsClass = new Details() },
                            Selected = false,
                        });
                    }
                }

                var sqlEntity = new SqLiteDatabase();
                var userList = sqlEntity.Get_MyContact().Except(MAdapter.UserList);

                MAdapter.UserList = new ObservableCollection<UserDataObject>(userList);
                MAdapter.NotifyDataSetChanged();



                Task.Factory.StartNew(StartApiService);
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
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadContactsAsync });
            }
        }

        private async Task LoadContactsAsync()
        {
            if (MainScrollEvent.IsLoading)
                return;

            MainScrollEvent.IsLoading = true;

            var lastIdUser = MAdapter.UserList.LastOrDefault()?.UserId ?? "0";
            if (Methods.CheckConnectivity())
            {
                var countList = MAdapter.UserList.Count;
                var (apiStatus, respond) = await RequestsAsync.Global.GetFriendsAsync(UserDetails.UserId, "following", "35", lastIdUser);
                if (apiStatus != 200 || respond is not GetFriendsObject result || result.DataFriends == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.DataFriends.Following.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in result.DataFriends.Following)
                            {
                                var check = MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId);
                                if (check == null)
                                {
                                    MAdapter.UserList.Add(item);
                                }
                            }
                            RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.UserList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.UserList = new ObservableCollection<UserDataObject>(result.DataFriends.Following);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                    }
                }

                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.UserList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Insert_Or_Replace_MyContactTable(MAdapter.UserList);

                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoUsers);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Search 

        private void Search()
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (Methods.CheckConnectivity())
                    {
                        MAdapter?.UserList?.Clear();
                        MAdapter?.NotifyDataSetChanged();

                        if (!SwipeRefreshLayout.Refreshing)
                            SwipeRefreshLayout.Refreshing = true;

                        if (EmptyStateLayout != null)
                            EmptyStateLayout.Visibility = ViewStates.Gone;

                        StartSearchRequest();
                    }
                }
                else
                {
                    Inflated ??= EmptyStateLayout?.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    if (EmptyStateLayout != null)
                    {
                        EmptyStateLayout.Visibility = ViewStates.Visible;
                    }

                    if (SwipeRefreshLayout.Refreshing)
                        SwipeRefreshLayout.Refreshing = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void StartSearchRequest()
        {
            if (MainScrollEvent.IsLoading)
                return;

            MainScrollEvent.IsLoading = true;

            int countUserList = MAdapter.UserList.Count;

            var dictionary = new Dictionary<string, string>
                {
                    {"user_id", UserDetails.UserId},
                    {"limit", "30"},
                    {"user_offset", "0"},
                    {"gender", UserDetails.SearchGender},
                    {"search_key", SearchText},
                };

            var (apiStatus, respond) = await RequestsAsync.Global.SearchAsync(dictionary);
            if (apiStatus == 200)
            {
                if (respond is GetSearchObject result)
                {
                    var respondUserList = result.Users?.Count;
                    if (respondUserList > 0)
                    {
                        if (countUserList > 0)
                        {
                            foreach (var item in result.Users)
                            {
                                var check = MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId);
                                if (check == null)
                                {
                                    MAdapter.UserList.Add(item);
                                }
                            }

                            RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countUserList, MAdapter.UserList.Count - countUserList); });
                        }
                        else
                        {
                            MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Users);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                    }
                }
            }
            else Methods.DisplayReportResult(this, respond);

            RunOnUiThread(ShowEmptyPage);
        }

        //No Internet Connection 
        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                SearchText = "a";
                Search();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Transfer Message Contacts


        private bool CheckAccess()
        {
            try
            {
                InitializeWoWonder.Initialize(AppSettings.TripleDesAppServiceProvider, PackageName, AppSettings.TurnTrustFailureOnWebException);

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                dbDatabase.CheckTablesStatus();

                var login = dbDatabase.Get_data_Login_Credentials();
                if (login != null && !string.IsNullOrEmpty(UserDetails.AccessToken) && !string.IsNullOrEmpty(UserDetails.UserId))
                {
                    dbDatabase.GetSettings();

                    return true;
                }


                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private void InitTransferMessageContacts()
        {
            try
            {
                // Get intent, action and MIME type
                Intent intent = Intent;
                string action = intent.Action;
                string type = intent.Type;

                switch (action)
                {
                    case Intent.ActionSend when type != null && Intent.Extras != null:
                        {
                            if ("text/plain".Equals(type))
                            {
                                HandleSendText(intent); // Handle text being sent
                            }
                            if ("text/x-vcard".Equals(type))
                            {
                                HandleSendTextCard(intent); // Handle text being sent
                            }
                            else if (type.StartsWith("image/") || type.StartsWith("video/") || type.StartsWith("application/") || type.StartsWith("audio/"))
                            {
                                HandleSendFile(intent); // Handle single file being sent
                            }

                            break;
                        }
                    case Intent.ActionSendMultiple when type != null:
                        HandleSendMultipleFiles(intent); // Handle multiple images being sent
                        break;
                    default:
                        // Handle other intents, such as being started from the home screen
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Update UI to reflect text being shared 
        /// </summary>
        /// <param name="intent"></param>
        private void HandleSendText(Intent intent)
        {
            try
            {
                var sharedText = intent.GetStringExtra(Intent.ExtraText);
                if (sharedText != null)
                {
                    AllItem = new List<MessageDataExtra>();

                    string timeNow = DateTime.Now.ToShortTimeString();
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time2 = Convert.ToString(unixTimestamp);

                    string replacement = Regex.Replace(sharedText, @"\t|\n|\r", "");

                    MessageDataExtra m1 = new MessageDataExtra
                    {
                        Id = time2,
                        FromId = UserDetails.UserId,
                        Text = replacement,
                        Position = "right",
                        ModelType = MessageModelType.RightText,
                        TimeText = DateTime.Now.ToShortTimeString(),
                    };
                    AllItem.Add(m1);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleSendTextCard(Intent intent)
        {
            try
            {
                var sharedText = intent.GetStringExtra(Intent.ExtraText);
                if (sharedText != null)
                {
                    AllItem = new List<MessageDataExtra>();

                    string timeNow = DateTime.Now.ToShortTimeString();
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time2 = Convert.ToString(unixTimestamp);

                    var contact = Methods.PhoneContactManager.Get_ContactInfoBy_Id(intent.Data.LastPathSegment);
                    if (contact != null)
                    {
                        var name = contact.UserDisplayName;
                        var phone = contact.PhoneNumber;

                        MessageDataExtra m1 = new MessageDataExtra
                        {
                            Id = time2,
                            FromId = UserDetails.UserId,
                            ContactName = name,
                            ContactNumber = phone,
                            TimeText = timeNow,
                            Position = "right",
                            ModelType = MessageModelType.RightContact
                        };
                        AllItem.Add(m1);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void HandleSendFile(Intent intent)
        {
            try
            {
                string timeNow = DateTime.Now.ToShortTimeString();
                var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string time2 = Convert.ToString(unixTimestamp);

                var uri = intent.GetParcelableExtra(Intent.ExtraStream);
                if (uri != null && uri is Android.Net.Uri fileUri)
                {
                    AllItem = new List<MessageDataExtra>();

                    // Update UI to reflect image being shared
                    var filePath = Methods.AttachmentFiles.GetActualPathFromFile(this, fileUri);

                    var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filePath);
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

                    var type = Methods.AttachmentFiles.Check_FileExtension(filePath);
                    switch (type)
                    {
                        case "Image" when filePath.Contains(".gif"):
                            {
                                MessageDataExtra m1 = new MessageDataExtra
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    Media = filePath,
                                    MediaFileName = filePath,
                                    Position = "right",
                                    ModelType = MessageModelType.RightGif,
                                    TimeText = timeNow,
                                    Stickers = filePath,
                                };
                                AllItem.Add(m1);
                                break;
                            }
                        case "Image":
                            {
                                MessageDataExtra m1 = new MessageDataExtra
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    Media = filePath,
                                    Position = "right",
                                    ModelType = MessageModelType.RightImage,
                                    TimeText = timeNow,
                                };
                                AllItem.Add(m1);
                                break;
                            }
                        case "Video":
                            {
                                MessageDataExtra m1 = new MessageDataExtra
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    Media = filePath,
                                    Position = "right",
                                    ModelType = MessageModelType.RightVideo,
                                    TimeText = timeNow
                                };
                                AllItem.Add(m1);
                                break;
                            }
                        case "File":
                            {
                                string totalSize = Methods.FunString.Format_byte_size(filePath);
                                MessageDataExtra m1 = new MessageDataExtra
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    Media = filePath,
                                    FileSize = totalSize,
                                    TimeText = timeNow,
                                    Position = "right",
                                    ModelType = MessageModelType.RightFile
                                };
                                AllItem.Add(m1);
                                break;
                            }
                        case "Audio":
                            {
                                string totalSize = Methods.FunString.Format_byte_size(filePath);
                                MessageDataExtra m1 = new MessageDataExtra
                                {
                                    Id = time2,
                                    FromId = UserDetails.UserId,
                                    Media = filePath,
                                    FileSize = totalSize,
                                    Position = "right",
                                    TimeText = GetText(Resource.String.Lbl_Uploading),
                                    MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(Methods.AudioRecorderAndPlayer.Get_MediaFileDuration(filePath)),
                                    ModelType = MessageModelType.RightAudio
                                };
                                AllItem.Add(m1);
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

        /// <summary>
        ///  Update UI to reflect multiple file being shared
        /// </summary>
        /// <param name="intent"></param>
        private async void HandleSendMultipleFiles(Intent intent)
        {
            try
            {
                var uris = intent.GetParcelableArrayListExtra(Intent.ExtraStream);
                if (uris != null)
                {
                    AllItem = new List<MessageDataExtra>();

                    foreach (var uri in uris)
                    {
                        string timeNow = DateTime.Now.ToShortTimeString();
                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string time2 = Convert.ToString(unixTimestamp);

                        if (uri != null && uri is Android.Net.Uri fileUri)
                        {
                            // Update UI to reflect image being shared
                            var filePath = Methods.AttachmentFiles.GetActualPathFromFile(this, fileUri);
                            var (check, info) = await WoWonderTools.CheckMimeTypesWithServer(filePath);
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

                            var type = Methods.AttachmentFiles.Check_FileExtension(filePath);
                            switch (type)
                            {
                                case "Image" when filePath.Contains(".gif"):
                                    {
                                        MessageDataExtra m1 = new MessageDataExtra
                                        {
                                            Id = time2,
                                            FromId = UserDetails.UserId,
                                            Media = filePath,
                                            MediaFileName = filePath,
                                            Position = "right",
                                            ModelType = MessageModelType.RightGif,
                                            TimeText = timeNow,
                                            Stickers = filePath,
                                        };
                                        AllItem.Add(m1);
                                        break;
                                    }
                                case "Image":
                                    {
                                        MessageDataExtra m1 = new MessageDataExtra
                                        {
                                            Id = time2,
                                            FromId = UserDetails.UserId,
                                            Media = filePath,
                                            Position = "right",
                                            ModelType = MessageModelType.RightImage,
                                            TimeText = timeNow,
                                        };
                                        AllItem.Add(m1);
                                        break;
                                    }
                                case "Video":
                                    {
                                        MessageDataExtra m1 = new MessageDataExtra
                                        {
                                            Id = time2,
                                            FromId = UserDetails.UserId,
                                            Media = filePath,
                                            Position = "right",
                                            ModelType = MessageModelType.RightVideo,
                                            TimeText = timeNow
                                        };
                                        AllItem.Add(m1);
                                        break;
                                    }
                                case "File":
                                    {
                                        string totalSize = Methods.FunString.Format_byte_size(filePath);
                                        MessageDataExtra m1 = new MessageDataExtra
                                        {
                                            Id = time2,
                                            FromId = UserDetails.UserId,
                                            Media = filePath,
                                            FileSize = totalSize,
                                            TimeText = timeNow,
                                            Position = "right",
                                            ModelType = MessageModelType.RightFile
                                        };
                                        AllItem.Add(m1);
                                        break;
                                    }
                                case "Audio":
                                    {
                                        string totalSize = Methods.FunString.Format_byte_size(filePath);
                                        MessageDataExtra m1 = new MessageDataExtra
                                        {
                                            Id = time2,
                                            FromId = UserDetails.UserId,
                                            Media = filePath,
                                            FileSize = totalSize,
                                            Position = "right",
                                            TimeText = GetText(Resource.String.Lbl_Uploading),
                                            MediaDuration = Methods.AudioRecorderAndPlayer.GetTimeString(Methods.AudioRecorderAndPlayer.Get_MediaFileDuration(filePath)),
                                            ModelType = MessageModelType.RightAudio
                                        };
                                        AllItem.Add(m1);
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private async void SendMess(string userId = "", string text = "", string contact = "", string pathFile = "", string imageUrl = "", string stickerId = "", string gifUrl = "", string lat = "", string lng = "")
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
                    var (apiStatus, respond) = await RequestsAsync.Message.SendMessageAsync(userId, time2, text, contact, pathFile, imageUrl, stickerId, gifUrl, lat, lng);
                    if (apiStatus == 200)
                    {
                        if (respond is SendMessageObject result)
                        {
                            Console.WriteLine(result.MessageData);
                            MessageController.UpdateLastIdMessage(result);
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

    }
}