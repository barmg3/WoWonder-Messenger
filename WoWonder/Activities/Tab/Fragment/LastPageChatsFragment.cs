using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.Tab.Adapter;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.PageChat;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Tab.Fragment
{
    public class LastPageChatsFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        public LastChatsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TLastMessagesLayout, container, false);
                return view;
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

                InitComponent(view);
                SetRecyclerViewAdapters();

                Task.Factory.StartNew(() => StartApiService());
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
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
                MAdapter = new LastChatsAdapter(Activity, "page") { LastChatsList = new ObservableCollection<Classes.LastChatsClass>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetItemAnimator(null);

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<PageDataObject>(Activity, MAdapter, sizeProvider, 10);
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

        #endregion

        #region Events 

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.LastChatsList.LastOrDefault(a => a.Type == Classes.ItemType.LastChatNewV);
                if (item?.LastChatPage != null && !string.IsNullOrEmpty(item.LastChatPage?.PageId) && !MainScrollEvent.IsLoading)
                    StartApiService(item.LastChatPage?.PageId);
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

                MAdapter.LastChatsList.Clear();
                MAdapter.NotifyDataSetChanged();

                Task.Factory.StartNew(() => StartApiService());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, LastChatsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        Intent intent = new Intent(Context, typeof(PageChatWindowActivity));
                        intent.PutExtra("ChatId", item.LastChatPage.ChatId);
                        intent.PutExtra("PageId", item.LastChatPage.PageId);
                        intent.PutExtra("TypeChat", "PageProfile");
                        intent.PutExtra("ShowEmpty", "no");
                        intent.PutExtra("PageObject", JsonConvert.SerializeObject(item.LastChatPage));
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemLongClick(object sender, LastChatsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        OptionsLastMessagesBottomSheet bottomSheet = new OptionsLastMessagesBottomSheet();
                        Bundle bundle = new Bundle();
                        bundle.PutString("Type", "page");
                        bundle.PutString("ItemObject", JsonConvert.SerializeObject(item.LastChatPage));
                        bottomSheet.Arguments = bundle;
                        bottomSheet.Show(ChildFragmentManager, bottomSheet.Tag);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Page Chat

        public void StartApiService(string offset = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadPageChatAsync(offset) });
            else
                ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
        }

        private async Task LoadPageChatAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            MainScrollEvent.IsLoading = true;

            var countList = MAdapter.LastChatsList.Count;
            var (apiStatus, respond) = await RequestsAsync.PageChat.GetPageChatListAsync("20", offset);
            if (apiStatus.Equals(200))
            {
                if (respond is PageListObject result)
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var pageClass in from pageClass in result.Data let check = MAdapter?.LastChatsList.FirstOrDefault(a => a.LastChatPage?.PageId == pageClass.PageId) where check == null select pageClass)
                        {
                            var item = WoWonderTools.FilterDataLastChatPage(pageClass);
                            MAdapter?.LastChatsList.Add(new Classes.LastChatsClass
                            {
                                LastChatPage = item,
                                Type = Classes.ItemType.LastChatPage
                            });
                        }

                        if (countList > 0)
                        {
                            Activity?.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.LastChatsList.Count - countList); });
                        }
                        else
                        {
                            Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter?.LastChatsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_NoMorePage), ToastLength.Short);
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity?.RunOnUiThread(ShowEmptyPage);
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (SwipeRefreshLayout.Refreshing)
                    SwipeRefreshLayout.Refreshing = false;

                if (MainScrollEvent != null)
                    MainScrollEvent.IsLoading = false;

                if (MAdapter.LastChatsList.Count > 0)
                {
                    var emptyStateChecker = MAdapter.LastChatsList.FirstOrDefault(a => a.Type == Classes.ItemType.EmptyPage);
                    if (emptyStateChecker != null)
                    {
                        var index = MAdapter.LastChatsList.IndexOf(emptyStateChecker);

                        MAdapter.LastChatsList.Remove(emptyStateChecker);
                        MAdapter.NotifyItemRemoved(index);
                    }

                    //var archive = MAdapter.LastChatsList.FirstOrDefault(a => a.Type == Classes.ItemType.Archive);
                    //if (archive != null)
                    //{
                    //    archive.CountArchive = ListUtils.ArchiveUserChatList.Count.ToString();

                    //    var index = MAdapter.LastChatsList.IndexOf(archive);
                    //    MAdapter.LastChatsList.Move(index, MAdapter.LastChatsList.Count);
                    //    MAdapter.NotifyItemMoved(index, MAdapter.LastChatsList.Count);
                    //}
                    //else
                    //{
                    //    MAdapter.LastChatsList.Add(new Classes.LastChatsClass()
                    //    {
                    //        CountArchive = ListUtils.ArchiveUserChatList.Count.ToString(),
                    //        Type = Classes.ItemType.Archive, 
                    //    });
                    //    MAdapter.NotifyItemInserted(MAdapter.LastChatsList.Count);
                    //}
                }
                else
                {
                    var emptyStateChecker = MAdapter.LastChatsList.FirstOrDefault(q => q.Type == Classes.ItemType.EmptyPage);
                    if (emptyStateChecker == null)
                    {
                        MAdapter.LastChatsList.Add(new Classes.LastChatsClass
                        {
                            Type = Classes.ItemType.EmptyPage
                        });
                        MAdapter.NotifyDataSetChanged();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

                if (SwipeRefreshLayout.Refreshing)
                    SwipeRefreshLayout.Refreshing = false;

                if (MainScrollEvent != null)
                    MainScrollEvent.IsLoading = false;
            }
        }


        #endregion

    }
}