using Android.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using WoWonder.Activities.Story.Adapter;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests; 
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Story
{
    public class StorySeenListFragment : BottomSheetDialogFragment
    {
        #region Variables Basic

        private LinearLayout BottomSheetLayout;

        private Toolbar ToolBar;
        private TextView TxtEmpty, IconShare, IconDelete;

        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private StorySeenListAdapter MAdapter;
        private RecyclerViewOnScrollListener MainScrollEvent;

        private string StoryId;
        private StoryDataObject.Story DataStories;
        private readonly ViewStoryFragment StoryFragment;

        #endregion

        #region General

        public StorySeenListFragment(ViewStoryFragment storyFragment)
        {
            try
            {
                StoryFragment = storyFragment;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.StorySeenListLayout, container, false);
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
                if (Arguments != null)
                {
                    StoryId = Arguments.GetString("StoryId") ?? "";
                    DataStories = JsonConvert.DeserializeObject<StoryDataObject.Story>(Arguments.GetString("DataNowStory") ?? "");
                }

                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnStart()
        {
            try
            {
                base.OnStart();
                var dialog = Dialog;
                //Make dialog full screen with transparent background
                if (dialog != null)
                {
                    var width = ViewGroup.LayoutParams.MatchParent;
                    var height = ViewGroup.LayoutParams.MatchParent;
                    dialog.Window.SetLayout(width, height);
                    dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
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

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
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

        public override void OnDestroy()
        {
            try
            {
                StoryFragment?.StoryStateListener?.OnResume();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                BottomSheetLayout = view.FindViewById<LinearLayout>(Resource.Id.bottom_sheet);
                TxtEmpty = view.FindViewById<TextView>(Resource.Id.empty_view);
                IconShare = view.FindViewById<TextView>(Resource.Id.toolbar_share);
                IconDelete = view.FindViewById<TextView>(Resource.Id.toolbar_delete);
                MRecycler = view.FindViewById<RecyclerView>(Resource.Id.seenList);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconShare, IonIconsFonts.ShareAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconDelete, IonIconsFonts.Trash);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                ToolBar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_ViewedBy) + " " + DataStories.ViewCount;
                    ToolBar.SetTitleTextColor(Color.White);
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
                MAdapter = new StorySeenListAdapter(Activity)
                {
                    UserList = new ObservableCollection<UserDataObject>()
                };
                LayoutManager = new LinearLayoutManager(Activity);
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
                    ToolBar.Click += CloseOnClick;
                    IconShare.Click += IconShareOnClick;
                    IconDelete.Click += IconDeleteOnClick;
                }
                else
                {
                    ToolBar.Click -= CloseOnClick;
                    IconShare.Click -= IconShareOnClick;
                    IconDelete.Click -= IconDeleteOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                var item = MAdapter.UserList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.UserId) && !MainScrollEvent.IsLoading)
                    StartApiService(item.UserId);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void IconDeleteOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(StoryId))
                    return;

                if (Methods.CheckConnectivity())
                {
                    (int respondCode, var respond) = await RequestsAsync.Story.DeleteStoryAsync(StoryId);
                    if (respondCode == 200)
                    {
                        var modelStory = TabbedMainActivity.GetInstance()?.LastStoriesTab?.MAdapter;

                        var story = modelStory?.StoryList?.FirstOrDefault(a => a.UserId == UserDetails.UserId);
                        if (story == null) return;
                        var item = story.Stories.FirstOrDefault(q => q.Id == StoryId);
                        if (item != null)
                        {
                            story.Stories.Remove(item);

                            modelStory.NotifyItemChanged(modelStory.StoryList.IndexOf(story));

                            if (story.Stories.Count == 0)
                            {
                                modelStory?.StoryList.Remove(story);
                                modelStory.NotifyDataSetChanged();
                            }
                        }
                        ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_Deleted), ToastLength.Short);

                        Dismiss();
                    }
                    else Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //wael
        private async void IconShareOnClick(object sender, EventArgs e)
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = "",
                    Text = "",
                    Url = ""
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CloseOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void Dismiss()
        {
            try
            {
                StoryFragment?.StoryStateListener?.OnResume();

                base.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load User 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadUsers(offset) });
        }

        private async Task LoadUsers(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                var countList = MAdapter.UserList.Count;
                var (apiStatus, respond) = await RequestsAsync.Story.GetStoryViewsAsync(StoryId, "20", offset);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case StoryViewsObject result:
                                    {
                                        result.Users.RemoveAll(o => o.UserId == UserDetails.UserId);
                                        var respondList = result.Users.Count;
                                        switch (respondList)
                                        {
                                            case > 0 when countList > 0:
                                                {
                                                    foreach (var item in from item in result.Users let check = MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                                    {
                                                        MAdapter.UserList.Add(item);
                                                    }

                                                    Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.UserList.Count - countList); });
                                                    break;
                                                }
                                            case > 0:
                                                MAdapter.UserList = new ObservableCollection<UserDataObject>(result.Users);
                                                Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                                break;
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(Activity, respond);
                        break;
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (MAdapter.UserList.Count > 0)
                {
                    TxtEmpty.Visibility = ViewStates.Gone;
                    MRecycler.Visibility = ViewStates.Visible;

                }
                else
                {
                    TxtEmpty.Visibility = ViewStates.Visible;
                    MRecycler.Visibility = ViewStates.Gone;
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