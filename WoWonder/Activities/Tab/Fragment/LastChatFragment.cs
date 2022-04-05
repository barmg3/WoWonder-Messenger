using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.FriendRequest;
using WoWonder.Activities.GroupChat; 
using WoWonder.Activities.PageChat;
using WoWonder.Activities.Tab.Adapter;
using WoWonder.Frameworks;
using WoWonder.Frameworks.Floating;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonder.SocketSystem;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Tab.Fragment
{
    public class LastChatFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        public LastChatsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private TabbedMainActivity GlobalContext;
        public static bool ApiRun;
        private static bool NoMoreUser;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (TabbedMainActivity)Activity;
        }

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

                LoadChat();

                GlobalContext?.GetOneSignalNotification();
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
                SwipeRefreshLayout.Refreshing = false;
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
                MAdapter = new LastChatsAdapter(Activity, "user") { LastChatsList = new ObservableCollection<Classes.LastChatsClass>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.ItemLongClick += MAdapterOnItemLongClick;

                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(50);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetItemAnimator(null);

                var sizeProvider = new ViewPreloadSizeProvider();
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
                var idUser = MAdapter?.LastChatsList?.LastOrDefault(a => a.Type == Classes.ItemType.LastChatNewV && a.LastChat?.ChatType == "user")?.LastChat?.ChatTime ?? "0";
                var idGroup = MAdapter?.LastChatsList?.LastOrDefault(a => a.Type == Classes.ItemType.LastChatNewV && a.LastChat?.ChatType == "group")?.LastChat?.ChatTime ?? "0";
                var idPage = MAdapter?.LastChatsList?.LastOrDefault(a => a.Type == Classes.ItemType.LastChatNewV && a.LastChat?.ChatType == "page")?.LastChat?.ChatTime ?? "0";
                if (idUser != "0" && !string.IsNullOrEmpty(idUser) && idGroup != "0" && !string.IsNullOrEmpty(idGroup) && idPage != "0" && !string.IsNullOrEmpty(idPage) && !MainScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadChatAsync(false, idUser, idGroup, idPage) });
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
                ApiRun = false;
                NoMoreUser = false;

                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);

                    if (SwipeRefreshLayout.Refreshing)
                        SwipeRefreshLayout.Refreshing = false;
                }
                else
                {
                    MAdapter?.LastChatsList?.Clear();
                    MAdapter?.NotifyDataSetChanged();
                    ListUtils.UserList.Clear();

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.ClearAll_LastUsersChat();
                    dbDatabase.ClearAll_Messages();

                    StartApiService();
                }
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
                                    if (item.LastChat.LastMessage.LastMessageClass != null && item.LastChat.LastMessage.LastMessageClass.Seen == "0" && item.LastChat.LastMessage.LastMessageClass.ToId == UserDetails.UserId && item.LastChat.LastMessage.LastMessageClass.FromId != UserDetails.UserId)
                                    {
                                        item.LastChat.LastMessage.LastMessageClass.Seen = "1";
                                        Activity?.RunOnUiThread(() => { MAdapter?.NotifyItemChanged(position); });

                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Insert_Or_Update_one_LastUsersChat(item.LastChat);
                                    }

                                    Intent intent = null!;
                                    switch (item.LastChat.ChatType)
                                    {
                                        case "user":

                                            string mainChatColor = AppSettings.MainColor;
                                            if (item.LastChat.LastMessage.LastMessageClass != null)
                                                mainChatColor = item.LastChat.LastMessage.LastMessageClass.ChatColor.Contains("rgb") ? Methods.FunString.ConvertColorRgBtoHex(item.LastChat.LastMessage.LastMessageClass.ChatColor) : item.LastChat.LastMessage.LastMessageClass.ChatColor ?? AppSettings.MainColor;

                                            if (!WoWonderTools.ChatIsAllowed(item.LastChat))
                                                return;

                                            intent = new Intent(Context, typeof(ChatWindowActivity));
                                            intent.PutExtra("ChatId", item.LastChat.ChatId);
                                            intent.PutExtra("UserID", item.LastChat.UserId);
                                            intent.PutExtra("TypeChat", "LastMessenger");
                                            intent.PutExtra("ShowEmpty", "no");
                                            intent.PutExtra("ColorChat", mainChatColor);
                                            intent.PutExtra("UserItem", JsonConvert.SerializeObject(item.LastChat));
                                            break;
                                        case "page":
                                            intent = new Intent(Context, typeof(PageChatWindowActivity));
                                            intent.PutExtra("ChatId", item.LastChat.ChatId);
                                            intent.PutExtra("PageId", item.LastChat.PageId);
                                            intent.PutExtra("ShowEmpty", "no");
                                            intent.PutExtra("TypeChat", "");
                                            intent.PutExtra("PageObject", JsonConvert.SerializeObject(item.LastChat));
                                            break;
                                        case "group":
                                            intent = new Intent(Context, typeof(GroupChatWindowActivity));
                                            intent.PutExtra("ChatId", item.LastChat.ChatId);
                                            intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item.LastChat));
                                            intent.PutExtra("ShowEmpty", "no");
                                            intent.PutExtra("GroupId", item.LastChat.GroupId);
                                            break;
                                    }
                                    StartActivity(intent);
                                    break;
                                }
                            case Classes.ItemType.FriendRequest:
                                {
                                    if (item.UserRequestList.Count > 0)
                                    {
                                        var intent = new Intent(Context, typeof(FriendRequestActivity));
                                        Context.StartActivity(intent);
                                    }

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
                        switch (item.Type)
                        {
                            case Classes.ItemType.LastChatNewV:
                                {
                                    OptionsLastMessagesBottomSheet bottomSheet = new OptionsLastMessagesBottomSheet();
                                    Bundle bundle = new Bundle();
                                    switch (item.LastChat.ChatType)
                                    {
                                        case "user":
                                            bundle.PutString("Type", "user");
                                            bundle.PutString("ItemObject", JsonConvert.SerializeObject(item.LastChat));
                                            break;
                                        case "page":
                                            bundle.PutString("Type", "page");
                                            bundle.PutString("ItemObject", JsonConvert.SerializeObject(item.LastChat));
                                            break;
                                        case "group":
                                            bundle.PutString("Type", "group");
                                            bundle.PutString("ItemObject", JsonConvert.SerializeObject(item.LastChat));
                                            break;
                                    }
                                    bottomSheet.Arguments = bundle;
                                    bottomSheet.Show(ChildFragmentManager, bottomSheet.Tag);
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

        #endregion

        #region Load Chat

        private void LoadChat()
        {
            try
            {
                if (ListUtils.UserList.Count > 0)
                {
                    foreach (var chatObject in ListUtils.UserList.Where(chatObject => !chatObject.IsArchive && chatObject.Mute?.Archive == "no"))
                    {
                        if (!chatObject.IsArchive)
                        {
                            var item = WoWonderTools.FilterDataLastChatNewV(chatObject);

                            MAdapter?.LastChatsList?.Add(new Classes.LastChatsClass
                            {
                                LastChat = item,
                                Type = Classes.ItemType.LastChatNewV
                            });
                        }
                    }

                    MAdapter?.NotifyDataSetChanged();
                    Activity?.RunOnUiThread(ShowEmptyPage);
                }

                if (MAdapter?.LastChatsList?.Count == 0)
                {
                    SwipeRefreshLayout.Refreshing = true;
                }

                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService()
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadChatAsync(true), LoadGeneralData });
            else
                ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
        }

        private async Task LoadChatAsync(bool firstRun, string userOffset = "0", string groupOffset = "0", string pageOffset = "0")
        {
            if (MainScrollEvent != null && MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                //if (NoMoreUser && userOffset != "0" && groupOffset != "0" && pageOffset != "0")
                //    return;

                ApiRun = true;

                if (MainScrollEvent != null)
                    MainScrollEvent.IsLoading = true;

                var countList = MAdapter?.LastChatsList?.Count ?? 0;

                var fetch = "users";

                if (AppSettings.LastChatSystem == SystemGetLastChat.Default)
                {
                    if (AppSettings.EnableChatGroup)
                        fetch += ",groups";

                    if (AppSettings.EnableChatPage)
                        fetch += ",pages";
                }

                string limit = "20";
                if (firstRun)
                    limit = "30";

                int apiStatus;
                dynamic respond;

                if (userOffset != "0" && groupOffset != "0" && pageOffset != "0")
                    (apiStatus, respond) = await RequestsAsync.Message.GetChatAsync(fetch, "", userOffset, "10", groupOffset, "10", pageOffset, "10").ConfigureAwait(false);
                else
                    (apiStatus, respond) = await RequestsAsync.Message.GetChatAsync(fetch, "", userOffset, limit, groupOffset, limit, pageOffset, limit);

                if (apiStatus != 200 || respond is not LastChatObject result || result.Data == null)
                {
                    ApiRun = false;
                    if (MainScrollEvent != null)
                        MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    Activity?.RunOnUiThread(() => { LoadCall(result); });
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList == 0 || firstRun)
                        {
                            LoadDataLastChatNewV(result);
                        }
                        else
                        {
                            LoadMoreLastChatNewV(result);
                        }
                    }
                    else
                    {
                        Activity?.RunOnUiThread(() =>
                        {
                            if (MAdapter?.LastChatsList?.Count > 10 && !MRecycler.CanScrollVertically(1) && !NoMoreUser)
                            {
                                ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                                NoMoreUser = true;
                            }
                        });
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
        }

        public void LoadDataLastChatNewV(LastChatObject result)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var countList = MAdapter?.LastChatsList?.Count;
                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            //result.Data = result.Data.OrderBy(o => o.ChatTime).ToList();

                            foreach (var itemChatObject in result.Data)
                            {
                                ChatObject item = WoWonderTools.FilterDataLastChatNewV(itemChatObject);

                                Classes.LastChatsClass checkUser = MAdapter?.LastChatsList?.FirstOrDefault(a => a.LastChat?.ChatId == item.ChatId && a.LastChat?.ChatType == item.ChatType);

                                int index = -1;
                                if (checkUser != null)
                                    index = MAdapter.LastChatsList.IndexOf(checkUser);

                                var archive = WoWonderTools.CheckArchive(item.ChatId, item.ChatType, item.Mute);
                                Classes.LastChatArchive archiveObject = archive.Item1;
                                item.IsArchive = archive.Item2;

                                if (item.IsArchive)
                                    continue;

                                if (checkUser == null)
                                {
                                    Activity?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            if (!item.IsArchive)
                                            {
                                                Console.WriteLine(item.Name);

                                                var checkPin = MAdapter?.LastChatsList?.LastOrDefault(o => o.LastChat != null && o.LastChat.IsPin);
                                                if (checkPin != null)
                                                {
                                                    var toIndex = MAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                                    MAdapter?.LastChatsList?.Insert(toIndex, new Classes.LastChatsClass
                                                    {
                                                        LastChat = item,
                                                        Type = Classes.ItemType.LastChatNewV
                                                    });
                                                    MAdapter?.NotifyItemInserted(toIndex);
                                                    MRecycler.ScrollToPosition(toIndex);
                                                }
                                                else
                                                {
                                                    if (ListUtils.FriendRequestsList.Count > 0)
                                                    {
                                                        MAdapter?.LastChatsList?.Insert(1, new Classes.LastChatsClass
                                                        {
                                                            LastChat = item,
                                                            Type = Classes.ItemType.LastChatNewV
                                                        });
                                                        MAdapter?.NotifyItemInserted(1);
                                                        MRecycler.ScrollToPosition(1);
                                                    }
                                                    else
                                                    {
                                                        MAdapter?.LastChatsList?.Insert(0, new Classes.LastChatsClass
                                                        {
                                                            LastChat = item,
                                                            Type = Classes.ItemType.LastChatNewV
                                                        });
                                                        MAdapter?.NotifyItemInserted(0);
                                                        MRecycler.ScrollToPosition(0);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (archiveObject != null)
                                                {
                                                    if (archiveObject.LastChat.LastMessage.LastMessageClass?.Id != item.LastMessage.LastMessageClass?.Id)
                                                    {
                                                        var checkPin = MAdapter?.LastChatsList?.LastOrDefault(o => o.LastChat != null && o.LastChat.IsPin);
                                                        if (checkPin != null)
                                                        {
                                                            var toIndex = MAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                                            MAdapter?.LastChatsList?.Insert(toIndex, new Classes.LastChatsClass
                                                            {
                                                                LastChat = item,
                                                                Type = Classes.ItemType.LastChatNewV
                                                            });
                                                            MAdapter?.NotifyItemInserted(toIndex);
                                                            MRecycler.ScrollToPosition(toIndex);
                                                        }
                                                        else
                                                        {
                                                            if (ListUtils.FriendRequestsList.Count > 0)
                                                            {
                                                                MAdapter?.LastChatsList?.Insert(1, new Classes.LastChatsClass
                                                                {
                                                                    LastChat = item,
                                                                    Type = Classes.ItemType.LastChatNewV
                                                                });
                                                                MAdapter?.NotifyItemInserted(1);
                                                                MRecycler.ScrollToPosition(1);
                                                            }
                                                            else
                                                            {
                                                                MAdapter?.LastChatsList?.Insert(0, new Classes.LastChatsClass
                                                                {
                                                                    LastChat = item,
                                                                    Type = Classes.ItemType.LastChatNewV
                                                                });
                                                                MAdapter?.NotifyItemInserted(0);
                                                                MRecycler.ScrollToPosition(0);
                                                            }
                                                        }

                                                        ListUtils.ArchiveList.Remove(archiveObject);

                                                        var sqLiteDatabase = new SqLiteDatabase();
                                                        sqLiteDatabase.InsertORDelete_Archive(archiveObject);
                                                    }
                                                }
                                            }

                                            if (item.LastMessage.LastMessageClass?.FromId != UserDetails.UserId && !item.IsMute)
                                            {
                                                var floating = new FloatingObject
                                                {
                                                    ChatType = item.ChatType,
                                                    ChatId = item.ChatId,
                                                    UserId = item.UserId,
                                                    PageId = item.PageId,
                                                    GroupId = item.GroupId,
                                                    Avatar = item.Avatar,
                                                    ChatColor = "",
                                                    LastSeen = item.LastseenStatus,
                                                    LastSeenUnixTime = item.LastseenUnixTime,
                                                    Name = item.Name,
                                                    MessageCount = item.LastMessage.LastMessageClass?.MessageCount ?? "1"
                                                };

                                                switch (item.ChatType)
                                                {
                                                    case "user":
                                                        floating.Name = item.Name;
                                                        break;
                                                    case "page":
                                                        var userAdminPage = item.UserId;
                                                        if (userAdminPage == item.LastMessage.LastMessageClass?.ToData?.UserId)
                                                        {
                                                            floating.Name = item.LastMessage.LastMessageClass?.UserData.Name + "(" + item.PageName + ")";
                                                        }
                                                        else
                                                        {
                                                            floating.Name = item.LastMessage.LastMessageClass?.ToData?.Name + "(" + item.PageName + ")";
                                                        }
                                                        break;
                                                    case "group":
                                                        floating.Name = item.GroupName;
                                                        break;
                                                }

                                                if (UserDetails.ChatHead && InitFloating.CanDrawOverlays(Context) && Methods.AppLifecycleObserver.AppState == "Background")
                                                    GlobalContext?.Floating?.FloatingShow(floating);
                                                //else if (!InitFloating.CanDrawOverlays(this))
                                                //    DisplayChatHeadDialog();
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                                else
                                {
                                    Activity?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            checkUser.LastChat.LastseenUnixTime = item.LastseenUnixTime;
                                            checkUser.LastChat.ChatTime = item.ChatTime;
                                            checkUser.LastChat.Time = item.Time;

                                            if (item.LastMessage.LastMessageClass == null)
                                                return;

                                            if (checkUser.LastChat.LastMessage.LastMessageClass?.Text != item.LastMessage.LastMessageClass?.Text || checkUser.LastChat.LastMessage.LastMessageClass?.Media != item.LastMessage.LastMessageClass?.Media)
                                            {
                                                checkUser.LastChat = item;
                                                checkUser.LastChat.LastMessage = item.LastMessage;
                                                checkUser.LastChat.MessageCount = item.MessageCount;

                                                if (index > 0 && checkUser.LastChat.ChatType == item.ChatType)
                                                {
                                                    if (!item.IsPin)
                                                    {
                                                        var checkPin = MAdapter?.LastChatsList?.LastOrDefault(o => o.LastChat != null && o.LastChat.IsPin);
                                                        if (checkPin != null)
                                                        {
                                                            var toIndex = MAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                                            if (index != toIndex)
                                                            {
                                                                MAdapter?.LastChatsList?.Move(index, toIndex);
                                                                MAdapter?.NotifyItemMoved(index, toIndex);
                                                            }

                                                            MAdapter?.NotifyItemChanged(toIndex, "WithoutBlobText");
                                                        }
                                                        else
                                                        {
                                                            if (ListUtils.FriendRequestsList.Count > 0)
                                                            {
                                                                if (index != 1)
                                                                {
                                                                    MAdapter?.LastChatsList?.Move(index, 1);
                                                                    MAdapter?.NotifyItemMoved(index, 1);
                                                                }

                                                                MAdapter?.NotifyItemChanged(1, "WithoutBlobText");
                                                            }
                                                            else
                                                            {
                                                                if (index != 0)
                                                                {
                                                                    MAdapter?.LastChatsList?.Move(index, 0);
                                                                    MAdapter?.NotifyItemMoved(index, 0);
                                                                }

                                                                MAdapter?.NotifyItemChanged(0, "WithoutBlobText");
                                                            }
                                                        }
                                                    }

                                                    if (item.LastMessage.LastMessageClass.FromId != UserDetails.UserId && !item.IsMute)
                                                    {
                                                        var floating = new FloatingObject
                                                        {
                                                            ChatType = item.ChatType,
                                                            ChatId = item.ChatId,
                                                            UserId = item.UserId,
                                                            PageId = item.PageId,
                                                            GroupId = item.GroupId,
                                                            Avatar = item.Avatar,
                                                            ChatColor = "",
                                                            LastSeen = item.Lastseen,
                                                            LastSeenUnixTime = item.LastseenUnixTime,
                                                            Name = item.Name,
                                                            MessageCount = item.LastMessage.LastMessageClass.MessageCount ?? "1"
                                                        };

                                                        switch (item.ChatType)
                                                        {
                                                            case "user":
                                                                floating.Name = item.Name;
                                                                break;
                                                            case "page":
                                                                var userAdminPage = item.UserId;
                                                                if (userAdminPage == item.LastMessage.LastMessageClass.ToData?.UserId)
                                                                {
                                                                    floating.Name = item.LastMessage.LastMessageClass.UserData?.Name + "(" + item.PageName + ")";
                                                                }
                                                                else
                                                                {
                                                                    floating.Name = item.LastMessage.LastMessageClass.ToData?.Name + "(" + item.PageName + ")";
                                                                }

                                                                break;
                                                            case "group":
                                                                floating.Name = item.GroupName;
                                                                break;
                                                        }

                                                        if (UserDetails.ChatHead && InitFloating.CanDrawOverlays(Context) && Methods.AppLifecycleObserver.AppState == "Background")
                                                            GlobalContext?.Floating?.FloatingShow(floating);
                                                        //else if (!InitFloating.CanDrawOverlays(this))
                                                        //    DisplayChatHeadDialog();
                                                    }
                                                }
                                                else if (index == 0 && checkUser.LastChat.ChatType == item.ChatType)
                                                {
                                                    MAdapter?.NotifyItemChanged(index, "WithoutBlobText");
                                                }
                                            }

                                            if (checkUser.LastChat.LastseenStatus?.ToLower() != item.LastseenStatus?.ToLower())
                                            {
                                                checkUser.LastChat.LastseenStatus = item.LastseenStatus;
                                                checkUser.LastChat.Showlastseen = item.Showlastseen;
                                                checkUser.LastChat.LastseenTimeText = item.LastseenTimeText;

                                                if (index > -1 && checkUser.LastChat.ChatType == item.ChatType)
                                                    MAdapter?.NotifyItemChanged(index, "WithoutBlobLastSeen");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                            }
                        }
                        else
                        {
                            foreach (var itemChatObject in result.Data.Where(chatObject => !chatObject.IsArchive && chatObject.Mute?.Archive == "no"))
                            {
                                var item = WoWonderTools.FilterDataLastChatNewV(itemChatObject);

                                Console.WriteLine(item.Name);

                                var archive = WoWonderTools.CheckArchive(item.ChatId, item.ChatType, item.Mute);
                                Classes.LastChatArchive archiveObject = archive.Item1;
                                item.IsArchive = archive.Item2;

                                if (item.IsArchive)
                                    continue;

                                if (item.IsPin)
                                {
                                    if (ListUtils.FriendRequestsList.Count > 0)
                                    {
                                        MAdapter?.LastChatsList?.Insert(1, new Classes.LastChatsClass
                                        {
                                            LastChat = item,
                                            Type = Classes.ItemType.LastChatNewV
                                        });
                                    }
                                    else
                                    {
                                        MAdapter?.LastChatsList?.Insert(0, new Classes.LastChatsClass
                                        {
                                            LastChat = item,
                                            Type = Classes.ItemType.LastChatNewV
                                        });
                                    }
                                }
                                else
                                {
                                    MAdapter?.LastChatsList?.Add(new Classes.LastChatsClass
                                    {
                                        LastChat = item,
                                        Type = Classes.ItemType.LastChatNewV
                                    });
                                }
                            }
                        }

                        Activity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var list = MAdapter.LastChatsList.OrderByDescending(o => o.LastChat?.ChatTime).ToList();
                                var listPin = list.Where(o => o.LastChat != null && o.LastChat.IsPin).ToList();
                                list.RemoveAll(s => s.LastChat != null && s.LastChat.IsPin);
                                list.InsertRange(0, listPin);
                                MAdapter.LastChatsList = new ObservableCollection<Classes.LastChatsClass>(list);

                                MAdapter?.NotifyDataSetChanged();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                    ApiRun = false;
                    Activity?.RunOnUiThread(ShowEmptyPage);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    ApiRun = false;
                    Activity?.RunOnUiThread(ShowEmptyPage);
                }
            });
        }

        private void LoadMoreLastChatNewV(LastChatObject result)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var countList = MAdapter?.LastChatsList?.Count ?? 0;
                    if (countList > 0)
                    {
                        foreach (var chatsClass in MAdapter?.LastChatsList)
                        {
                            var chk = result.Data.FirstOrDefault(a => a.ChatId == chatsClass?.LastChat?.ChatId);
                            if (chk != null)
                                result.Data.Remove(chk);
                        }
                    }

                    var respondList = result.Data?.Count;
                    if (respondList > 0)
                    {
                        bool add = false;

                        foreach (var itemChatObject in from itemChatObject in result.Data let checkUser = MAdapter.LastChatsList.FirstOrDefault(a => a.LastChat?.ChatId == itemChatObject.ChatId && a.LastChat?.ChatType == itemChatObject.ChatType) where checkUser == null select itemChatObject)
                        {
                            var item = WoWonderTools.FilterDataLastChatNewV(itemChatObject);
                            Classes.LastChatArchive archiveObject = WoWonderTools.CheckArchive(item.ChatId, item.ChatType, item.Mute).Item1;

                            if (!item.IsArchive && item.Mute?.Archive == "no")
                            {
                                add = true;
                                var checkPin = MAdapter?.LastChatsList?.LastOrDefault(o => o.LastChat != null && o.LastChat.IsPin);
                                if (checkPin != null)
                                {
                                    var toIndex = MAdapter.LastChatsList.IndexOf(checkPin) + 1;

                                    MAdapter?.LastChatsList?.Insert(toIndex, new Classes.LastChatsClass
                                    {
                                        LastChat = item,
                                        Type = Classes.ItemType.LastChatNewV
                                    });
                                }
                                else
                                {
                                    if (ListUtils.FriendRequestsList.Count > 0)
                                    {
                                        MAdapter?.LastChatsList?.Insert(1, new Classes.LastChatsClass
                                        {
                                            LastChat = item,
                                            Type = Classes.ItemType.LastChatNewV
                                        });
                                    }
                                    else
                                    {
                                        MAdapter?.LastChatsList?.Insert(0, new Classes.LastChatsClass
                                        {
                                            LastChat = item,
                                            Type = Classes.ItemType.LastChatNewV
                                        });
                                    }
                                }
                            }
                            else
                            {
                                if (archiveObject != null)
                                {
                                    if (archiveObject.LastChat.LastMessage.LastMessageClass?.Id != item.LastMessage.LastMessageClass?.Id)
                                    {
                                        add = true;
                                        MAdapter?.LastChatsList?.Add(new Classes.LastChatsClass
                                        {
                                            Type = Classes.ItemType.LastChatNewV,
                                            LastChat = item
                                        });

                                        ListUtils.ArchiveList.Remove(archiveObject);

                                        var sqLiteDatabase = new SqLiteDatabase();
                                        sqLiteDatabase.InsertORDelete_Archive(archiveObject);
                                    }
                                }
                                else
                                {
                                    add = true;
                                    MAdapter?.LastChatsList?.Add(new Classes.LastChatsClass
                                    {
                                        Type = Classes.ItemType.LastChatNewV,
                                        LastChat = item
                                    });
                                }
                            }
                        }

                        if (add)
                            Activity?.RunOnUiThread(() =>
                            {
                                try
                                {
                                    var list = MAdapter.LastChatsList.OrderByDescending(o => o.LastChat?.ChatTime).ToList();
                                    var listPin = list.Where(o => o.LastChat != null && o.LastChat.IsPin).ToList();
                                    list.RemoveAll(s => s.LastChat != null && s.LastChat.IsPin);
                                    list.InsertRange(0, listPin);
                                    MAdapter.LastChatsList = new ObservableCollection<Classes.LastChatsClass>(list);

                                    MAdapter?.NotifyDataSetChanged();
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                    }
                    else
                    {
                        ApiRun = false;
                        return;
                    }

                    ApiRun = false;
                    Activity?.RunOnUiThread(ShowEmptyPage);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    ApiRun = false;
                    Activity?.RunOnUiThread(ShowEmptyPage);
                }
            });
        }

        //===============================================================

        public static void LoadCall(dynamic respond)
        {
            try
            {
                if (respond == null || !AppSettings.EnableAudioVideoCall || TabbedMainActivity.RunCall || VideoAudioComingCallActivity.IsActive)
                    return;

                string typeCalling = "";
                CallUserObject callUser = null!;

                switch (respond)
                {
                    case LastChatObject chatObject:
                        switch (AppSettings.UseLibrary)
                        {
                            case SystemCall.Twilio:
                                {
                                    var twilioVideoCall = chatObject.VideoCall ?? false;
                                    var twilioAudioCall = chatObject.AudioCall ?? false;

                                    if (twilioVideoCall)
                                    {
                                        typeCalling = "Twilio_video_call";
                                        callUser = chatObject.VideoCallUser?.CallUserClass;
                                    }
                                    else if (twilioAudioCall)
                                    {
                                        typeCalling = "Twilio_audio_call";
                                        callUser = chatObject.AudioCallUser?.CallUserClass;
                                    }

                                    break;
                                }
                            case SystemCall.Agora:
                                {
                                    var agoraCall = chatObject.AgoraCall ?? false;
                                    if (agoraCall)
                                    {
                                        callUser = chatObject.AgoraCallData?.CallUserClass;
                                        if (callUser != null)
                                        {
                                            typeCalling = callUser.Data.Type switch
                                            {
                                                "video" => "Agora_video_call_recieve",
                                                "audio" => "Agora_audio_call_recieve",
                                                _ => typeCalling
                                            };
                                        }
                                    }

                                    break;
                                }
                        }

                        break;

                }

                if (callUser != null)
                {
                    TabbedMainActivity.RunCall = true;
                    Intent intent = null!;
                    switch (typeCalling)
                    {
                        case "Twilio_video_call":
                            intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(callUser));
                            intent.PutExtra("type", "Twilio_video_call");
                            break;
                        case "Twilio_audio_call":
                            intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(callUser));
                            intent.PutExtra("type", "Twilio_audio_call");
                            break;
                        case "Agora_video_call_recieve":
                            intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(callUser));
                            intent.PutExtra("type", "Agora_video_call_recieve");
                            break;
                        case "Agora_audio_call_recieve":
                            intent = new Intent(Application.Context, typeof(VideoAudioComingCallActivity));
                            intent.PutExtra("callUserObject", JsonConvert.SerializeObject(callUser));
                            intent.PutExtra("type", "Agora_audio_call_recieve");
                            break;
                    }

                    if (intent != null && !VideoAudioComingCallActivity.IsActive)
                    {
                        intent.AddFlags(ActivityFlags.NewTask);
                        Application.Context.StartActivity(intent);
                    }
                }
                else
                {
                    if (VideoAudioComingCallActivity.IsActive)
                        VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

                TabbedMainActivity.RunCall = false;

                if (VideoAudioComingCallActivity.IsActive)
                    VideoAudioComingCallActivity.CallActivity?.FinishVideoAudio();

            }
        }

        public void ShowEmptyPage()
        {
            try
            {
                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    if (UserDetails.Socket == null)
                    {
                        UserDetails.Socket = new WoSocketHandler();
                        UserDetails.Socket?.InitStart();
                    }

                    //Connect to socket with access token
                    UserDetails.Socket?.Emit_Join(UserDetails.Username, UserDetails.AccessToken);
                }

                if (SwipeRefreshLayout != null && SwipeRefreshLayout.Refreshing)
                    SwipeRefreshLayout.Refreshing = false;

                if (MainScrollEvent != null)
                    MainScrollEvent.IsLoading = false;

                if (MAdapter?.LastChatsList?.Count > 0)
                {
                    var emptyStateChecker = MAdapter?.LastChatsList?.FirstOrDefault(a => a.Type == Classes.ItemType.EmptyPage);
                    if (emptyStateChecker != null)
                    {
                        var index = MAdapter.LastChatsList.IndexOf(emptyStateChecker);

                        MAdapter?.LastChatsList?.Remove(emptyStateChecker);
                        MAdapter?.NotifyItemRemoved(index);
                    }

                    if (ListUtils.ArchiveList.Count > 0)
                    {
                        var archive = MAdapter?.LastChatsList?.FirstOrDefault(a => a.Type == Classes.ItemType.Archive);
                        if (archive != null)
                        {
                            archive.CountArchive = ListUtils.ArchiveList.Count.ToString();

                            var index = MAdapter.LastChatsList.IndexOf(archive);

                            MAdapter?.LastChatsList?.Move(index, MAdapter.LastChatsList.Count - 1);
                            MAdapter?.NotifyItemMoved(index, MAdapter.LastChatsList.Count - 1);
                        }
                        else
                        {
                            MAdapter?.LastChatsList?.Add(new Classes.LastChatsClass
                            {
                                CountArchive = ListUtils.ArchiveList.Count.ToString(),
                                Type = Classes.ItemType.Archive,
                            });
                            MAdapter?.NotifyItemInserted(MAdapter.LastChatsList.Count - 1);
                        }
                    }
                    else
                    {
                        var archive = MAdapter?.LastChatsList?.FirstOrDefault(a => a.Type == Classes.ItemType.Archive);
                        if (archive != null)
                        {
                            var index = MAdapter.LastChatsList.IndexOf(archive);

                            MAdapter?.LastChatsList?.Remove(archive);
                            MAdapter?.NotifyItemRemoved(index);
                        }
                    }

                    //add insert dbDatabase 
                    List<Classes.LastChatsClass> list = MAdapter?.LastChatsList?.Where(a => a.LastChat != null && a.Type == Classes.ItemType.LastChatNewV).ToList();
                    ListUtils.UserList = new ObservableCollection<ChatObject>(list.Select(lastChatsClass => lastChatsClass.LastChat).ToList());

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.Insert_Or_Update_LastUsersChat(Context, ListUtils.UserList, UserDetails.ChatHead);
                }
                else
                {
                    var emptyStateChecker = MAdapter?.LastChatsList?.FirstOrDefault(q => q.Type == Classes.ItemType.EmptyPage);
                    if (emptyStateChecker == null)
                    {
                        MAdapter?.LastChatsList?.Add(new Classes.LastChatsClass
                        {
                            Type = Classes.ItemType.EmptyPage
                        });
                        MAdapter?.NotifyDataSetChanged();
                    }
                }
                ApiRun = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

                if (SwipeRefreshLayout != null && SwipeRefreshLayout.Refreshing)
                    SwipeRefreshLayout.Refreshing = false;

                if (MainScrollEvent != null)
                    MainScrollEvent.IsLoading = false;
                ApiRun = false;
            }
        }

        #endregion

        //Get General Data Using Api >> Friend Requests and Group Chat Requests
        private async Task LoadGeneralData()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var fetch = "friend_requests";

                    if (AppSettings.EnableChatGroup)
                        fetch += ",group_chat_requests";

                    var (apiStatus, respond) = await RequestsAsync.Global.GetGeneralDataAsync(false, UserDetails.OnlineUsers, UserDetails.DeviceId, "", fetch).ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is GetGeneralDataObject result)
                        {
                            // FriendRequests
                            var respondListFriendRequests = result?.FriendRequests?.Count;
                            if (respondListFriendRequests > 0)
                            {
                                ListUtils.FriendRequestsList = new ObservableCollection<UserDataObject>(result.FriendRequests);

                                var checkList = MAdapter?.LastChatsList?.FirstOrDefault(q => q.Type == Classes.ItemType.FriendRequest);
                                if (checkList == null)
                                {
                                    var friendRequests = new Classes.LastChatsClass
                                    {
                                        UserRequestList = new List<UserDataObject>(),
                                        Type = Classes.ItemType.FriendRequest
                                    };

                                    var list = result.FriendRequests.TakeLast(4).ToList();
                                    if (list.Count > 0)
                                        friendRequests.UserRequestList.AddRange(list);

                                    MAdapter?.LastChatsList?.Insert(0, friendRequests);
                                }
                                else
                                {
                                    if (checkList.UserRequestList.Count < 3)
                                    {
                                        var list = result.FriendRequests.TakeLast(4).ToList();
                                        if (list.Count > 0)
                                            checkList.UserRequestList.AddRange(list);
                                    }
                                }

                                Activity?.RunOnUiThread(() => { MAdapter?.NotifyItemInserted(0); });
                            }
                            else
                            {
                                var checkList = MAdapter?.LastChatsList?.FirstOrDefault(q => q.Type == Classes.ItemType.FriendRequest);
                                if (checkList != null)
                                {
                                    MAdapter?.LastChatsList?.Remove(checkList);
                                    Activity?.RunOnUiThread(() => { MAdapter?.NotifyItemRemoved(0); });
                                }
                            }

                            // Group Requests
                            if (AppSettings.EnableChatGroup)
                            {
                                var respondListGroupRequests = result?.GroupChatRequests?.Count;
                                if (respondListGroupRequests > 0)
                                {
                                    ListUtils.GroupRequestsList = new ObservableCollection<GroupChatRequest>(result.GroupChatRequests);
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