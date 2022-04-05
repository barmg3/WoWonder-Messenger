using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Graphics;
using Android.OS;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using WoWonder.Activities.StickersView.Adapters;
using WoWonder.Activities.StickersView.Page;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.SQLite;

namespace WoWonder.Activities.StickersView.Fragment
{
    public class MyStickersFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        public StickersAdapters MAdapter;
        private BrowseStickersFragment ContextStickers;
        public SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated; 

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.MainFragmentLayout, container, false);
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
                LoadData();
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
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                
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
                LayoutManager = new LinearLayoutManager(Activity);
                MAdapter = new StickersAdapters(Activity, ContextStickers.TypePage) { StickersList = new ObservableCollection<StickersModel>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<StickersModel>(Activity, MAdapter, sizeProvider, 10);
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
         
        private void MAdapterOnItemClick(object sender, StickersAdaptersViewHolderClickEventArgs e)
        {
            try
            {
                var item = MAdapter?.StickersList[e.Position];
                if (item != null)
                {
                    if (item.Visibility == false)
                    {
                        item.Visibility = true;
                    }
                    else if (item.Visibility)
                    {
                        item.Visibility = false;
                    }

                    MAdapter.NotifyItemChanged(e.Position);

                    var sqLiteDatabase = new SqLiteDatabase();
                    sqLiteDatabase.Update_To_StickersTable(item.Name, item.Visibility);

                    EmojisViewTools.StickerView?.RefreshNow();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
          
        private void LoadData()
        {
            try
            {
                if (ListUtils.StickersList.Count > 0)
                {
                    foreach (var item in ListUtils.StickersList)
                    {
                        var listSticker = new WoWonderStickers(item.PackageId).GetStickers();

                        var stk = new StickersModel()
                        {
                            PackageId = item.PackageId,
                            Name = item.Name,
                            Visibility = item.Visibility,
                            Count = item.Count,
                            ListSticker = new List<string>(),
                            ItemType = StickersModel.ItemTypeShop.MySticker
                        };

                        foreach (var sticker in listSticker)
                        {
                            var data = (string)sticker.Data;
                            if (data != null)
                                stk.ListSticker.Add(data);
                        }

                        MAdapter.StickersList.Add(stk);
                    }

                    MAdapter.NotifyDataSetChanged();
                }

                ContextStickers.ShowEmptyPage("GetStickers");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}