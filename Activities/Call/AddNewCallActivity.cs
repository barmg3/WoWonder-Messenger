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
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Newtonsoft.Json;
using WoWonder.Activities.Base;
using WoWonder.Activities.Call.Adapters;
using WoWonder.Frameworks.Agora;
using WoWonder.Frameworks.Twilio;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Call
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AddNewCallActivity : BaseActivity
    {
        #region Variables Basic

        private AddNewCallAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private AdView MAdView;

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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_Select_contact);

                    toolbar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(toolbar);
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
                MAdapter = new AddNewCallAdapter(this)
                {
                    UserList = new ObservableCollection<UserDataObject>(),
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
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
                    MAdapter.AudioCallClick += CallAdapter_OnAudioCallClick;
                    MAdapter.VideoCallClick += CallAdapter_OnVideoCallClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    MAdapter.AudioCallClick -= CallAdapter_OnAudioCallClick;
                    MAdapter.VideoCallClick -= CallAdapter_OnVideoCallClick;
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

        private void CallAdapter_OnVideoCallClick(object sender, AddNewCallAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        Intent intentVideoCall = new Intent(this, typeof(TwilioVideoCallActivity));
                        switch (AppSettings.UseLibrary)
                        {
                            case SystemCall.Agora:
                                intentVideoCall = new Intent(this, typeof(AgoraVideoCallActivity));
                                intentVideoCall.PutExtra("type", "Agora_video_calling_start");
                                break;
                            case SystemCall.Twilio:
                                intentVideoCall = new Intent(this, typeof(TwilioVideoCallActivity));
                                intentVideoCall.PutExtra("type", "Twilio_video_calling_start");
                                break;
                        }

                        var callUserObject = new CallUserObject
                        {
                            UserId = item.UserId,
                            Avatar = item.Avatar,
                            Name = item.Name,
                            Data = new CallUserObject.DataCallUser()
                        };
                        intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));

                        StartActivity(intentVideoCall);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CallAdapter_OnAudioCallClick(object sender, AddNewCallAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        Intent intentVideoCall = new Intent(this, typeof(TwilioVideoCallActivity));
                        switch (AppSettings.UseLibrary)
                        {
                            case SystemCall.Agora:
                                intentVideoCall = new Intent(this, typeof(AgoraAudioCallActivity));
                                intentVideoCall.PutExtra("type", "Agora_audio_calling_start");
                                break;
                            case SystemCall.Twilio:
                                intentVideoCall = new Intent(this, typeof(TwilioAudioCallActivity));
                                intentVideoCall.PutExtra("type", "Twilio_audio_calling_start");
                                break;
                        }

                        var callUserObject = new CallUserObject
                        {
                            UserId = item.UserId,
                            Avatar = item.Avatar,
                            Name = item.Name,
                            Data = new CallUserObject.DataCallUser()
                        };
                        intentVideoCall.PutExtra("callUserObject", JsonConvert.SerializeObject(callUserObject));

                        StartActivity(intentVideoCall);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


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
                if (item != null && !string.IsNullOrEmpty(item.UserId) && !MainScrollEvent.IsLoading)
                    Task.Factory.StartNew(StartApiService);
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

                MAdapter.UserList = new ObservableCollection<UserDataObject>(userList);
                if (MAdapter.UserList.Count > 0)
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
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadContactsAsync });
        }

        private async Task LoadContactsAsync()
        {
            if (MainScrollEvent.IsLoading)
                return;

            var lastIdUser = MAdapter.UserList.LastOrDefault()?.UserId ?? "0";
            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                var countList = MAdapter.UserList.Count;
                var (apiStatus, respond) = await RequestsAsync.Global.GetFriendsAsync(UserDetails.UserId, "following", "35", lastIdUser);
                if (apiStatus == 200)
                {
                    if (respond is GetFriendsObject result)
                    {
                        var respondList = result.DataFriends.Following.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.DataFriends.Following let check = MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    MAdapter.UserList.Add(item);
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

    }
}