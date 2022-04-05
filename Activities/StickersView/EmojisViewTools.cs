using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Graphics.Drawable;
using Com.Aghajari.Emojiview;
using Com.Aghajari.Emojiview.Listener;
using Com.Aghajari.Emojiview.Sticker;
using Com.Aghajari.Emojiview.Utils;
using Com.Aghajari.Emojiview.View;
using WoWonder.Activities.ChatWindow;
using WoWonder.Activities.GroupChat;
using WoWonder.Activities.PageChat;
using WoWonder.Activities.StickersView.Page;
using WoWonder.Activities.Story;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;

namespace WoWonder.Activities.StickersView
{
    public class EmojisViewTools
    {
        public static bool MEmojiView = true;
        public static bool MSingleEmojiView = false;
        public static bool MStickerView = true;
        public static bool MCustomView = false;
        public static bool MFooterView = true;
        public static bool MCustomFooter = true;
        public static bool MWhiteCategory = true;

        private static bool DarkMode = false;
        public static AXStickerView StickerView;
        private static EmojisViewActions EmojisViewActions;

        public static void LoadTheme(string mainColor)
        {
            try
            {
                // release theme
                DarkMode = false;
                AXEmojiManager.ResetTheme();

                var color = Color.ParseColor(mainColor);

                // set EmojiView Theme
                AXEmojiManager.EmojiViewTheme.FooterEnabled = MFooterView && !MCustomFooter;
                AXEmojiManager.EmojiViewTheme.SelectionColor = color;
                AXEmojiManager.EmojiViewTheme.FooterSelectedItemColor = color;
                AXEmojiManager.StickerViewTheme.SelectionColor = color;

                if (MWhiteCategory)
                {
                    AXEmojiManager.EmojiViewTheme.SelectionColor = Color.Transparent;
                    AXEmojiManager.EmojiViewTheme.SelectedColor = color;
                    AXEmojiManager.EmojiViewTheme.CategoryColor = Color.White;
                    AXEmojiManager.EmojiViewTheme.FooterBackgroundColor = Color.White;
                    AXEmojiManager.EmojiViewTheme.SetAlwaysShowDivider(true);

                    AXEmojiManager.StickerViewTheme.SelectedColor = color;
                    AXEmojiManager.StickerViewTheme.CategoryColor = Color.White;
                    AXEmojiManager.StickerViewTheme.SetAlwaysShowDivider(true);
                }

                AXEmojiManager.BackspaceCategoryEnabled = !MCustomFooter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void LoadDarkTheme()
        {
            try
            {
                // release theme
                DarkMode = true;
                AXEmojiManager.ResetTheme();

                // set EmojiView Theme
                AXEmojiManager.EmojiViewTheme.FooterEnabled = MFooterView && !MCustomFooter;
                AXEmojiManager.EmojiViewTheme.SelectionColor = Color.ParseColor("#82ADD9");
                AXEmojiManager.EmojiViewTheme.SelectedColor = Color.ParseColor("#82ADD9");
                AXEmojiManager.EmojiViewTheme.FooterSelectedItemColor = Color.ParseColor("#82ADD9");
                AXEmojiManager.EmojiViewTheme.BackgroundColor = Color.ParseColor("#1E2632");
                AXEmojiManager.EmojiViewTheme.CategoryColor = Color.ParseColor("#1E2632");
                AXEmojiManager.EmojiViewTheme.FooterBackgroundColor = Color.ParseColor("#1E2632");
                AXEmojiManager.EmojiViewTheme.VariantPopupBackgroundColor = Color.ParseColor("#232D3A");
                AXEmojiManager.EmojiViewTheme.VariantDividerEnabled = false;
                AXEmojiManager.EmojiViewTheme.DividerColor = Color.ParseColor("#1B242D");
                AXEmojiManager.EmojiViewTheme.DefaultColor = Color.ParseColor("#677382");
                AXEmojiManager.EmojiViewTheme.TitleColor = Color.ParseColor("#677382");

                AXEmojiManager.StickerViewTheme.SelectionColor = Color.ParseColor("#82ADD9");
                AXEmojiManager.StickerViewTheme.SelectedColor = Color.ParseColor("#82ADD9");
                AXEmojiManager.StickerViewTheme.BackgroundColor = Color.ParseColor("#1E2632");
                AXEmojiManager.StickerViewTheme.CategoryColor = Color.ParseColor("#1E2632");
                AXEmojiManager.StickerViewTheme.DividerColor = Color.ParseColor("#1B242D");
                AXEmojiManager.StickerViewTheme.DefaultColor = Color.ParseColor("#677382");

                if (MWhiteCategory)
                {
                    AXEmojiManager.EmojiViewTheme.SelectionColor = Color.Transparent;
                    AXEmojiManager.EmojiViewTheme.CategoryColor = Color.ParseColor("#232D3A");
                    AXEmojiManager.EmojiViewTheme.FooterBackgroundColor = Color.ParseColor("#232D3A");
                    AXEmojiManager.EmojiViewTheme.SetAlwaysShowDivider(true);

                    AXEmojiManager.StickerViewTheme.CategoryColor = Color.ParseColor("#232D3A");
                    AXEmojiManager.StickerViewTheme.SetAlwaysShowDivider(true);
                }
                AXEmojiManager.BackspaceCategoryEnabled = !MCustomFooter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static AXEmojiPager LoadView(Activity context, AXEmojiEditText editText, string typePage, ImageView chatEmojImage)
        {
            try
            {
                AXEmojiPager emojiPager = new AXEmojiPager(context);

                if (MSingleEmojiView)
                {
                    //add single emoji view
                    AXSingleEmojiView singleEmojiView = new AXSingleEmojiView(context);
                    emojiPager.AddPage(singleEmojiView, Resource.Drawable.icon_smile_vector);
                }

                if (MEmojiView)
                {
                    // add emoji view (with viewpager)
                    AXEmojiView emojiView = new AXEmojiView(context);
                    emojiPager.AddPage(emojiView, Resource.Drawable.icon_smile_vector);
                }

                if (MStickerView)
                {
                    //add Sticker View
                    StickerView = new AXStickerView(context, "stickers", new WoWonderProvider());
                    emojiPager.AddPage(StickerView, Resource.Drawable.icon_stickers_vector);
                    StickerView.SetOnStickerActionsListener(new MyStickerActions(context, typePage));
                }

                if (MCustomView)
                {
                    emojiPager.AddPage(new LoadingView(context), Resource.Drawable.msg_round_load_m);
                }

                editText.SetEmojiSize(Utils.DpToPx(context, 23));
                // set target emoji edit text to emojiViewPager
                emojiPager.EditText = editText;

                emojiPager.SetSwipeWithFingerEnabled(true);

                if (MCustomFooter)
                {
                    InitCustomFooter2(context, typePage, emojiPager);
                }
                else
                {
                    emojiPager.SetLeftIcon(Resource.Drawable.icon_search_vector);
                    emojiPager.SetOnFooterItemClicked(new MyFooterItemClicked());
                }
                 
                EmojisViewActions = new EmojisViewActions(context, typePage, emojiPager, editText, chatEmojImage);

                return emojiPager;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public static void InitCustomFooter(Activity context, AXEmojiPager emojiPager)
        {
            try
            {
                FrameLayout footer = new FrameLayout(context);
                //Drawable drawable = AppCompatResources.GetDrawable(context, Resource.Drawable.circle);
                //if (DarkMode) DrawableCompat.SetTint(DrawableCompat.Wrap(drawable), Color.ParseColor("#1B242D"));
                footer.SetBackgroundColor(DarkMode ? Color.ParseColor("#1B242D") : Color.ParseColor("#efefef"));
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    footer.Elevation = Utils.Dp(context, 4);
                }

                var lp = new AXEmojiLayout.LayoutParams(Utils.Dp(context, 48), Utils.Dp(context, 48))
                {
                    RightMargin = Utils.Dp(context, 12),
                    BottomMargin = Utils.Dp(context, 12),
                    Gravity = GravityFlags.Right | GravityFlags.Bottom,
                };
                footer.LayoutParameters = lp;
                emojiPager.SetCustomFooter(footer, true);

                ImageView img = new ImageView(context);
                var lp2 = new FrameLayout.LayoutParams(Utils.Dp(context, 22), Utils.Dp(context, 22))
                {
                    Gravity = GravityFlags.Center
                };
                footer.AddView(img, lp2);

                emojiPager.SetOnEmojiPageChangedListener(new MyEmojiPagerPageChanged(context, footer, img));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void InitCustomFooter2(Activity context, string typePage, AXEmojiPager emojiPager)
        {
            try
            {
                LayoutInflater layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                View newRow = layoutInflater.Inflate(Resource.Layout.CustomFooterEmojisView, null, false);

                var lp = new AXEmojiLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, Utils.Dp(context, 40))
                {
                    //RightMargin = Utils.Dp(context, 12),
                    //BottomMargin = Utils.Dp(context, 12),
                    Gravity = GravityFlags.Bottom,
                };
                newRow.LayoutParameters = lp;

                emojiPager.SetCustomFooter(newRow, true); 
                emojiPager.SetOnEmojiPageChangedListener(new MyEmojiPagerPageChanged2(context, typePage, emojiPager, newRow));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyEmojiPagerPageChanged : Java.Lang.Object, IOnEmojiPagerPageChanged, View.IOnClickListener
        {
            private readonly Activity Context;
            private readonly FrameLayout Footer;
            private readonly ImageView Img;
            public MyEmojiPagerPageChanged(Activity context, FrameLayout footer, ImageView img)
            {
                try
                {
                    Context = context;
                    Footer = footer;
                    Img = img; 
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnPageChanged(AXEmojiPager emojiPager, AXEmojiBase @base, int position)
            {
                try
                {
                    Drawable drawable;
                    if (AXEmojiManager.IsAXEmojiView(@base))
                    {
                        drawable = AppCompatResources.GetDrawable(Context, Resource.Drawable.emoji_backspace);
                        Utils.EnableBackspaceTouch(Footer, emojiPager.EditText);
                        Footer.SetOnClickListener(null);
                    }
                    else
                    {
                        drawable = AppCompatResources.GetDrawable(Context, Resource.Drawable.icon_search_vector);
                        Footer.SetOnTouchListener(null);
                        Footer.SetOnClickListener(this);
                    }
                    DrawableCompat.SetTint(DrawableCompat.Wrap(drawable), AXEmojiManager.EmojiViewTheme.FooterItemColor);
                    Img.SetImageDrawable(drawable);
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
                    ToastUtils.ShowToast(Application.Context, "Search Clicked", ToastLength.Short);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        private class MyEmojiPagerPageChanged2 : Java.Lang.Object, IOnEmojiPagerPageChanged, View.IOnClickListener
        {
            private readonly Activity Context;
            private readonly string TypePage;

            private readonly ChatWindowActivity ChatWindow;
            private readonly GroupChatWindowActivity GroupActivityView;
            private readonly PageChatWindowActivity PageActivityView;
            private readonly StoryReplyActivity StoryReplyActivity;

            private readonly View Footer;
            private readonly AXEmojiPager EmojiPager;
            private readonly ImageView IconSearch, IconSmile, IconStickers, IconEmojiBackspace;
            public MyEmojiPagerPageChanged2(Activity context, string typePage, AXEmojiPager emojiPager, View footer)
            {
                try
                {
                    Context = context;
                    EmojiPager = emojiPager;
                    TypePage = typePage;
                    Footer = footer;
                      
                    switch (typePage)
                    {
                        // Create your fragment here
                        case "ChatWindowActivity":
                            ChatWindow = ChatWindowActivity.GetInstance();
                            break;
                        case "PageChatWindowActivity":
                            PageActivityView = PageChatWindowActivity.GetInstance();
                            break;
                        case "GroupChatWindowActivity":
                            GroupActivityView = GroupChatWindowActivity.GetInstance();
                            break;
                        case "StoryReplyActivity":
                            StoryReplyActivity = StoryReplyActivity.GetInstance();
                            break;
                    }

                    IconSearch = footer.FindViewById<ImageView>(Resource.Id.icon_search);
                    IconSmile = footer.FindViewById<ImageView>(Resource.Id.icon_smile);
                    IconStickers = footer.FindViewById<ImageView>(Resource.Id.icon_stickers);
                    IconEmojiBackspace = footer.FindViewById<ImageView>(Resource.Id.icon_emoji_backspace);

                    IconSearch?.SetOnClickListener(this);
                    IconSmile?.SetOnClickListener(this);
                    IconStickers?.SetOnClickListener(this);
                     
                    if (!MStickerView)
                    {
                        IconStickers.Visibility = ViewStates.Gone;
                    } 
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnPageChanged(AXEmojiPager emojiPager, AXEmojiBase @base, int position)
            {
                try
                {
                    if (AXEmojiManager.IsAXEmojiView(@base))
                    {
                        IconSearch.Visibility = ViewStates.Visible;
                       
                        IconSmile.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                        IconStickers.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.ParseColor("#efefef") : Color.ParseColor("#282828"));

                        var drawable = AppCompatResources.GetDrawable(Context, Resource.Drawable.emoji_backspace);
                        IconEmojiBackspace.SetImageDrawable(drawable);
                        IconEmojiBackspace.Tag = "Backspace";
                         
                        Utils.EnableBackspaceTouch(IconEmojiBackspace, emojiPager.EditText);
                    }
                    else
                    {
                        IconSearch.Visibility = ViewStates.Invisible;

                        IconStickers.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                        IconSmile.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.ParseColor("#efefef") : Color.ParseColor("#282828"));
                         
                        var drawable = AppCompatResources.GetDrawable(Context, Resource.Drawable.icon_search_vector);
                        IconEmojiBackspace.SetImageDrawable(drawable);
                        IconEmojiBackspace.Tag = "AddStickers";

                        IconEmojiBackspace.SetOnTouchListener(null);
                        IconEmojiBackspace?.SetOnClickListener(this);
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
                    if (v.Id == IconSearch.Id)
                    {
                        EmojisViewActions?.Popup?.ShowSearchView();
                    }
                    else if (v.Id == IconSmile.Id)
                    {
                        EmojiPager.ViewPager.SetCurrentItem(0, false);
                    }
                    else if (v.Id == IconStickers.Id)
                    {
                        EmojiPager.ViewPager.SetCurrentItem(1, false); 
                    }
                    else if(v.Id == IconEmojiBackspace.Id)
                    {
                        if (IconEmojiBackspace.Tag?.ToString() == "AddStickers")
                        {
                            BrowseStickersFragment fragment = new BrowseStickersFragment();
                            Bundle bundle = new Bundle();
                            bundle.PutString("TypePage", TypePage);
                            fragment.Arguments = bundle;

                            switch (TypePage)
                            {
                                // Create your fragment here
                                case "ChatWindowActivity":
                                    fragment.Show(ChatWindow.SupportFragmentManager, fragment.Tag);
                                    break;
                                case "PageChatWindowActivity":
                                    fragment.Show(PageActivityView.SupportFragmentManager, fragment.Tag);
                                    break;
                                case "GroupChatWindowActivity":
                                    fragment.Show(GroupActivityView.SupportFragmentManager, fragment.Tag);
                                    break;
                                case "StoryReplyActivity":
                                    fragment.Show(StoryReplyActivity.SupportFragmentManager, fragment.Tag);
                                    break;
                            } 
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        private class MyStickerActions : Java.Lang.Object, IOnStickerActions
        {
            private readonly string TypePage;

            public MyStickerActions(Activity activity, string typePage)
            {
                try
                {
                    TypePage = typePage;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnClick(View view, Sticker sticker, bool fromRecent)
            {
                try
                {
                    //ToastUtils.ShowToast(Application.Context, sticker.ToString() + " clicked!", ToastLength.Short);
                    var stickerUrl = sticker.ToString();

                    new StickerItemClickListener(TypePage).StickerAdapterOnOnItemClick(stickerUrl);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public bool OnLongClick(View view, Sticker sticker, bool fromRecent)
            {
                return false;
            }
        }

        private class MyFooterItemClicked : Java.Lang.Object, AXEmojiPager.IOnFooterItemClicked
        {
            public void OnClick(View view, bool leftIcon)
            {
                try
                {
                    if (leftIcon)
                    {
                        //ToastUtils.ShowToast(Application.Context, "Search Clicked!", ToastLength.Short);
                        EmojisViewActions?.Popup?.ShowSearchView();
                    }

                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }
    }
}