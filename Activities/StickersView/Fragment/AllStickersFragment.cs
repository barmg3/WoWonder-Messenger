using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using WoWonder.Activities.StickersView.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;

namespace WoWonder.Activities.StickersView.Fragment
{
    public class AllStickersFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        public StickersAdapters MAdapter, MAdapterSearch;
        private BrowseStickersFragment ContextStickers;
        public SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler, MRecyclerSearch;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent, MainSearchScrollEvent;
        private int CountOffset = 1, CountSearchOffset = 1;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.AllStickersLayout, container, false);
                ContextStickers = BrowseStickersFragment.GetInstance();
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
                SetRecyclerSearchViewAdapters();

                Task.Factory.StartNew(ContextStickers.StartApiService);
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
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                MRecyclerSearch = (RecyclerView)view.FindViewById(Resource.Id.recyclerSearch);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                 
                MRecycler.Visibility = ViewStates.Visible;
                MRecyclerSearch.Visibility = ViewStates.Gone;
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
                var layoutManager = new GridLayoutManager(Context, 2);
                layoutManager.SetSpanSizeLookup(new MySpanSizeLookup3(2, 1, 2));//20, 1, 4
                MAdapter = new StickersAdapters(Activity, ContextStickers.TypePage) { StickersList = new ObservableCollection<StickersModel>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MRecycler.SetLayoutManager(layoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<StickersModel>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(layoutManager);
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

        private void SetRecyclerSearchViewAdapters()
        {
            try
            {
                var layoutManager = new GridLayoutManager(Context, 4);
                MAdapterSearch = new StickersAdapters(Activity, ContextStickers.TypePage) { StickersList = new ObservableCollection<StickersModel>() };
                MAdapterSearch.ItemClick += MAdapterSearchOnItemClick;
                MRecyclerSearch.SetLayoutManager(layoutManager);
                MRecyclerSearch.HasFixedSize = true;
                MRecyclerSearch.SetItemViewCacheSize(10);
                MRecyclerSearch.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<StickersModel>(Activity, MAdapterSearch, sizeProvider, 10);
                MRecyclerSearch.AddOnScrollListener(preLoader);
                MRecyclerSearch.SetAdapter(MAdapterSearch);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(layoutManager);
                MainSearchScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainSearchScrollEvent.LoadMoreEvent += MainSearchScrollEventOnLoadMoreEvent;
                MRecyclerSearch.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainSearchScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Scroll Search
        private void MainSearchScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                if (!Methods.CheckConnectivity())
                    return;

                //Code get last id where LoadMore >>
                var checkList = MAdapter.StickersList.FirstOrDefault(q => q.ItemType == StickersModel.ItemTypeShop.ShopSticker);
                if (MainScrollEvent != null && checkList != null && !MainScrollEvent.IsLoading)
                {
                    CountSearchOffset += 1;
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ContextStickers.StartSearchRequest(CountSearchOffset.ToString()) });
                }
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
                if (!Methods.CheckConnectivity())
                    return;

                //Code get last id where LoadMore >>
                var checkList = MAdapter.StickersList.FirstOrDefault(q => q.ItemType == StickersModel.ItemTypeShop.ShopSticker);
                if (MainScrollEvent != null && checkList != null && !MainScrollEvent.IsLoading)
                {
                    CountOffset += 1;
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ContextStickers.StartNewStickerPacks(CountOffset.ToString()) });
                } 
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
                MAdapter.StickersList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                Task.Factory.StartNew(ContextStickers.StartApiService);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, StickersAdaptersViewHolderClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (item.ItemType == StickersModel.ItemTypeShop.IconSticker)
                    {
                        var url = item.ListSticker.FirstOrDefault();
                        new StickerItemClickListener(ContextStickers.TypePage).StickerAdapterOnOnItemClick(url);
                        ContextStickers.Dismiss();
                    }
                    else if (item.ItemType == StickersModel.ItemTypeShop.ShopSticker)
                    {
                        if (MAdapterSearch.StickersList.Count > 0)
                        {
                            MAdapterSearch.StickersList.Clear();
                            MAdapterSearch.NotifyDataSetChanged(); 
                        }

                        SwipeRefreshLayout.Refreshing = true;

                        MRecyclerSearch.Visibility = ViewStates.Visible;

                        MRecycler.Visibility = ViewStates.Gone;
                        EmptyStateLayout.Visibility = ViewStates.Gone;

                        if (!Methods.CheckConnectivity())
                            ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPackageInfoRequest(item.PackageId) });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void MAdapterSearchOnItemClick(object sender, StickersAdaptersViewHolderClickEventArgs e)
        {
            try
            {
                var item = MAdapterSearch.GetItem(e.Position);
                if (item != null)
                {
                    if (item.ItemType == StickersModel.ItemTypeShop.IconSticker)
                    {
                        var url = item.ListSticker.FirstOrDefault();
                        new StickerItemClickListener(ContextStickers.TypePage).StickerAdapterOnOnItemClick(url);
                        ContextStickers.Dismiss();
                    }
                    else if (item.ItemType == StickersModel.ItemTypeShop.ShopSticker)
                    {
                        if (MAdapterSearch.StickersList.Count > 0)
                        {
                            MAdapterSearch.StickersList.Clear();
                            MAdapterSearch.NotifyDataSetChanged(); 
                        }

                        SwipeRefreshLayout.Refreshing = true;

                        MRecyclerSearch.Visibility = ViewStates.Visible;

                        MRecycler.Visibility = ViewStates.Gone;
                        EmptyStateLayout.Visibility = ViewStates.Gone;

                        if (!Methods.CheckConnectivity())
                            ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPackageInfoRequest(item.PackageId) });
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        #region Get Sticker Pack info

        private async Task GetPackageInfoRequest(string id)
        {
            var result = await StickersModel.ApiStipop.ApiGetStickerPackInfo(id);
            if (result != null)
            {
                var respondUserList = result.Stickers.Count;
                if (respondUserList > 0)
                {
                    foreach (var item in from item in result.Stickers let check = MAdapterSearch.StickersList.FirstOrDefault(a => a.PackageId == item.StickerId) where check == null select item)
                    {
                        var sticker = new StickersModel
                        {
                            ListSticker = new List<string>(),
                            ItemType = StickersModel.ItemTypeShop.IconSticker,
                            PackageId = item.StickerId,
                            Name = ""
                        };
                         
                        sticker.ListSticker.Add(item.StickerImg);

                        MAdapterSearch.StickersList.Add(sticker);
                    }

                    Activity.RunOnUiThread(() => { MAdapterSearch.NotifyDataSetChanged(); });
                }

                Activity.RunOnUiThread(() => { ContextStickers.ShowEmptyPage("GetStickersSearch"); });
            }
        }

        #endregion
    }
}