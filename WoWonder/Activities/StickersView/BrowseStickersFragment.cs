using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Tabs;
using WoWonder.Activities.StickersView.Fragment;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.StickersView
{
    public class BrowseStickersFragment : BottomSheetDialogFragment, TabLayoutMediator.ITabConfigurationStrategy, TextView.IOnEditorActionListener, IDialogInterfaceOnShowListener
    {
        #region Variables Basic

        private AutoCompleteTextView SearchView;
        private ImageView CloseIcon;
        private MainTabAdapter Adapter;
        private ViewPager2 ViewPager;
        private AllStickersFragment StickersTab;
        private MyStickersFragment MyStickersTab;
        private TabLayout TabLayout;
        private static BrowseStickersFragment Instance;
        public string TypePage, SearchText = "";

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.SettingStickersLayout, container, false);
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

                Instance = this;
                Dialog.SetOnShowListener(this);

                TypePage = Arguments.GetString("TypePage") ?? ""; //ChatWindow ,GroupChatWindow,PageChatWindow

                InitComponent(view); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                CloseIcon = view.FindViewById<ImageView>(Resource.Id.closeIcon);
                CloseIcon.Visibility = ViewStates.Invisible;
                CloseIcon.Click += CloseIconOnClick;
                SearchView = view.FindViewById<AutoCompleteTextView>(Resource.Id.searchBox);
                SearchView.SetOnEditorActionListener(this);
                SearchView.TextChanged += SearchViewOnTextChanged;

                //Change text colors
                SearchView.SetHintTextColor(Color.Gray);
                SearchView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                ViewPager = view.FindViewById<ViewPager2>(Resource.Id.viewpager);
                TabLayout = view.FindViewById<TabLayout>(Resource.Id.tabs);

                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static BrowseStickersFragment GetInstance()
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

        #region Set Tap

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                StickersTab = new AllStickersFragment();
                MyStickersTab = new MyStickersFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(StickersTab, GetText(Resource.String.Lbl_AllStickers));
                Adapter.AddFragment(MyStickersTab, GetText(Resource.String.Lbl_MyStickers));

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                ViewPager.UserInputEnabled = false;
                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        #endregion

        #region Event
         
        private void CloseIconOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchText = "";
                SearchView.Text = "";
                SearchView.ClearFocus();

                StickersTab.MRecyclerSearch.Visibility = ViewStates.Gone;

                if (StickersTab.MAdapter.StickersList.Count > 0)
                {
                    StickersTab.MRecycler.Visibility = ViewStates.Visible;
                    StickersTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchViewOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (SearchView.Text?.Length > 0)
                {
                    CloseIcon.Visibility = ViewStates.Visible;
                }
                else
                {
                    CloseIcon.Visibility = ViewStates.Invisible;

                    StickersTab.MRecyclerSearch.Visibility = ViewStates.Gone;

                    if (StickersTab.MAdapter.StickersList.Count > 0)
                    {
                        StickersTab.MRecycler.Visibility = ViewStates.Visible;
                        StickersTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public bool OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
        {
            switch (actionId)
            {
                case ImeAction.Search:

                    StartSearch(v.Text);

                    v.ClearFocus();

                    return true;
                default:
                    return false;
            }
        }

        #endregion
         
        #region Load Stickers

        public void StartApiService()
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartTrendingSearch });
            else
                ToastUtils.ShowToast(Activity, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
        }

        private async Task StartTrendingSearch()
        {
            try
            {
                var checkList = StickersTab.MAdapter.StickersList.FirstOrDefault(q => q.ItemType == StickersModel.ItemTypeShop.RecommendedTag);
                if (checkList == null)
                {
                    var recommendedTag = new StickersModel
                    {
                        ListTags = new List<string>(),
                        ItemType = StickersModel.ItemTypeShop.RecommendedTag
                    };

                    recommendedTag.ListTags.Add("love");
                    recommendedTag.ListTags.Add("kiss");
                    recommendedTag.ListTags.Add("dance");
                    recommendedTag.ListTags.Add("wink");
                    recommendedTag.ListTags.Add("wow");
                    recommendedTag.ListTags.Add("happy");
                    recommendedTag.ListTags.Add("lazy");
                    recommendedTag.ListTags.Add("sleepy");
                    recommendedTag.ListTags.Add("excited");
                    recommendedTag.ListTags.Add("good");

                    StickersTab.MAdapter.StickersList.Insert(0, recommendedTag);
                   // Activity.RunOnUiThread(() => { StickersTab.MAdapter.NotifyDataSetChanged(); });
                }
                await StartTrendingSticker();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task StartTrendingSticker()
        {
            var result = await StickersModel.ApiStipop.ApiGetTrendingStickerPack(SearchText);
            if (result != null)
            {
                var respondUserList = result.Count;
                if (respondUserList > 0)
                {
                    var checkList = StickersTab.MAdapter.StickersList.FirstOrDefault(q => q.ItemType == StickersModel.ItemTypeShop.TrendingSticker);
                    if (checkList == null)
                    {
                        var trending = new StickersModel
                        {
                            ListSticker = new List<string>(),
                            ItemType = StickersModel.ItemTypeShop.TrendingSticker
                        };

                        foreach (var item in from item in result let check = StickersTab.MAdapter.StickersList.FirstOrDefault(a => a.PackageId == item.PackageId) where check == null select item)
                        {
                            trending.PackageId = item.PackageId;
                            trending.Name = item.PackageName;
                            trending.ListSticker.Add(item.PackageImg);
                        }

                        StickersTab.MAdapter.StickersList.Insert(1, trending);
                        //Activity.RunOnUiThread(() => { StickersTab.MAdapter.NotifyItemInserted(1); });
                    }
                    else
                    {
                        foreach (var item in from item in result let check = StickersTab.MAdapter.StickersList.FirstOrDefault(a => a.PackageId == item.PackageId) where check == null select item)
                        {
                            checkList.PackageId = item.PackageId;
                            checkList.Name = item.PackageName;
                            checkList.ListSticker.Add(item.PackageImg);
                        }
                    }
                }
            }

            await StartNewStickerPacks();
        }

        public async Task StartNewStickerPacks(string offset = "1")
        {
            if (StickersTab.MainScrollEvent.IsLoading) return;
             
            StickersTab.MainScrollEvent.IsLoading = true;
            var result = await StickersModel.ApiStipop.ApiGetNewStickerPacks(offset);
            if (result != null)
            {
                var respondUserList = result.Count;
                if (respondUserList > 0)
                {
                    foreach (var item in from item in result let check = StickersTab.MAdapter.StickersList.FirstOrDefault(a => a.PackageId == item.PackageId) where check == null select item)
                    {
                        var sticker = new StickersModel
                        {
                            PackageId = item.PackageId,
                            Name = item.PackageName,
                            ListSticker = new List<string>(),
                            ItemType = StickersModel.ItemTypeShop.ShopSticker
                        };
                        sticker.ListSticker.Add(item.PackageImg);

                        StickersTab.MAdapter.StickersList.Add(sticker);
                    }

                    Activity.RunOnUiThread(() => { StickersTab.MAdapter.NotifyDataSetChanged(); });
                }
            }

            Activity.RunOnUiThread(() => { ShowEmptyPage("GetStickers"); });
        }

        public void ShowEmptyPage(string type)
        {
            try
            {
                if (type == "GetStickers")
                {
                    StickersTab.MainScrollEvent.IsLoading = false;
                    StickersTab.SwipeRefreshLayout.Refreshing = false;

                    StickersTab.MRecyclerSearch.Visibility = ViewStates.Gone;

                    if (StickersTab.MAdapter.StickersList.Count > 0)
                    {
                        StickersTab.MRecycler.Visibility = ViewStates.Visible;
                        StickersTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        StickersTab.MRecycler.Visibility = ViewStates.Gone;

                        StickersTab.Inflated = StickersTab.Inflated switch
                        {
                            null => StickersTab.EmptyStateLayout.Inflate(),
                            _ => StickersTab.Inflated
                        };

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(StickersTab.Inflated, EmptyStateInflater.Type.SomThingWentWrong);
                        if (!x.EmptyStateButton.HasOnClickListeners) x.EmptyStateButton.Click += null!;

                        StickersTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "GetStickersSearch")
                {
                    StickersTab.MainSearchScrollEvent.IsLoading = false;
                    StickersTab.SwipeRefreshLayout.Refreshing = false;

                    StickersTab.MRecycler.Visibility = ViewStates.Gone;

                    if (StickersTab.MAdapterSearch.StickersList.Count > 0)
                    {
                        StickersTab.MRecyclerSearch.Visibility = ViewStates.Visible;
                        StickersTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        StickersTab.MRecyclerSearch.Visibility = ViewStates.Gone;

                        StickersTab.Inflated = StickersTab.Inflated switch
                        {
                            null => StickersTab.EmptyStateLayout.Inflate(),
                            _ => StickersTab.Inflated
                        };

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(StickersTab.Inflated, EmptyStateInflater.Type.SomThingWentWrong);
                        if (!x.EmptyStateButton.HasOnClickListeners) x.EmptyStateButton.Click += null!;

                        StickersTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "GetMyStickers")
                {
                    MyStickersTab.SwipeRefreshLayout.Refreshing = false;

                    if (MyStickersTab.MAdapter.StickersList.Count > 0)
                    {
                        MyStickersTab.MRecycler.Visibility = ViewStates.Visible;
                        MyStickersTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        MyStickersTab.MRecycler.Visibility = ViewStates.Gone;

                        MyStickersTab.Inflated = MyStickersTab.Inflated switch
                        {
                            null => MyStickersTab.EmptyStateLayout.Inflate(),
                            _ => MyStickersTab.Inflated
                        };

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(MyStickersTab.Inflated, EmptyStateInflater.Type.SomThingWentWrong);
                        if (!x.EmptyStateButton.HasOnClickListeners) x.EmptyStateButton.Click += null!;

                        MyStickersTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
            }
            catch (Exception e)
            {
                StickersTab.MainSearchScrollEvent.IsLoading = false;
                StickersTab.MainScrollEvent.IsLoading = false;
                StickersTab.SwipeRefreshLayout.Refreshing = false;
                MyStickersTab.SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Search
         
        public void StartSearch(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return;

                SearchText = key;
                SearchView.Text = key;

                StickersTab.MRecycler.Visibility = ViewStates.Gone;

                StickersTab.MRecyclerSearch.Visibility = ViewStates.Visible;
                StickersTab.MAdapterSearch.StickersList.Clear();
                StickersTab.MAdapterSearch.NotifyDataSetChanged();

                StickersTab.SwipeRefreshLayout.Refreshing = true;

                SearchViewOnQueryTextSubmit();

                SearchView.ClearFocus();   
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SearchViewOnQueryTextSubmit()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => StartSearchRequest() });
        }
         
        public async Task StartSearchRequest(string offset = "1")
        {
            if (string.IsNullOrEmpty(SearchText))
                return;

            if (StickersTab.MainSearchScrollEvent.IsLoading) return;

            StickersTab.MainSearchScrollEvent.IsLoading = true;

            var result = await StickersModel.ApiStipop.ApiGetSearch(SearchText, offset);
            if (result != null)
            {
                var respondUserList = result.Body.StickerList.Count;
                if (respondUserList > 0)
                {
                    foreach (var item in from item in result.Body.StickerList let check = StickersTab.MAdapterSearch.StickersList.FirstOrDefault(a => a.PackageId == item.StickerId) where check == null select item)
                    {
                        var sticker = new StickersModel
                        {
                            ListSticker = new List<string>(),
                            ItemType = StickersModel.ItemTypeShop.IconSticker,
                            PackageId = item.StickerId,
                            Name = ""
                        };

                        sticker.ListSticker.Add(item.StickerImg);

                        StickersTab.MAdapterSearch.StickersList.Add(sticker);
                    }

                    Activity.RunOnUiThread(() => { StickersTab.MAdapterSearch.NotifyDataSetChanged(); });
                }

                Activity.RunOnUiThread(() => { ShowEmptyPage("GetStickersSearch"); });
            }
        }

        #endregion

        public void OnShow(IDialogInterface dialog)
        {
            try
            {
                var d = dialog as BottomSheetDialog;
                var bottomSheet = d.FindViewById<View>(Resource.Id.design_bottom_sheet) as FrameLayout;
                var bottomSheetBehavior = BottomSheetBehavior.From(bottomSheet);
                var layoutParams = bottomSheet.LayoutParameters;

                if (layoutParams != null)
                    layoutParams.Height = Resources.DisplayMetrics.HeightPixels;

                bottomSheet.LayoutParameters = layoutParams;
                bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}