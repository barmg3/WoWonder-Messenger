using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.GroupChat.Adapter;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using SearchView = AndroidX.AppCompat.Widget.SearchView;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.GroupChat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MentionActivity : BaseActivity
    {
        #region Variables Basic

        public static MentionAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private TextView BtnAction;
        private AdView MAdView;
        private SearchView SearchView;
        private Toolbar ToolBar;
        private string SearchText = "";
        

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
                 
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters(); 

                LoadContacts();

                AdsGoogle.Ad_RewardedVideo(this);
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

            var item = menu.FindItem(Resource.Id.searchUserBar);
            SearchView searchItem = (SearchView)item?.ActionView;

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

                MAdapter.MentionList.Clear();
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
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                 
                BtnAction = FindViewById<TextView>(Resource.Id.toolbar_title);
                BtnAction.Visibility = ViewStates.Visible;

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
                    ToolBar.Title = GetText(Resource.String.Lbl_AddParticipants);

                    ToolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(ToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetHomeAsUpIndicator(AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.ic_action_right_arrow_color : Resource.Drawable.ic_action_left_arrow_color));

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
                MAdapter = new MentionAdapter(this)
                {
                    MentionList = new ObservableCollection<UserDataObject>(),
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
                    BtnAction.Click += BtnActionOnClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    BtnAction.Click -= BtnActionOnClick;
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
         
        private void MAdapterOnItemClick(object sender, MentionAdapterClickEventArgs e)
        {
            try
            {
                int position = e.Position;
                if (position <= -1) return;

                var item = MAdapter.GetItem(position);
                if (item == null) return;

                item.Selected = !item.Selected;
                MAdapter.NotifyItemChanged(e.Position);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MainScrollEvent.IsLoading = false;

                MAdapter.MentionList.Clear();
                MAdapter.NotifyDataSetChanged();

                Task.Factory.StartNew(() => StartApiService());
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
                var item = MAdapter.MentionList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.UserId) && !MainScrollEvent.IsLoading)
                    LoadContactsAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnActionOnClick(object sender, EventArgs e)
        {
            try
            {  
                List<UserDataObject> listUsers = MAdapter.MentionList.Where(a => a.Selected).ToList(); 
                var resultIntent = new Intent();
                resultIntent.PutExtra("Users", JsonConvert.SerializeObject(listUsers)); 
                SetResult(Result.Ok, resultIntent);
                Finish();
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
                var sqlEntity = new SqLiteDatabase();
                var userList = sqlEntity.Get_MyContact();

                MAdapter.MentionList = new ObservableCollection<UserDataObject>(userList);
                if (MAdapter.MentionList.Count > 0)
                {
                    MAdapter.NotifyDataSetChanged();
                    
                }
                else
                {
                    SwipeRefreshLayout.Refreshing = true;
                }

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
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadContactsAsync });
        }

        private async Task LoadContactsAsync()
        {
            if (MainScrollEvent.IsLoading)
                return;

            var lastIdUser = MAdapter.MentionList.LastOrDefault()?.UserId ?? "0";
            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                var countList = MAdapter.MentionList.Count;
                var(apiStatus, respond) = await RequestsAsync.Global.GetFriendsAsync(UserDetails.UserId, "following", "35", lastIdUser);
                if (apiStatus == 200)
                {
                    if (respond is GetFriendsObject result)
                    {
                        var respondList = result.DataFriends.Following.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.DataFriends.Following let check = MAdapter.MentionList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    MAdapter.MentionList.Add(item);
                                }

                                RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.MentionList.Count - countList); });
                            }
                            else
                            {
                                MAdapter.MentionList = new ObservableCollection<UserDataObject>(result.DataFriends.Following);
                                RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MAdapter.MentionList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users),ToastLength.Short);
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respond);

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

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
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

                if (MAdapter.MentionList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Insert_Or_Replace_MyContactTable(MAdapter.MentionList);
                    
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

        public void Search()
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (Methods.CheckConnectivity())
                    {
                        MAdapter?.MentionList?.Clear();
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

        public async void StartSearchRequest()
        {
            if (MainScrollEvent.IsLoading)
                return;

            MainScrollEvent.IsLoading = true;

            int countUserList = MAdapter.MentionList.Count;

            var dictionary = new Dictionary<string, string>
            {
                {"user_id", UserDetails.UserId},
                {"limit", "30"},
                {"user_offset", "0"},
                {"gender", UserDetails.SearchGender},
                {"search_key", SearchText},
            };

            var(apiStatus, respond) = await RequestsAsync.Global.SearchAsync(dictionary);
            if (apiStatus == 200)
            {
                if (respond is GetSearchObject result)
                {
                    var respondUserList = result.Users?.Count;
                    if (respondUserList > 0)
                    {
                        if (countUserList > 0)
                        {
                            foreach (var item in from item in result.Users let check = MAdapter.MentionList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                MAdapter.MentionList.Add(item);
                            }

                            RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countUserList, MAdapter.MentionList.Count - countUserList); });
                        }
                        else
                        {
                            MAdapter.MentionList = new ObservableCollection<UserDataObject>(result.Users);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.MentionList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users),ToastLength.Short);
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
    }
}