using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.BottomSheet;
using Newtonsoft.Json;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.PageChat;
using WoWonder.Adapters;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonderClient;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.ChatWindow
{
    public class OptionsChatWindowBottomSheet : BottomSheetDialogFragment
    {
        #region Variables Basic

        private ChatWindowActivity ChatWindowContext;
        private GroupChatWindowActivity GroupChatWindowContext;
        private PageChatWindowActivity PageChatWindowContext;

        private ImageView MImgButtonOne, MImgButtonTwo, MImgButtonThree, MImgButtonFour, MImgButtonFive, MImgButtonSix;
        private LinearLayout ReactLayout, CopyLayout, MessageInfoLayout, DeleteMessageLayout, ReplyLayout, ForwardLayout, PinLayout, FavoriteLayout;
        private TextView CopyIcon, MessageInfoIcon, DeleteMessageIcon, ReplyIcon, ForwardIcon, PinIcon, FavoriteIcon;
        private TextView CopyText, MessageInfoText, DeleteMessageText, ReplyText, ForwardText, PinText, FavoriteText;

        private string Page;
        private Holders.TypeClick Type;
        private MessageDataExtra DataMessageObject;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetChatWindowLayout, container, false);
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
                ReactLayout = view.FindViewById<LinearLayout>(Resource.Id.reactLayout);
                MImgButtonOne = view.FindViewById<ImageView>(Resource.Id.imgButtonOne);
                MImgButtonTwo = view.FindViewById<ImageView>(Resource.Id.imgButtonTwo);
                MImgButtonThree = view.FindViewById<ImageView>(Resource.Id.imgButtonThree);
                MImgButtonFour = view.FindViewById<ImageView>(Resource.Id.imgButtonFour);
                MImgButtonFive = view.FindViewById<ImageView>(Resource.Id.imgButtonFive);
                MImgButtonSix = view.FindViewById<ImageView>(Resource.Id.imgButtonSix);

                MImgButtonOne.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Like);
                MImgButtonTwo.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Love);
                MImgButtonThree.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.HaHa);
                MImgButtonFour.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Wow);
                MImgButtonFive.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Sad);
                MImgButtonSix.Click += (sender, args) => ImgButtonOnClick(sender, args, ReactConstants.Angry);

                CopyLayout = view.FindViewById<LinearLayout>(Resource.Id.CopyLayout);
                CopyIcon = view.FindViewById<TextView>(Resource.Id.CopyIcon);
                CopyText = view.FindViewById<TextView>(Resource.Id.CopyText);
                CopyLayout.Click += CopyLayoutOnClick;

                MessageInfoLayout = view.FindViewById<LinearLayout>(Resource.Id.MessageInfoLayout);
                MessageInfoIcon = view.FindViewById<TextView>(Resource.Id.MessageInfoIcon);
                MessageInfoText = view.FindViewById<TextView>(Resource.Id.MessageInfoText);
                MessageInfoLayout.Click += MessageInfoLayoutOnClick;

                DeleteMessageLayout = view.FindViewById<LinearLayout>(Resource.Id.DeleteMessageLayout);
                DeleteMessageIcon = view.FindViewById<TextView>(Resource.Id.DeleteMessageIcon);
                DeleteMessageText = view.FindViewById<TextView>(Resource.Id.DeleteMessageText);
                DeleteMessageLayout.Click += DeleteMessageLayoutOnClick;

                ReplyLayout = view.FindViewById<LinearLayout>(Resource.Id.ReplyLayout);
                ReplyIcon = view.FindViewById<TextView>(Resource.Id.ReplyIcon);
                ReplyText = view.FindViewById<TextView>(Resource.Id.ReplyText);
                ReplyLayout.Click += ReplyLayoutOnClick;

                ForwardLayout = view.FindViewById<LinearLayout>(Resource.Id.ForwardLayout);
                ForwardIcon = view.FindViewById<TextView>(Resource.Id.ForwardIcon);
                ForwardText = view.FindViewById<TextView>(Resource.Id.ForwardText);
                ForwardLayout.Click += ForwardLayoutOnClick;

                PinLayout = view.FindViewById<LinearLayout>(Resource.Id.PinLayout);
                PinIcon = view.FindViewById<TextView>(Resource.Id.PinIcon);
                PinText = view.FindViewById<TextView>(Resource.Id.PinText);
                PinLayout.Click += PinLayoutOnClick;

                FavoriteLayout = view.FindViewById<LinearLayout>(Resource.Id.FavoriteLayout);
                FavoriteIcon = view.FindViewById<TextView>(Resource.Id.FavoriteIcon);
                FavoriteText = view.FindViewById<TextView>(Resource.Id.FavoriteText);
                FavoriteLayout.Click += FavoriteLayoutOnClick;

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, CopyIcon, FontAwesomeIcon.Copy);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, MessageInfoIcon, FontAwesomeIcon.InfoCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, DeleteMessageIcon, FontAwesomeIcon.Trash);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, ReplyIcon, FontAwesomeIcon.Reply);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, ForwardIcon, FontAwesomeIcon.Share);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, PinIcon, FontAwesomeIcon.Thumbtack);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, FavoriteIcon, FontAwesomeIcon.Stars);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        private string LastReact;
        private void ImgButtonOnClick(object sender, EventArgs e, string reactText)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                if (LastReact == reactText)
                    return;

                LastReact = reactText;

                switch (UserDetails.SoundControl)
                {
                    case true:
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("down.mp3");
                        break;
                }

                int resReact = Resource.Drawable.emoji_like;
                DataMessageObject.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                if (reactText == ReactConstants.Like)
                {
                    DataMessageObject.Reaction.Type = "1";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Like").Value?.Id ?? "1";

                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        UserDetails.Socket?.EmitAsync_message_reaction(DataMessageObject.Id, react, UserDetails.AccessToken);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(DataMessageObject.Id, react) });

                    resReact = Resource.Drawable.emoji_like;
                }
                else if (reactText == ReactConstants.Love)
                {
                    DataMessageObject.Reaction.Type = "2";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Love").Value?.Id ?? "2";
                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        UserDetails.Socket?.EmitAsync_message_reaction(DataMessageObject.Id, react, UserDetails.AccessToken);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(DataMessageObject.Id, react) });
                    resReact = Resource.Drawable.emoji_love;
                }
                else if (reactText == ReactConstants.HaHa)
                {
                    DataMessageObject.Reaction.Type = "3";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "HaHa").Value?.Id ?? "3";
                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        UserDetails.Socket?.EmitAsync_message_reaction(DataMessageObject.Id, react, UserDetails.AccessToken);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(DataMessageObject.Id, react) });
                    resReact = Resource.Drawable.emoji_haha;
                }
                else if (reactText == ReactConstants.Wow)
                {
                    DataMessageObject.Reaction.Type = "4";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Wow").Value?.Id ?? "4";
                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        UserDetails.Socket?.EmitAsync_message_reaction(DataMessageObject.Id, react, UserDetails.AccessToken);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(DataMessageObject.Id, react) });
                    resReact = Resource.Drawable.emoji_wow;
                }
                else if (reactText == ReactConstants.Sad)
                {
                    DataMessageObject.Reaction.Type = "5";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Sad").Value?.Id ?? "5";
                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        UserDetails.Socket?.EmitAsync_message_reaction(DataMessageObject.Id, react, UserDetails.AccessToken);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(DataMessageObject.Id, react) });
                    resReact = Resource.Drawable.emoji_sad;
                }
                else if (reactText == ReactConstants.Angry)
                {
                    DataMessageObject.Reaction.Type = "6";
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Angry").Value?.Id ?? "6";
                    if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                        UserDetails.Socket?.EmitAsync_message_reaction(DataMessageObject.Id, react, UserDetails.AccessToken);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Message.ReactMessageAsync(DataMessageObject.Id, react) });
                    resReact = Resource.Drawable.emoji_angry;
                }

                Console.WriteLine(resReact);

                DataMessageObject.Reaction.IsReacted = true;
                DataMessageObject.Reaction.Count++;

                if (Page == "ChatWindow")
                {
                    var dataClass = ChatWindowContext?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == DataMessageObject.Id);
                    if (dataClass != null)
                    {
                        dataClass.MesData = DataMessageObject;

                        ChatWindowContext?.MAdapter.NotifyItemChanged(ChatWindowContext.MAdapter.DifferList.IndexOf(dataClass));
                    }
                }
                else if (Page == "GroupChatWindow")
                {
                    var dataClass = GroupChatWindowContext?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == DataMessageObject.Id);
                    if (dataClass != null)
                    {
                        dataClass.MesData = DataMessageObject;

                        GroupChatWindowContext?.MAdapter.NotifyItemChanged(GroupChatWindowContext.MAdapter.DifferList.IndexOf(dataClass));
                    }
                }
                else if (Page == "PageChatWindow")
                {
                    var dataClass = PageChatWindowContext?.MAdapter?.DifferList?.FirstOrDefault(a => a.MesData?.Id == DataMessageObject.Id);
                    if (dataClass != null)
                    {
                        dataClass.MesData = DataMessageObject;

                        PageChatWindowContext?.MAdapter.NotifyItemChanged(PageChatWindowContext.MAdapter.DifferList.IndexOf(dataClass));
                    }
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CopyLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (DataMessageObject != null && !string.IsNullOrEmpty(DataMessageObject.Text))
                {
                    Methods.CopyToClipboard(Activity, DataMessageObject.Text);
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FavoriteLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.StarMessageItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PinLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.PinMessageItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ForwardLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.ForwardItems();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext?.ForwardItems();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext?.ForwardItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ReplyLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.ReplyItems();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext?.ReplyItems();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext?.ReplyItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DeleteMessageLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.DeleteMessageItems();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext?.DeleteMessageItems();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext?.DeleteMessageItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MessageInfoLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Page == "ChatWindow")
                    ChatWindowContext?.MessageInfoItems();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext?.MessageInfoItems();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext?.MessageInfoItems();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void LoadDataChat()
        {
            try
            {
                Page = Arguments.GetString("Page") ?? ""; //ChatWindow ,GroupChatWindow,PageChatWindow
                if (Page == "ChatWindow")
                    ChatWindowContext = ChatWindowActivity.GetInstance();
                else if (Page == "GroupChatWindow")
                    GroupChatWindowContext = GroupChatWindowActivity.GetInstance();
                else if (Page == "PageChatWindow")
                    PageChatWindowContext = PageChatWindowActivity.GetInstance();

                Type = JsonConvert.DeserializeObject<Holders.TypeClick>(Arguments.GetString("Type") ?? "");
                DataMessageObject = JsonConvert.DeserializeObject<MessageDataExtra>(Arguments.GetString("ItemObject") ?? "");
                if (DataMessageObject != null)
                {
                    CopyLayout.Visibility = Type == Holders.TypeClick.Text ? ViewStates.Visible : ViewStates.Gone;

                    if (DataMessageObject.Position == "right")
                    {
                        MessageInfoLayout.Visibility = ViewStates.Visible;
                        DeleteMessageLayout.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        MessageInfoLayout.Visibility = ViewStates.Gone;
                        DeleteMessageLayout.Visibility = ViewStates.Gone;
                    }

                    ReplyLayout.Visibility = AppSettings.EnableReplyMessageSystem ? ViewStates.Visible : ViewStates.Gone;

                    ForwardLayout.Visibility = AppSettings.EnableForwardMessageSystem ? ViewStates.Visible : ViewStates.Gone;

                    if (AppSettings.EnablePinMessageSystem && Page == "ChatWindow")
                    {
                        PinLayout.Visibility = ViewStates.Visible;
                        PinText.Text = DataMessageObject.IsPinned ? GetText(Resource.String.Lbl_UnPin) : GetText(Resource.String.Lbl_Pin);
                    }
                    else
                        PinLayout.Visibility = ViewStates.Gone;

                    if (AppSettings.EnableFavoriteMessageSystem && Page == "ChatWindow")
                    {
                        FavoriteLayout.Visibility = ViewStates.Visible;
                        FavoriteText.Text = DataMessageObject.Fav == "yes" ? GetText(Resource.String.Lbl_UnFavorite) : GetText(Resource.String.Lbl_Favorite);
                    }
                    else
                        FavoriteLayout.Visibility = ViewStates.Gone;

                    if (AppSettings.EnableReactionMessageSystem && DataMessageObject.Position == "left")
                    {
                        ReactLayout.Visibility = ViewStates.Visible;
                        switch (AppSettings.ReactionTheme)
                        {
                            case PostButtonSystem.ReactionDefault:
                                Glide.With(Activity).Load(Resource.Drawable.gif_like).Apply(new RequestOptions()).Into(MImgButtonOne);
                                Glide.With(Activity).Load(Resource.Drawable.gif_love).Apply(new RequestOptions()).Into(MImgButtonTwo);
                                Glide.With(Activity).Load(Resource.Drawable.gif_haha).Apply(new RequestOptions()).Into(MImgButtonThree);
                                Glide.With(Activity).Load(Resource.Drawable.gif_wow).Apply(new RequestOptions()).Into(MImgButtonFour);
                                Glide.With(Activity).Load(Resource.Drawable.gif_sad).Apply(new RequestOptions()).Into(MImgButtonFive);
                                Glide.With(Activity).Load(Resource.Drawable.gif_angry).Apply(new RequestOptions()).Into(MImgButtonSix);
                                break;
                            case PostButtonSystem.ReactionSubShine:
                            default:
                                Glide.With(Activity).Load(Resource.Drawable.like).Apply(new RequestOptions().FitCenter()).Into(MImgButtonOne);
                                Glide.With(Activity).Load(Resource.Drawable.love).Apply(new RequestOptions().FitCenter()).Into(MImgButtonTwo);
                                Glide.With(Activity).Load(Resource.Drawable.haha).Apply(new RequestOptions().FitCenter()).Into(MImgButtonThree);
                                Glide.With(Activity).Load(Resource.Drawable.wow).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFour);
                                Glide.With(Activity).Load(Resource.Drawable.sad).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFive);
                                Glide.With(Activity).Load(Resource.Drawable.angry).Apply(new RequestOptions().FitCenter()).Into(MImgButtonSix);
                                break;
                        }
                    }
                    else
                    {
                        ReactLayout.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}