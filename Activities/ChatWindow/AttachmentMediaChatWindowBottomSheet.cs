using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using MaterialDialogsCore;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.PageChat;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.ChatWindow
{
    public class AttachmentMediaChatWindowBottomSheet : BottomSheetDialogFragment, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private ChatWindowActivity ChatWindowContext;
        private GroupChatWindowActivity GroupChatWindowContext;
        private PageChatWindowActivity PageChatWindowContext;

        private LinearLayout ImageLayout, VideoLayout, FileLayout, MusicLayout, GifLayout, ContactLayout, LocationLayout;
        private LinearLayout ImageContainer, VideoContainer, FileContainer, MusicContainer, GifContainer, ContactContainer, LocationContainer;
        private TextView ImageIcon, VideoIcon, FileIcon, MusicIcon, GifIcon, ContactIcon, LocationIcon;
        private TextView ImageText, VideoText, FileText, MusicText, GifText, ContactText, LocationText;

        private string Page;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetAttachmentMediaChatWindowLayout, container, false);
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

                LoadDataChat();
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
                ImageLayout = view.FindViewById<LinearLayout>(Resource.Id.ImageLayout);
                ImageContainer = view.FindViewById<LinearLayout>(Resource.Id.ImageContainer);
                ImageIcon = view.FindViewById<TextView>(Resource.Id.IconImage);
                ImageText = view.FindViewById<TextView>(Resource.Id.NameImage);
                ImageLayout.Click += ImageLayoutOnClick;

                VideoLayout = view.FindViewById<LinearLayout>(Resource.Id.VideoLayout);
                VideoContainer = view.FindViewById<LinearLayout>(Resource.Id.VideoContainer);
                VideoIcon = view.FindViewById<TextView>(Resource.Id.IconVideo);
                VideoText = view.FindViewById<TextView>(Resource.Id.NameVideo);
                VideoLayout.Click += VideoLayoutOnClick;

                FileLayout = view.FindViewById<LinearLayout>(Resource.Id.FileLayout);
                FileContainer = view.FindViewById<LinearLayout>(Resource.Id.FileContainer);
                FileIcon = view.FindViewById<TextView>(Resource.Id.IconFile);
                FileText = view.FindViewById<TextView>(Resource.Id.NameFile);
                FileLayout.Click += FileLayoutOnClick;

                MusicLayout = view.FindViewById<LinearLayout>(Resource.Id.MusicLayout);
                MusicContainer = view.FindViewById<LinearLayout>(Resource.Id.MusicContainer);
                MusicIcon = view.FindViewById<TextView>(Resource.Id.IconMusic);
                MusicText = view.FindViewById<TextView>(Resource.Id.NameMusic);
                MusicLayout.Click += MusicLayoutOnClick;

                GifLayout = view.FindViewById<LinearLayout>(Resource.Id.GifLayout);
                GifContainer = view.FindViewById<LinearLayout>(Resource.Id.GifContainer);
                GifIcon = view.FindViewById<TextView>(Resource.Id.IconGif);
                GifText = view.FindViewById<TextView>(Resource.Id.NameGif);
                GifLayout.Click += GifLayoutOnClick;

                ContactLayout = view.FindViewById<LinearLayout>(Resource.Id.ContactLayout);
                ContactContainer = view.FindViewById<LinearLayout>(Resource.Id.ContactContainer);
                ContactIcon = view.FindViewById<TextView>(Resource.Id.IconContact);
                ContactText = view.FindViewById<TextView>(Resource.Id.NameContact);
                ContactLayout.Click += ContactLayoutOnClick;

                LocationLayout = view.FindViewById<LinearLayout>(Resource.Id.LocationLayout);
                LocationContainer = view.FindViewById<LinearLayout>(Resource.Id.LocationContainer);
                LocationIcon = view.FindViewById<TextView>(Resource.Id.IconLocation);
                LocationText = view.FindViewById<TextView>(Resource.Id.NameLocation);
                LocationLayout.Click += LocationLayoutOnClick;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, ImageIcon, FontAwesomeIcon.Images);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, VideoIcon, FontAwesomeIcon.Video);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, FileIcon, FontAwesomeIcon.FilePdf);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, MusicIcon, FontAwesomeIcon.Music);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, GifIcon, FontAwesomeIcon.Gift);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, ContactIcon, FontAwesomeIcon.AddressBook);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, LocationIcon, FontAwesomeIcon.MapMarkedAlt);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private void ImageLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_ImageGallery));
                arrayAdapter.Add(GetText(Resource.String.Lbl_TakeImageFromCamera));

                dialogList.Title(GetString(Resource.String.Lbl_SelectImageFrom));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void VideoLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));
                arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));

                dialogList.Title(GetString(Resource.String.Lbl_SelectVideoFrom));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FileLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_File));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_File));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_File));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MusicLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Music));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Music));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Music));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GifLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Gif));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Gif));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Gif));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ContactLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Contact));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Contact));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Contact));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LocationLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Location));
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Location));
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(null, null, 0, Activity.GetText(Resource.String.Lbl_Location));
                        break;
                }
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext.OnSelection(dialog, itemView, position, itemString);
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext.OnSelection(dialog, itemView, position, itemString);
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext.OnSelection(dialog, itemView, position, itemString);
                        break;
                }
                Dismiss();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void LoadDataChat()
        {
            try
            {
                Page = Arguments.GetString("Page") ?? ""; //ChatWindow ,GroupChatWindow,PageChatWindow
                switch (Page)
                {
                    case "ChatWindow":
                        ChatWindowContext = ChatWindowActivity.GetInstance();
                        break;
                    case "GroupChatWindow":
                        GroupChatWindowContext = GroupChatWindowActivity.GetInstance();
                        break;
                    case "PageChatWindow":
                        PageChatWindowContext = PageChatWindowActivity.GetInstance();
                        break;
                }

                if (!AppSettings.ShowButtonImage)
                    ImageLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonVideo || !WoWonderTools.CheckAllowedFileSharingInServer("Video"))
                    VideoLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonAttachFile || !WoWonderTools.CheckAllowedFileSharingInServer("File"))
                    FileLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonMusic || !WoWonderTools.CheckAllowedFileSharingInServer("Audio"))
                    MusicLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonGif)
                    GifLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonContact)
                    ContactLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowButtonLocation)
                    LocationLayout.Visibility = ViewStates.Gone;

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}