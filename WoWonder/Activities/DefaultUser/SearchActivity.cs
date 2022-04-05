using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Google.Android.Material.FloatingActionButton;
using WoWonder.Activities.Base;
using WoWonder.Activities.DefaultUser.Adapters;
using WoWonder.Activities.DialogUserFragment;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.DefaultUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SearchActivity : BaseActivity, TextView.IOnEditorActionListener
    {
        #region Variables Basic

        public ContactsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private Toolbar ToolBar;
        private AutoCompleteTextView SearchView; 
        private FloatingActionButton FloatingActionButtonView;
        private ProgressBar ProgressBarLoader;
        private string DataKey, SearchText = "a", OffsetUser;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Window.SetSoftInputMode(SoftInput.AdjustNothing);
                 
                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.SearchLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                DataKey = Intent?.GetStringExtra("Key") ?? "Data not available";
                if (DataKey != "Data not available" && !string.IsNullOrEmpty(DataKey))
                {
                    SearchText = DataKey;

                    if (SearchText == "Random")
                    {
                        SearchText = "a";
                    }
                    else
                    { 
                        SearchView?.ClearFocus(); 
                    }
                }

                //Close keyboard
                InputMethodManager inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                if (inputManager != null && inputManager.IsActive)
                {
                    if (ToolBar != null)
                    {
                        inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                        inputManager?.HideSoftInputFromWindow(ToolBar.WindowToken, 0);
                    } 
                }

                SearchView.ClearFocus();

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
                SearchView?.ClearFocus();

                base.OnPause();
                AddOrRemoveEvent(false);
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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                ProgressBarLoader = (ProgressBar)FindViewById(Resource.Id.sectionProgress);
                ProgressBarLoader.Visibility = ViewStates.Gone;

                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
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
                    ToolBar.Title = "";
                    ToolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(ToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetHomeAsUpIndicator(AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.ic_action_right_arrow_color : Resource.Drawable.ic_action_left_arrow_color));
                }

                SearchView = FindViewById<AutoCompleteTextView>(Resource.Id.searchBox);
                SearchView?.SetOnEditorActionListener(this);
                //SearchView.SetQuery("", false);
                //SearchView.SetIconifiedByDefault(false);
                //SearchView.OnActionViewExpanded();
                //SearchView.Iconified = false;
                //SearchView.QueryTextChange += SearchViewOnQueryTextChange;
                //SearchView.QueryTextSubmit += SearchViewOnQueryTextSubmit;
                SearchView?.ClearFocus();

                //Change text colors 
                SearchView?.SetHintTextColor(Color.ParseColor(AppSettings.MainColor));
                SearchView?.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                //Remove Icon Search
                ImageView searchViewIcon = (ImageView)SearchView.FindViewById(Resource.Id.search_mag_icon);
                ViewGroup linearLayoutSearchView = (ViewGroup)searchViewIcon?.Parent;
                linearLayoutSearchView?.RemoveView(searchViewIcon);

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
                MAdapter = new ContactsAdapter(this, true, ContactsAdapter.TypeTextSecondary.LastSeen) { UserList = new ObservableCollection<UserDataObject>() }; 
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

                Inflated ??= EmptyStateLayout.Inflate();

                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += TryAgainButton_Click;
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
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                    MAdapter.FollowButtonItemClick += MAdapter.OnFollowButtonItemClick;
                    FloatingActionButtonView.Click += FloatingActionButtonViewOnClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    MAdapter.ItemLongClick -= MAdapterOnItemLongClick;
                    MAdapter.FollowButtonItemClick -= MAdapter.OnFollowButtonItemClick;
                    FloatingActionButtonView.Click -= FloatingActionButtonViewOnClick;
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
                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                Search();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        private void MAdapterOnItemLongClick(object sender, ContactsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        //Pull up dialog 
                        DialogUser userDialog = new DialogUser(this,item.UserId, item);
                        userDialog.Show(SupportFragmentManager, userDialog.Tag);
                        userDialog.OnUserUpComplete += UserDialogOnOnUserUpComplete; 
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UserDialogOnOnUserUpComplete(object sender, DialogUser.OnUserUpEventArgs e)
        {
            try
            {
                Thread th = new Thread(ActLikeARequest);
                th.Start();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ActLikeARequest()
        {
            int x = Resource.Animation.slide_right;
            Console.WriteLine(x);
        }

        private void MAdapterOnItemClick(object sender, ContactsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        // WoWonderTools.OpenProfile(this, item.UserId, item);
                        //Pull up dialog 
                        DialogUser userDialog = new DialogUser(this, item.UserId, item);
                        userDialog.Show(SupportFragmentManager, userDialog.Tag);
                        userDialog.OnUserUpComplete += UserDialogOnOnUserUpComplete;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.UserId) && !MainScrollEvent.IsLoading)
                {
                    OffsetUser = item.UserId;
                    Task.Factory.StartNew(() => StartApiService());
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Filter
        private void FloatingActionButtonViewOnClick(object sender, EventArgs e)
        {
            try
            {
                FilterSearchDialogFragment mFragment = new FilterSearchDialogFragment();
                mFragment.Show(SupportFragmentManager, mFragment.Tag);
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
                        MAdapter?.UserList?.Clear();
                        MAdapter?.NotifyDataSetChanged();
 
                        if (ProgressBarLoader != null)
                            ProgressBarLoader.Visibility = ViewStates.Visible;

                        if (EmptyStateLayout != null)
                            EmptyStateLayout.Visibility = ViewStates.Gone;

                        MainScrollEvent.IsLoading = false;

                        Task.Factory.StartNew(() => StartApiService());
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

                    ProgressBarLoader.Visibility = ViewStates.Gone; 
                }
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
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
        }

        private async Task StartSearchRequest()
        {
            if (MainScrollEvent.IsLoading)
                return;

            MainScrollEvent.IsLoading = true;
            int countUserList = MAdapter.UserList.Count;
             
            var dictionary = new Dictionary<string, string>
            {
                {"user_id", UserDetails.UserId},
                {"limit", "15"},
                {"user_offset", OffsetUser},
                {"gender", UserDetails.SearchGender},
                {"search_key", SearchText},
                {"country", UserDetails.SearchCountry},
                {"status", UserDetails.SearchStatus},
                {"verified", UserDetails.SearchVerified},
                {"filterbyage", UserDetails.SearchFilterByAge},
                {"age_from", UserDetails.SearchAgeFrom},
                {"age_to", UserDetails.SearchAgeTo},
                {"image", UserDetails.SearchProfilePicture},
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
                            foreach (var item in from item in result.Users let check = MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                MAdapter.UserList.Add(item);
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
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_No_more_users),ToastLength.Short);
                    }
                }
            }
            else
            {
                Methods.DisplayReportResult(this, respond);
                MainScrollEvent.IsLoading = false;
            } 

            RunOnUiThread(ShowEmptyPage);
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;
                MainScrollEvent.IsLoading = false;
                ProgressBarLoader.Visibility = ViewStates.Gone;
             
                if (MAdapter.UserList.Count > 0)
                {
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                MainScrollEvent.IsLoading = false;
                ProgressBarLoader.Visibility = ViewStates.Gone;
                Methods.DisplayReportResultTrack(e);
            }
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

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchView.ClearFocus();
                MAdapter.UserList.Clear();
                MAdapter.NotifyDataSetChanged();

                OffsetUser = "0";

                if (string.IsNullOrEmpty(SearchText) || string.IsNullOrWhiteSpace(SearchText))
                {
                    SearchText = "a";
                }

                if (Methods.CheckConnectivity())
                {
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                    Search();
                }
                else
                {
                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        public bool OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
        {
            if (actionId == ImeAction.Search)
            {
                try
                {
                    SearchText = v.Text;

                    SearchView.ClearFocus();
                    v.ClearFocus();

                    MAdapter.UserList.Clear();
                    MAdapter.NotifyDataSetChanged();

                    OffsetUser = "0";

                    if (Methods.CheckConnectivity())
                    {
                        if (MAdapter.UserList.Count > 0)
                        {
                            MAdapter.UserList.Clear();
                            MAdapter.NotifyDataSetChanged();
                        }

                        ProgressBarLoader.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;

                        MainScrollEvent.IsLoading = false;

                        //Close keyboard
                        InputMethodManager inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                        if (inputManager != null && inputManager.IsActive)
                        {
                            if (ToolBar != null)
                            {
                                inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                                inputManager?.HideSoftInputFromWindow(ToolBar.WindowToken, 0);
                            }
                        }

                        Task.Factory.StartNew(() => StartApiService());
                    }
                    else
                    {
                        Inflated ??= EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                            x.EmptyStateButton.Click -= TryAgainButton_Click;
                        }

                        x.EmptyStateButton.Click += TryAgainButton_Click;
                        ProgressBarLoader.Visibility = ViewStates.Gone;
                        EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
                 
                return true;
            }

            return false;
        }
    }
}