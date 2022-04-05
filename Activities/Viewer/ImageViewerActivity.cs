using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using Newtonsoft.Json;
using Sephiroth.ImageZoom;
using WoWonder.Activities.ChatWindow;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.Library.Anjo.Stories.DragView;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Viewer
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/DragTransparentBlack", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ImageViewerActivity : AppCompatActivity, DragToClose.IDragListener, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private DragToClose DragToClose;
        private ImageViewTouch Image;
        private ImageView MoreButton;

        private string Id, MediaFile;
        private MessageDataExtra MesData;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.ImageViewerLayout);

                Id = Intent?.GetStringExtra("Id") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                LoadData();
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

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                base.OnTrimMemory(level);
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
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
                base.OnLowMemory();
                GC.Collect(GC.MaxGeneration);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu 

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                Image = FindViewById<ImageViewTouch>(Resource.Id.imageview);
                MoreButton = FindViewById<ImageView>(Resource.Id.moreButton);

                DragToClose = FindViewById<DragToClose>(Resource.Id.drag_to_close);
                DragToClose.SetCloseOnClick(true);
                DragToClose.SetDragListener(this);
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
                    toolbar.Title = "";
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    MoreButton.Click += MoreButtonOnClick;
                }
                else
                {
                    MoreButton.Click -= MoreButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MoreButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (MesData.Position == "right")
                {
                    arrayAdapter.Add(GetText(Resource.String.Lbl_MessageInfo));
                }

                if (AppSettings.EnableForwardMessageSystem)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Forward));

                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));

                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data 

        private void LoadData()
        {
            try
            {
                MesData = JsonConvert.DeserializeObject<MessageDataExtra>(Intent?.GetStringExtra("SelectedItem") ?? "");
                if (MesData != null)
                {
                    var fileName = MesData.Media.Split('/').Last();
                    MediaFile = WoWonderTools.GetFile(Id, Methods.Path.FolderDcimImage, fileName, MesData.Media , "other");

                    string imageFile = Methods.MultiMedia.CheckFileIfExits(MediaFile);
                    if (imageFile != "File Dont Exists")
                    {
                        File file2 = new File(MediaFile);
                        var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                        if (imageFile.Contains(".gif"))
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).FitCenter()).Into(Image);
                        else
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(Image);
                    }
                    else
                    {
                        if (MediaFile.Contains(".gif"))
                            Glide.With(this).Load(MediaFile).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).FitCenter()).Into(Image);
                        else
                            Glide.With(this).Load(MediaFile).Apply(new RequestOptions()).Into(Image);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MaterialDialog

        public async void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Lbl_MessageInfo))
                {
                    var intent = new Intent(this, typeof(MessageInfoActivity));
                    intent.PutExtra("UserId", Id);
                    intent.PutExtra("MainChatColor", !string.IsNullOrEmpty(MesData.ChatColor) ? MesData.ChatColor : AppSettings.MainColor ?? AppSettings.MainColor);
                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(MesData));
                    StartActivity(intent);
                }
                else if (itemString == GetText(Resource.String.Lbl_Forward))
                {
                    var intent = new Intent(this, typeof(ForwardMessagesActivity));
                    intent.PutExtra("SelectedItem", JsonConvert.SerializeObject(MesData));
                    StartActivity(intent);
                }
                else if (itemString == GetText(Resource.String.Lbl_Share))
                {
                    string urlImage = MediaFile;
                    var fileName = urlImage?.Split('/').Last();

                    switch (AppSettings.AllowDownloadMedia)
                    {
                        case true:
                            await ShareFileImplementation.ShareRemoteFile(this, urlImage, urlImage, fileName, GetText(Resource.String.Lbl_SendTo));
                            break;
                        default:
                            await CrossShare.Current.Share(new ShareMessage
                            {
                                Title = "",
                                Text = urlImage,
                                Url = urlImage
                            });
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                Image.Alpha = offset;
                MoreButton.Alpha = offset;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnViewClosed()
        {
            try
            {
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}