using System;
using Android.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using WoWonder.Helpers.Fonts;

namespace WoWonder.Helpers.Utils
{
    public class EmptyStateInflater
    {
        public AppCompatButton EmptyStateButton;
        private TextView EmptyStateIcon, DescriptionText, TitleText;
        private ImageView EmptyImage;

        public enum Type
        {
            NoConnection,
            NoSearchResult,
            SomThingWentWrong,
            NoUsers,
            NoFollow,
            NoNearBy,
            NoStory,
            NoCall,
            NoGroup,
            NoPage,
            NoMessages,
            NoFiles,
            NoGroupRequest,
            Gif,
            NoSessions,
            NoStartedMessages,
            NoPinnedMessages,
            NoArchive,
            NoBlockedUsers,
        }

        public void InflateLayout(View inflated, Type type)
        {
            try
            {
                EmptyStateIcon = (TextView)inflated.FindViewById(Resource.Id.emtyicon);
                TitleText = (TextView)inflated.FindViewById(Resource.Id.headText);
                DescriptionText = (TextView)inflated.FindViewById(Resource.Id.seconderyText);
                EmptyStateButton = (AppCompatButton)inflated.FindViewById(Resource.Id.button);
                EmptyImage = inflated.FindViewById<ImageView>(Resource.Id.iv_empty);
                EmptyStateIcon.Visibility = ViewStates.Visible;
                EmptyImage.Visibility = ViewStates.Gone;

                switch (type)
                {
                    case Type.NoConnection:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Wifi);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_DescriptionText);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_Button);
                        break;
                    case Type.NoSearchResult:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Search);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_DescriptionText);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_Button);
                        break;
                    case Type.SomThingWentWrong:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Close);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_DescriptionText);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_Button);
                        break;
                    case Type.NoUsers: 
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Gone;
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_user);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_DescriptionText);
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoFollow:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoFollow_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoFollow_DescriptionText);
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoNearBy:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Gone;
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_user);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_NearBy);
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoStory:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_lastStoriess);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_lastStoriess);
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoCall:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Call);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_calls);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_calls);
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoGroup:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, EmptyStateIcon, FontAwesomeIcon.UserFriends);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Group);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Group);
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoMessages:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Chatbubbles);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Lastmessages);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Lastmessages);
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoFiles:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Document);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoAnyMedia);
                        DescriptionText.Text = " ";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoGroupRequest:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, EmptyStateIcon, FontAwesomeIcon.UserFriends);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoAnyGroupRequest);
                        DescriptionText.Text = " ";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.Gif:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Gift);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Gif);
                        DescriptionText.Text = "";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoSessions:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Fingerprint);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Sessions);
                        DescriptionText.Text = "";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoPage:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.CalendarAlt);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Page);
                        DescriptionText.Text = "";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoStartedMessages:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Star);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_StartedMessages);
                        DescriptionText.Text = "";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoPinnedMessages:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeSolid, EmptyStateIcon, FontAwesomeIcon.Thumbtack);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_PinnedMessages);
                        DescriptionText.Text = "";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break; 
                    case Type.NoArchive:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Archive);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_ArchivedChats);
                        DescriptionText.Text = "";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoBlockedUsers:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Person);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Gone;
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_user);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoBlockedUsersDesc);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}