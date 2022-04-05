using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.Content;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.Story;
using WoWonder.Activities.Story.Adapter;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Tab.Fragment
{ 
    public class LastStoriesFragment : AndroidX.Fragment.App.Fragment , MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        public StoryAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        private View Inflated;
        private ImageView ProfileUserImage;
        private RelativeLayout AddStoryLayout;
        public RelativeLayout AddNewLayout;
        private readonly TabbedMainActivity ContextActivity;
        public PublisherAdView PublisherAdView;

        #endregion

        #region General

        public LastStoriesFragment(TabbedMainActivity context)
        {
            ContextActivity = context;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TStoryFragmentLayout, container, false); 
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            } 
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                 
                InitComponent(view);
                SetRecyclerViewAdapters();

                StartApiService(); 
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                //Add Story
                AddNewLayout = view.FindViewById<RelativeLayout>(Resource.Id.addNewLayout);
                AddStoryLayout = view.FindViewById<RelativeLayout>(Resource.Id.rlMain);
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                ProfileUserImage = view.FindViewById<ImageView>(Resource.Id.ivUser);
                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                //Add story click
                AddStoryLayout.Click += AddStoryLayout_Click;

                PublisherAdView = view.FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                MAdapter = new StoryAdapter(Activity) { StoryList = new ObservableCollection<StoryDataObject>()};
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<StoryDataObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event
 
        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.StoryList.Clear();
                MAdapter.NotifyDataSetChanged();

                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, StoryAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                { 
                    Intent intent = new Intent(Context, typeof(StoryDetailsActivity));
                    intent.PutExtra("UserId", item.UserId);
                    intent.PutExtra("IndexItem", e.Position);
                    intent.PutExtra("StoriesCount", MAdapter.StoryList.Count);
                    intent.PutExtra("DataItem", JsonConvert.SerializeObject(MAdapter.StoryList));
                    Context.StartActivity(intent);

                    item.ProfileIndicator = AppSettings.StoryReadColor;
                    MAdapter.NotifyItemChanged(e.Position);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AddStoryLayout_Click(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(Activity.GetText(Resource.String.text));
                arrayAdapter.Add(GetText(Resource.String.image));

                if (WoWonderTools.CheckAllowedFileSharingInServer("Video"))
                    arrayAdapter.Add(GetText(Resource.String.video));

                dialogList.Title(GetText(Resource.String.Lbl_Addnewstory));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Story

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadStory });
        }

        private async Task LoadStory()
        {
            if (Methods.CheckConnectivity())
            {
                var(apiStatus, respond) = await RequestsAsync.Story.GetUserStoriesAsync();
                if (apiStatus == 200)
                {
                    try
                    {
                        if (respond is GetUserStoriesObject result)
                        {
                            foreach (var item in result.Stories)
                            {
                                var check = MAdapter.StoryList.FirstOrDefault(a => a.UserId == item.UserId);
                                if (check != null)
                                {
                                    foreach (var item2 in item.Stories)
                                    {
                                        item.DurationsList ??= new List<long>();

                                        //image and video
                                        string mediaFile = "";
                                        if (!item2.Thumbnail.Contains("avatar") && item2.Videos.Count == 0) mediaFile = item2.Thumbnail;
                                        else if (item2.Videos.Count > 0)
                                            mediaFile = item2.Videos[0].Filename;

                                        if (!string.IsNullOrEmpty(mediaFile))
                                        {
                                            var type = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                            if (type != "Video")
                                            {
                                                Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                                item.DurationsList.Add(AppSettings.StoryImageDuration * 1000); 
                                            }
                                            else
                                            {
                                                var fileName = mediaFile.Split('/').Last();
                                                mediaFile = WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile, "other");

                                                if (AppSettings.ShowFullVideo)
                                                {
                                                    var duration = WoWonderTools.GetDuration(mediaFile);
                                                    item.DurationsList.Add(Long.ParseLong(duration));
                                                }
                                                else
                                                {
                                                    item.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                                } 
                                            }
                                        } 
                                    }

                                    check.Stories = item.Stories;
                                }
                                else
                                {
                                    foreach (var item1 in item.Stories)
                                    {
                                        item.DurationsList ??= new List<long>();

                                        //image and video
                                        string mediaFile = "";
                                        if (!item1.Thumbnail.Contains("avatar") && item1.Videos.Count == 0)
                                            mediaFile = item1.Thumbnail;
                                        else if (item1.Videos.Count > 0)
                                            mediaFile = item1.Videos[0].Filename;

                                        if (!string.IsNullOrEmpty(mediaFile))
                                        {
                                            var type1 = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                            if (type1 != "Video")
                                            {
                                                Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                                item.DurationsList.Add(AppSettings.StoryImageDuration * 1000); 
                                            }
                                            else
                                            {
                                                var fileName = mediaFile.Split('/').Last();
                                                WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile, "other");

                                                if (AppSettings.ShowFullVideo)
                                                {
                                                    var duration = WoWonderTools.GetDuration(mediaFile);
                                                    item.DurationsList.Add(Long.ParseLong(duration));
                                                }
                                                else
                                                {
                                                    item.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                                }
                                            }
                                        }  
                                    }

                                    MAdapter.StoryList.Add(item);
                                }
                            }

                            Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e); 
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);

                Activity?.RunOnUiThread(ShowEmptyPage);
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

                ToastUtils.ShowToast(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
            }
        }

        public void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.StoryList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone; 
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoStory);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(UserDetails.Avatar))
                    Glide.With(Context).Load(UserDetails.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Into(ProfileUserImage);
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog
         
        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.image))
                {
                    ContextActivity.OnImage_Button_Click();
                }
                else if (itemString == GetText(Resource.String.video))
                {
                    ContextActivity.OnVideo_Button_Click();
                }
                else if (itemString == Activity.GetText(Resource.String.text))
                {
                    ContextActivity.OpenEditColor();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

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
         
        #endregion
    } 
}