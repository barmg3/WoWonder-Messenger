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
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.Tab.Adapter;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Tab.Fragment
{
    public class LastGroupChatsFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        public LastChatsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
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
                MAdapter = new LastChatsAdapter(Activity, "group") { LastChatsList = new ObservableCollection<Classes.LastChatsClass>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetItemAnimator(null);

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<ChatObject>(Activity, MAdapter, sizeProvider, 10);
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
                if (item != null && !string.IsNullOrEmpty(item.LastChat.GroupId) && !MainScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGroupChatAsync(item.LastChat.GroupId) });
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
                        switch (item.Type)
                        {
                            case Classes.ItemType.LastChatNewV:
                                {
                                    Activity?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            if (item.LastChat.LastMessage.LastMessageClass != null && item.LastChat.LastMessage.LastMessageClass.Seen == "0" && item.LastChat.LastMessage.LastMessageClass.ToId == UserDetails.UserId && item.LastChat.LastMessage.LastMessageClass.FromId != UserDetails.UserId)
                                            {
                                                item.LastChat.LastMessage.LastMessageClass.Seen = "1";
                                                MAdapter.NotifyItemChanged(position);
                                            }
                                        }
                                        catch (Exception exception)
                                        {
                                            Methods.DisplayReportResultTrack(exception);
                                        }
                                    });

                                    Intent intent = new Intent(Context, typeof(GroupChatWindowActivity));
                                    intent.PutExtra("ChatId", item.LastChat.ChatId);
                                    intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item.LastChat));
                                    intent.PutExtra("ShowEmpty", "no");
                                    intent.PutExtra("GroupId", item.LastChat.GroupId);
                                    StartActivity(intent);
                                    break;
                                }
                            case Classes.ItemType.GroupRequest:
                                {
                                    if (item.GroupRequestList.Count > 0)
                                    {
                                        var intent = new Intent(Context, typeof(GroupRequestActivity));
                                        Context.StartActivity(intent);
                                    }

                                    break;
                                }
                            case Classes.ItemType.Archive:
                                {
                                    var intent = new Intent(Context, typeof(ArchivedActivity));
                                    Context.StartActivity(intent);
                                    break;
                                }
                        }
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
                        bundle.PutString("Type", "group");
                        bundle.PutString("ItemObject", JsonConvert.SerializeObject(item.LastChat));
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

        #region Load Group Chat

        public void StartApiService(string offset = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGroupChatAsync(offset), LoadGeneralData });
            else
                ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
        }

        private async Task LoadGroupChatAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            MainScrollEvent.IsLoading = true;

            var countList = MAdapter.LastChatsList.Count;
            var (apiStatus, respond) = await RequestsAsync.GroupChat.GetGroupChatListAsync("20", offset);
            if (apiStatus.Equals(200))
            {
                if (respond is GroupListObject result)
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        foreach (var chatObject in from chatObject in result.Data let check = MAdapter?.LastChatsList.FirstOrDefault(a => a.LastChat?.GroupId == chatObject.GroupId) where check == null select chatObject)
                        {
                            chatObject.ChatType = "group";
                            var item = WoWonderTools.FilterDataLastChatNewV(chatObject);

                            MAdapter?.LastChatsList.Add(new Classes.LastChatsClass
                            {
                                LastChat = item,
                                Type = Classes.ItemType.LastChatNewV
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
                            ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_NoMoreGroup), ToastLength.Short);
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity?.RunOnUiThread(ShowEmptyPage);
        }

        public void ShowEmptyPage()
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

        //Get General Data Using Api >> Group Chat Requests
        private async Task LoadGeneralData()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.GetGeneralDataAsync(false, UserDetails.OnlineUsers, UserDetails.DeviceId, "1", "group_chat_requests");
                    if (apiStatus == 200)
                    {
                        if (respond is GetGeneralDataObject result)
                        {
                            // Group Requests
                            var respondListGroupRequests = result?.GroupChatRequests?.Count;
                            if (respondListGroupRequests > 0)
                            {
                                ListUtils.GroupRequestsList = new ObservableCollection<GroupChatRequest>(result.GroupChatRequests);

                                var checkList = MAdapter.LastChatsList.FirstOrDefault(q => q.Type == Classes.ItemType.GroupRequest);
                                if (checkList == null)
                                {
                                    var groupRequests = new Classes.LastChatsClass
                                    {
                                        GroupRequestList = new List<GroupChatRequest>(),
                                        Type = Classes.ItemType.GroupRequest
                                    };

                                    var list = result.GroupChatRequests.TakeLast(4).ToList();
                                    if (list.Count > 0)
                                        groupRequests.GroupRequestList.AddRange(list);

                                    MAdapter.LastChatsList.Insert(0, groupRequests);
                                }
                                else
                                {
                                    if (checkList.GroupRequestList.Count < 3)
                                    {
                                        var list = result.GroupChatRequests.TakeLast(4).ToList();
                                        if (list.Count > 0)
                                            checkList.GroupRequestList.AddRange(list);
                                    }
                                }

                                Activity?.RunOnUiThread(() => { MAdapter.NotifyItemInserted(0); });
                            }
                            else
                            {
                                var checkList = MAdapter?.LastChatsList?.FirstOrDefault(q => q.Type == Classes.ItemType.GroupRequest);
                                if (checkList != null)
                                {
                                    MAdapter.LastChatsList.Remove(checkList);
                                    Activity?.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}