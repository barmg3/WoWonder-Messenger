using System;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Graphics.Drawable;
using Com.Aghajari.Emojiview.Listener;
using Com.Aghajari.Emojiview.Search;
using Com.Aghajari.Emojiview.View;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.Story;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.StickersView
{
    public class EmojisViewActions : SimplePopupAdapter, View.IOnClickListener 
    {
        private readonly Activity ActivityContext;

        private readonly ChatWindowActivity ChatWindow;
        private readonly GroupChatWindowActivity GroupActivityView;
        private readonly PageChatWindowActivity PageActivityView;
        private readonly StoryReplyActivity StoryReplyActivity;

#pragma warning disable CS0618
        public readonly AXEmojiPopup Popup;
#pragma warning restore CS0618
        private readonly AXEmojiEditText AxEmojiEditText;
        private readonly ImageView EmojisViewImage;

        private readonly string TypePage;

        private bool IsShowing = false;

        public EmojisViewActions(Activity activity, string typePage, AXEmojiPager emojiPager, AXEmojiEditText editText, ImageView image)
        {
            try
            {
                ActivityContext = activity;
                TypePage = typePage;

                switch (typePage)
                {
                    // Create your fragment here
                    case "ChatWindowActivity":
                        ChatWindow = (ChatWindowActivity)activity;
                        break;
                    case "PageChatWindowActivity":
                        PageActivityView = (PageChatWindowActivity)activity;
                        break;
                    case "GroupChatWindowActivity":
                        GroupActivityView = (GroupChatWindowActivity)activity;
                        break;
                    case "StoryReplyActivity":
                        StoryReplyActivity = (StoryReplyActivity)activity;
                        break;
                }

#pragma warning disable CS0618
                Popup = new AXEmojiPopup(emojiPager);
#pragma warning restore CS0618
                AxEmojiEditText = editText;
                EmojisViewImage = image;

                EmojisViewImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.ParseColor("#444444"));

                AxEmojiEditText.SetOnClickListener(this);
                EmojisViewImage.SetOnClickListener(this);
                Popup.SetPopupListener(this);
                Popup.SearchView = new AXEmojiSearchView(activity, emojiPager.GetPage(0));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdateButton(bool emoji)
        {
            try
            {
                if (IsShowing == emoji) return;
                IsShowing = emoji;

                if (emoji)
                {
                    Drawable dr = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.ic_action_keyboard);
                    DrawableCompat.SetTint(DrawableCompat.Wrap(dr), Color.Black);
                    EmojisViewImage.SetImageDrawable(dr);
                }
                else
                {
                    Drawable dr = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.icon_smile_vector);
                    DrawableCompat.SetTint(DrawableCompat.Wrap(dr), Color.Black);
                    EmojisViewImage.SetImageDrawable(dr);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                if (v?.Id == AxEmojiEditText?.Id)
                {
                    if (Popup.IsShowing)
                    {
                        Popup.Toggle();

                        switch (TypePage)
                        {
                            // Create your fragment here
                            case "ChatWindowActivity":
                                ChatWindow?.RemoveButtonFragment();
                                break;
                            case "PageChatWindowActivity":
                                PageActivityView?.RemoveButtonFragment();
                                break;
                            case "GroupChatWindowActivity":
                                GroupActivityView?.RemoveButtonFragment();
                                break;
                            case "StoryReplyActivity":
                                StoryReplyActivity?.RemoveButtonFragment();
                                break;
                        }
                    }
                }
                else if (v?.Id == EmojisViewImage?.Id)
                {
                    Popup.Toggle();

                    switch (TypePage)
                    {
                        // Create your fragment here
                        case "ChatWindowActivity":
                            ChatWindow?.RemoveButtonFragment();
                            break;
                        case "PageChatWindowActivity":
                            PageActivityView?.RemoveButtonFragment();
                            break;
                        case "GroupChatWindowActivity":
                            GroupActivityView?.RemoveButtonFragment();
                            break;
                        case "StoryReplyActivity":
                            StoryReplyActivity?.RemoveButtonFragment();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnShow()
        {
            try
            {
                base.OnShow();
                UpdateButton(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDismiss()
        {
            try
            {
                base.OnDismiss();
                UpdateButton(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnKeyboardOpened(int height)
        {
            try
            {
                base.OnKeyboardOpened(height);
                UpdateButton(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnKeyboardClosed()
        {
            try
            {
                base.OnKeyboardClosed();
                UpdateButton(Popup.IsShowing);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}