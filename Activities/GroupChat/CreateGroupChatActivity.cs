using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Com.Aghajari.Emojiview.View;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Google.Android.Material.AppBar;
using Google.Android.Material.FloatingActionButton;
using Java.IO;
using TheArtOfDev.Edmodo.Cropper;
using WoWonder.Activities.Base;
using WoWonder.Activities.GroupChat.Adapter;
using WoWonder.Activities.StickersView;
using WoWonder.Activities.Tab;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.GroupChat;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.GroupChat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateGroupChatActivity : BaseActivity, AppBarLayout.IOnOffsetChangedListener, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private TextView BtnExitGroup;
        private AXEmojiEditText TxtGroupName;
        private ImageView ChatEmojImage;
        private ImageView ImageGroup;
        private AppCompatButton BtnImage, BtnDeleteGroup;
        private MembersAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;

        private string GroupPathImage = "", UsersIds;
        private List<UserDataObject> UserList;

        private AppBarLayout AppBarLayout;
        private CollapsingToolbarLayout CollapsingToolbar;
        private FloatingActionButton BtnAdd;
        private int Position;
        private TabbedMainActivity GlobalContext;

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
                SetContentView(Resource.Layout.CreateGroupChatLayout);

                GlobalContext = TabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
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
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                CollapsingToolbar = (CollapsingToolbarLayout)FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = "";

                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);
                AppBarLayout.AddOnOffsetChangedListener(this);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.userRecyler);
                TxtGroupName = FindViewById<AXEmojiEditText>(Resource.Id.groupName);
                ImageGroup = FindViewById<ImageView>(Resource.Id.groupCover);
                BtnImage = FindViewById<AppCompatButton>(Resource.Id.btn_selectimage);

                ChatEmojImage = FindViewById<ImageView>(Resource.Id.emojiicon);

                BtnAdd = FindViewById<FloatingActionButton>(Resource.Id.fab);

                BtnDeleteGroup = FindViewById<AppCompatButton>(Resource.Id.deleteGroupButton);
                BtnDeleteGroup.Visibility = ViewStates.Gone;

                BtnExitGroup = FindViewById<TextView>(Resource.Id.exitGroupButton);
                BtnExitGroup.Visibility = ViewStates.Gone;

                if (AppSettings.SetTabDarkTheme)
                {
                    TxtGroupName.SetTextColor(Color.White);
                    TxtGroupName.SetHintTextColor(Color.White);
                }
                else
                {
                    TxtGroupName.SetTextColor(Color.ParseColor("#444444"));
                    TxtGroupName.SetHintTextColor(Color.ParseColor("#444444"));
                }

                if (AppSettings.SetTabDarkTheme)
                    EmojisViewTools.LoadDarkTheme();
                else
                    EmojisViewTools.LoadTheme(AppSettings.MainColor);

                EmojisViewTools.MStickerView = false;
                EmojisViewTools.LoadView(this, TxtGroupName, "", ChatEmojImage);
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
                    toolbar.Title = GetString(Resource.String.Lbl_CreateGroup);
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
                MAdapter = new MembersAdapter(this, true)
                {
                    UserList = new ObservableCollection<UserDataObject>(),
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<UserDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default  
                MAdapter.UserList.Add(new UserDataObject
                {
                    UserId = "0",
                    Avatar = "addImage",
                    Name = GetString(Resource.String.Lbl_AddParticipants),
                    About = GetString(Resource.String.Lbl_Group_Add_Description),
                });
                MAdapter.NotifyDataSetChanged();
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
                    BtnAdd.Click += TxtAddOnClick;

                    BtnImage.Click += BtnImageOnClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    MAdapter.ItemLongClick += MAdapterOnItemLongClick;
                }
                else
                {
                    BtnAdd.Click -= TxtAddOnClick;

                    BtnImage.Click -= BtnImageOnClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    MAdapter.ItemLongClick -= MAdapterOnItemLongClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnItemLongClick(object sender, MembersAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item == null) return;
                    if (item.Avatar == "addImage") return;

                    Position = e.Position;
                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialog.Title(GetString(Resource.String.Lbl_Remove) + " " + WoWonderTools.GetNameFinal(item));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.Build().Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, MembersAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item == null) return;
                    if (item.Avatar != "addImage") return;
                    StartActivityForResult(new Intent(this, typeof(MentionActivity)), 3);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnImageOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery(); //requestCode >> 500 => Image Gallery
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtGroupName.Text) || string.IsNullOrWhiteSpace(TxtGroupName.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short);
                        return;
                    }

                    if (TxtGroupName.Text.Length < 4 && TxtGroupName.Text.Length > 15)
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_ErrorLengthGroupName), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(GroupPathImage))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short);
                        return;
                    }

                    var list = MAdapter.UserList.Where(a => a.Avatar != "addImage").ToList();
                    if (list.Count == 0)
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PleaseSelectUser), ToastLength.Long);
                    }
                    else
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        UsersIds = "";
                        foreach (var user in list)
                        {
                            UsersIds += user.UserId + ",";
                        }

                        UsersIds = UsersIds.Remove(UsersIds.Length - 1, 1);

                        var (apiStatus, respond) = await RequestsAsync.GroupChat.CreateGroupChatAsync(TxtGroupName.Text, UsersIds, GroupPathImage);
                        if (apiStatus == 200)
                        {
                            if (respond is CreateGroupChatObject result)
                            {
                                AndHUD.Shared.ShowSuccess(this);

                                //Add new item to my Group list
                                //remove item to my Group list  
                                if (AppSettings.LastChatSystem == SystemGetLastChat.Default)
                                {
                                    var adapter = GlobalContext?.LastChatTab?.MAdapter;
                                    if (result.Data?.Count > 0 && adapter != null)
                                    {
                                        adapter.LastChatsList.Insert(0, new Classes.LastChatsClass
                                        {
                                            LastChat = result.Data.FirstOrDefault(),
                                            Type = Classes.ItemType.LastChatNewV
                                        });
                                        adapter.NotifyDataSetChanged();

                                        GlobalContext.LastChatTab.ShowEmptyPage();
                                    }
                                }
                                else
                                {
                                    var adapter = GlobalContext?.LastGroupChatsTab?.MAdapter;
                                    if (result.Data?.Count > 0 && adapter != null)
                                    {
                                        adapter.LastChatsList.Insert(0, new Classes.LastChatsClass
                                        {
                                            LastChat = result.Data.FirstOrDefault(),
                                            Type = Classes.ItemType.LastChatNewV
                                        });
                                        adapter.NotifyDataSetChanged();

                                        GlobalContext?.LastGroupChatsTab?.ShowEmptyPage();
                                    }
                                }

                                Finish();
                            }
                        }
                        else
                        {
                            Methods.DisplayAndHudErrorResult(this, respond);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        private void TxtAddUserOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivityForResult(new Intent(this, typeof(MentionActivity)), 3);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    //If its from Camera or Gallery
                    case CropImage.CropImageActivityRequestCode when resultCode == Result.Ok:
                        {
                            var result = CropImage.GetActivityResult(data);

                            if (resultCode == Result.Ok)
                            {
                                if (result.IsSuccessful)
                                {
                                    var resultUri = result.Uri;

                                    if (!string.IsNullOrEmpty(resultUri.Path))
                                    {
                                        GroupPathImage = resultUri.Path;
                                        File file2 = new File(resultUri.Path);
                                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                        Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImageGroup);
                                    }
                                    else
                                    {
                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                                    }
                                }
                            }

                            break;
                        }
                    case 3:
                        {
                            UserList = MentionActivity.MAdapter.MentionList.Where(a => a.Selected).ToList();

                            UsersIds = "";
                            foreach (var user in UserList)
                            {
                                UsersIds += user.UserId + ",";

                                var dataUser = MAdapter.UserList.FirstOrDefault(attachments => attachments.UserId == user.UserId);
                                if (dataUser == null)
                                {
                                    MAdapter.UserList.Insert(1, user);
                                }
                            }
                            UsersIds = UsersIds.Remove(UsersIds.Length - 1, 1);

                            MAdapter.NotifyDataSetChanged();
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery();
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region appBarLayout

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            try
            {
                int minHeight = ViewCompat.GetMinimumHeight(CollapsingToolbar) * 2;
                float scale = (float)(minHeight + verticalOffset) / minHeight;

                BtnAdd.ScaleX = scale >= 0 ? scale : 0;
                BtnAdd.ScaleY = scale >= 0 ? scale : 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void OpenDialogGallery()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && PermissionsController.CheckPermissionStorage() && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var itemUser = MAdapter.GetItem(Position);
                        if (itemUser != null)
                        {
                            MAdapter.UserList.Remove(itemUser);
                            MAdapter.NotifyItemRemoved(Position);
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }
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