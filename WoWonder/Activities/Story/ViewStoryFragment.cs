using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views.Animations;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using WoWonder.Activities.Story.Adapter;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.Library.Anjo.Stories.DragView;
using WoWonder.Library.Anjo.Stories.StoriesProgressView;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace WoWonder.Activities.Story
{
    public class ViewStoryFragment : Fragment, DragToClose.IDragListener, StoriesProgressView.IStoriesListener
    {
        #region  Variables Basic

        private StoryDetailsActivity GlobalContext;
        private TextView IconBack;
        private ImageView UserImageView;
        private string UserId = "", StoryId = "";
        public StoriesProgressView StoriesProgress;
        private StoryDataObject DataStories;
        private View MainView;
        private LinearLayout UserLayout;
        private TextView UsernameTextView, LastSeenTextView, DeleteIconView;
        private RecyclerView MRecycler;
        private StoryShowAdapter MAdapter;
        private ObservableCollection<StoryDataObject> StoryList;
        private int Counter;
        public StoriesProgressView.IStoryStateListener StoryStateListener;

        private static ViewStoryFragment Instance;

        private bool MIsVisibleToUser;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (StoryDetailsActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                MainView = inflater.Inflate(Resource.Layout.StorySwipeLayout, container, false);
                return MainView;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                MainView = view;

                Instance = this;

                InitComponent(view);
                SetRecyclerViewAdapters();

                var checkSection = TabbedMainActivity.GetInstance()?.LastStoriesTab?.MAdapter?.StoryList;
                if (checkSection?.Count > 0)
                {
                    List<StoryDataObject> storiesList = new List<StoryDataObject>(checkSection);
                    storiesList.RemoveAll(o => o.Type == "Your" || o.Type == "Live");

                    StoryList = new ObservableCollection<StoryDataObject>(storiesList);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void SetMenuVisibility(bool menuVisible)
        {
            try
            {
                base.SetMenuVisibility(menuVisible);
                MIsVisibleToUser = menuVisible;
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
                //StoryStateListener?.OnResume(); 

                if (IsResumed && MIsVisibleToUser)
                {
                    //var position = Arguments.GetInt("position", 0); 
                    DataStories = JsonConvert.DeserializeObject<StoryDataObject>(Arguments.GetString("DataItem") ?? "");
                    if (DataStories != null)
                    {
                        LoadData(DataStories);
                    }
                }
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
                StoryStateListener?.OnPause();
                StoriesProgress?.Pause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnStop()
        {
            try
            {
                base.OnStop();

                if (MIsVisibleToUser)
                    StoryStateListener?.OnPause();
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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnDestroyView()
        {
            try
            {
                StoriesProgress?.Destroy();
                StoriesProgress = null!;

                base.OnDestroyView();
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
                StoriesProgress?.Destroy();
                StoriesProgress = null!;

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnDestroy();
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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.story_container);

                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack);

                StoriesProgress = view.FindViewById<StoriesProgressView>(Resource.Id.storyProgressView);

                UserLayout = view.FindViewById<LinearLayout>(Resource.Id.userLayout);
                UserImageView = view.FindViewById<ImageView>(Resource.Id.imageAvatar);
                UsernameTextView = view.FindViewById<TextView>(Resource.Id.username);
                LastSeenTextView = view.FindViewById<TextView>(Resource.Id.time);

                DeleteIconView = view.FindViewById<TextView>(Resource.Id.DeleteIcon);
                DeleteIconView.Visibility = ViewStates.Invisible;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, DeleteIconView, IonIconsFonts.Trash);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconBack, FontAwesomeIcon.LongArrowLeft);

                GlobalContext.DragToClose.SetDragListener(this);
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
                MAdapter = new StoryShowAdapter(Activity, StoriesProgress, this)
                {
                    StoryList = new ObservableCollection<StoryDataObject.Story>()
                };

                var layoutManager = new MyLinearLayoutManager(Context, LinearLayoutManager.Horizontal, false);
                MRecycler.SetLayoutManager(layoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<StoryDataObject.Story>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
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
                    DeleteIconView.Click += DeleteIconViewOnClick;
                    IconBack.Click += IconBackOnClick;
                }
                else
                {
                    DeleteIconView.Click -= DeleteIconViewOnClick;
                    IconBack.Click -= IconBackOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ViewStoryFragment GetInstance()
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

        private void IconBackOnClick(object sender, EventArgs e)
        {
            GlobalContext.Finish();
        }

        //delete story
        private async void DeleteIconViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(StoryId))
                    return;

                StoriesProgress?.Pause();

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

                        GlobalContext.Finish();
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

        #endregion

        #region Drag

        public void OnStartDraggingView()
        {

        }

        public void OnDraggingView(float offset)
        {
            try
            {
                if (StoriesProgress != null) StoriesProgress.Alpha = offset;
                if (UserLayout != null) UserLayout.Alpha = offset;
                if (MRecycler != null) MRecycler.Alpha = offset;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnViewClosed()
        {

        }

        #endregion

        #region StoriesProgressView

        public void SetStoryStateListener(StoriesProgressView.IStoryStateListener storyStateListener)
        {
            StoryStateListener = storyStateListener;
        }

        public void OnNext()
        {
            try
            {
                //StoriesProgress.Pause();
                ++Counter;

                if (Counter + 1 > DataStories.Stories.Count)
                {
                    OnComplete();
                    return;
                }

                var dataStory = DataStories.Stories[Counter];
                if (dataStory != null)
                {
                    StoryId = dataStory.Id;
                    StoryStateListener?.OnPause();
                    MRecycler.ScrollToPosition(Counter);
                }
                else
                {
                    OnComplete();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnPrev()
        {
            try
            {
                if (Counter <= 0)
                {
                    //StoriesProgress?.Pause();
                    StoriesProgress?.Destroy();
                    StoriesProgress = null!;

                    --Counter;

                    if (GlobalContext.Pager.CurrentItem > 0)
                        GlobalContext.Pager.SetCurrentItem(GlobalContext.Pager.CurrentItem - 1, true);
                    else
                        GlobalContext.Finish();
                }
                else
                {
                    //StoriesProgress?.Pause();
                    --Counter;

                    var dataStory = DataStories.Stories[Counter];
                    if (dataStory != null)
                    {
                        StoryId = dataStory.Id;
                        StoryStateListener?.OnPause();
                        MRecycler.ScrollToPosition(Counter);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnComplete()
        {
            try
            {
                if (GlobalContext.Pager.CurrentItem < StoryList.Count - 1)
                {
                    StoriesProgress?.Destroy();
                    StoriesProgress = null!;

                    if (GlobalContext.Pager.CurrentItem < StoryList.Count - 1)
                        GlobalContext.Pager.SetCurrentItem(GlobalContext.Pager.CurrentItem + 1, true);
                }
                else
                {
                    AdsGoogle.Ad_Interstitial(Activity);
                    GlobalContext.Finish();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Info story

        private void LoadData(StoryDataObject dataStories)
        {
            try
            {
                if (dataStories == null) return;
                UserId = dataStories.UserId;

                GlideImageLoader.LoadImage(Activity, dataStories.Avatar, UserImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                UsernameTextView.Text = WoWonderTools.GetNameFinal(dataStories);

                var fistStory = DataStories.Stories.FirstOrDefault();
                if (fistStory != null)
                {
                    StoryId = fistStory.Id;
                    DeleteIconView.Visibility = fistStory.IsOwner ? ViewStates.Visible : ViewStates.Invisible;

                    if (!string.IsNullOrEmpty(fistStory.TimeText))
                    {
                        LastSeenTextView.Text = Methods.FunString.DecodeString(fistStory.TimeText);
                    }
                    else
                    {
                        bool success = int.TryParse(fistStory.Posted, out var number);
                        switch (success)
                        {
                            case true:
                                LastSeenTextView.Text = Methods.Time.TimeAgo(number, false);
                                break;
                            default:
                                LastSeenTextView.Text = fistStory.Posted;
                                break;
                        }
                    }
                }

                StoriesProgress ??= MainView?.FindViewById<StoriesProgressView>(Resource.Id.storyProgressView);

                if (StoriesProgress != null)
                {
                    StoriesProgress.Visibility = ViewStates.Visible;

                    int count = dataStories.Stories.Count;
                    StoriesProgress.Visibility = ViewStates.Visible;
                    StoriesProgress.SetStoriesCount(count); // <- set stories
                    StoriesProgress.SetStoriesListener(this); // <- set listener  
                    //StoriesProgress.SetStoryDuration(10000L); // <- set a story duration   

                    StoriesProgress?.SetStoriesCountWithDurations(dataStories.DurationsList.ToArray());

                    MAdapter.StoryList = new ObservableCollection<StoryDataObject.Story>(dataStories.Stories);
                    MAdapter.NotifyDataSetChanged();

                    StoriesProgress?.StartStories(); // <- start progress 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private class MyLinearLayoutManager : LinearLayoutManager
        {

            protected MyLinearLayoutManager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
            }

            public MyLinearLayoutManager(Context context) : base(context)
            {
                Init();
            }

            public MyLinearLayoutManager(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
            {
                Init();
            }

            public MyLinearLayoutManager(Context context, int orientation, bool reverseLayout) : base(context, orientation, reverseLayout)
            {
                Init();
            }

            private void Init()
            {
                try
                {
                    Orientation = Horizontal;
                    ReverseLayout = false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override bool CanScrollVertically()
            {
                return false;
            }

            public override bool CanScrollHorizontally()
            {
                return false;
            }
        }

        public void SetLastSeenTextView(StoryDataObject.Story story)
        {
            Activity?.RunOnUiThread(() =>
            {
                try
                {
                    StoryId = story.Id;
                    UserId = story.UserId;
                    DeleteIconView.Visibility = story.IsOwner ? ViewStates.Visible : ViewStates.Invisible;

                    if (!string.IsNullOrEmpty(story.TimeText))
                    {
                        LastSeenTextView.Text = Methods.FunString.DecodeString(story.TimeText);
                    }
                    else
                    {
                        bool success = int.TryParse(story.Posted, out var number);
                        switch (success)
                        {
                            case true:
                                LastSeenTextView.Text = Methods.Time.TimeAgo(number, false);
                                break;
                            default:
                                LastSeenTextView.Text = story.Posted;
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            });
        }

        public void OnEventMainThread(bool show)
        {
            try
            {
                if (show)
                {
                    FadeInAnimation(UserLayout, 200);
                    FadeInAnimation(StoriesProgress, 200);
                }
                else
                {
                    FadeOutAnimation(UserLayout, 200);
                    FadeOutAnimation(StoriesProgress, 200);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FadeOutAnimation(View view, long animationDuration)
        {
            try
            {
                var fadeOut = new AlphaAnimation(1, 0)
                {
                    Interpolator = new AccelerateInterpolator(),
                    StartOffset = animationDuration,
                    Duration = animationDuration
                };
                fadeOut.AnimationEnd += (sender, args) =>
                {
                    try
                    {
                        if (view != null) view.Visibility = ViewStates.Invisible;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };

                view?.StartAnimation(fadeOut);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FadeInAnimation(View view, long animationDuration)
        {
            try
            {
                Animation fadeIn = new AlphaAnimation(0, 1)
                {
                    Interpolator = new DecelerateInterpolator(),
                    Duration = animationDuration
                };
                fadeIn.AnimationEnd += (sender, args) =>
                {
                    try
                    {
                        if (view != null) view.Visibility = ViewStates.Visible;
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };

                view?.StartAnimation(fadeIn);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}