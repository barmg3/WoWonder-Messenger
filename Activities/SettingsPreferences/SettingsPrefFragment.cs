using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using WoWonder.Activities.DefaultUser;
using WoWonder.Activities.SettingsPreferences.Custom;
using WoWonder.Activities.SettingsPreferences.General;
using WoWonder.Activities.SettingsPreferences.InviteFriends;
using WoWonder.Activities.Tab;
using WoWonder.Frameworks.Floating;
using WoWonder.Library.OneSignalNotif;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Requests; 
using Exception = System.Exception;
using String = System.String;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.SettingsPreferences
{
    public class SettingsPrefFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback, MaterialDialog.IListCallbackMultiChoice
    {
        private GeneralCustomPreference EditProfile, BlockedUsers, AccountPref, PasswordPref, GeneralInvitePref, GeneralCallPref, SupportHelpPref, SupportLogoutPref, SupportDeleteAccountPref, SupportReportPref, AboutMePref, TwoFactorPref, ManageSessionsPref,RateAppPref;
        //private ListPreference LangPref;
        private GeneralCustomPreference PrivacyFollowPref, PrivacyBirthdayPref, PrivacyMessagePref;
        private CustomSwitchPreference NotificationPopupPref, ShowOnlineUsersPref, ChatHeadsPref;
        private CustomCheckBoxPreference NotificationPlaySoundPref;
        private GeneralCustomPreference WallpaperPref, NightMode;
        private GeneralCustomPreference StorageConnectedMobilePref, StorageConnectedWiFiPref;
        private CustomSwitchPreference FingerprintLockPref;
        private static Activity ActivityContext;
        private string SAbout = "",SWhoCanFollowMe = "0", SWhoCanMessageMe = "0", SWhoCanSeeMyBirthday = "0", TypeDialog = "";

        #region General

        public SettingsPrefFragment(Activity activity)
        {
            try
            {
                ActivityContext = activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsThemeDark) : new ContextThemeWrapper(ActivityContext, Resource.Style.SettingsTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = base.OnCreateView(localInflater, container, savedInstanceState);

                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            {
                // Load the preferences from an XML resource
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs);

                MainSettings.SharedData = PreferenceManager.SharedPreferences;

                InitComponent();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                AddOrRemoveEvent(false);
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

        private void InitComponent()
        {
            try
            {
                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
                  
                EditProfile = (GeneralCustomPreference)FindPreference("editprofile_key");
                //LangPref = (ListPreference) FindPreference("Lang_key");
                AboutMePref = (GeneralCustomPreference)FindPreference("about_me_key");
                BlockedUsers = (GeneralCustomPreference)FindPreference("blocked_key");
                AccountPref = (GeneralCustomPreference)FindPreference("editAccount_key");
                PasswordPref = (GeneralCustomPreference)FindPreference("editpassword_key");
                GeneralInvitePref = (GeneralCustomPreference)FindPreference("invite_key");
                GeneralCallPref = (GeneralCustomPreference)FindPreference("Call_key");
                TwoFactorPref = (GeneralCustomPreference)FindPreference("Twofactor_key");
                ManageSessionsPref = (GeneralCustomPreference)FindPreference("ManageSessions_key");
                WallpaperPref = (GeneralCustomPreference)FindPreference("Wallpaper_key");
                NightMode = (GeneralCustomPreference)FindPreference("Night_Mode_key");
                SupportReportPref = (GeneralCustomPreference)FindPreference("Report_key");
                SupportHelpPref = (GeneralCustomPreference)FindPreference("help_key");
                SupportLogoutPref = (GeneralCustomPreference)FindPreference("logout_key");
                SupportDeleteAccountPref = (GeneralCustomPreference)FindPreference("deleteaccount_key");
                PrivacyFollowPref = (GeneralCustomPreference)FindPreference("whocanfollow_key");
                PrivacyMessagePref = (GeneralCustomPreference)FindPreference("whocanMessage_key");
                PrivacyBirthdayPref = (GeneralCustomPreference)FindPreference("whocanseemybirthday_key"); 
                NotificationPopupPref = (CustomSwitchPreference)FindPreference("notifications_key");
                ShowOnlineUsersPref = (CustomSwitchPreference)FindPreference("onlineuser_key");
                ChatHeadsPref = (CustomSwitchPreference)FindPreference("chatheads_key");
                NotificationPlaySoundPref = (CustomCheckBoxPreference)FindPreference("checkBox_PlaySound_key");
                RateAppPref = (GeneralCustomPreference)FindPreference("RateApp_key");
                StorageConnectedMobilePref = (GeneralCustomPreference)FindPreference("StorageConnectedMobile_key");
                StorageConnectedWiFiPref = (GeneralCustomPreference)FindPreference("StorageConnectedWiFi_key");
                FingerprintLockPref = (CustomSwitchPreference)FindPreference("FingerprintLock_key");

                //==================CategoryAccount_key======================
                var mCategoryAccount = (PreferenceCategory)FindPreference("CategoryAccount_key");
                if (!AppSettings.ShowSettingsAccount)
                    mCategoryAccount.RemovePreference(AccountPref);

                if (!AppSettings.ShowSettingsBlockedUsers)
                    mCategoryAccount.RemovePreference(BlockedUsers);

                //==================SecurityAccount_key======================
                var mCategorySecurity = (PreferenceCategory)FindPreference("SecurityAccount_key");
                if (!AppSettings.ShowSettingsPassword)
                    mCategorySecurity.RemovePreference(PasswordPref);

                if (!AppSettings.ShowSettingsTwoFactor)
                    mCategorySecurity.RemovePreference(TwoFactorPref);

                if (!AppSettings.ShowSettingsManageSessions)
                    mCategorySecurity.RemovePreference(ManageSessionsPref);

                if (!AppSettings.ShowSettingsFingerprintLock)
                    mCategorySecurity.RemovePreference(FingerprintLockPref);

                //==================category_General======================
                var mCategoryGeneral = (PreferenceCategory)FindPreference("category_General");

                if (!AppSettings.InvitationSystem)
                    mCategoryGeneral.RemovePreference(GeneralInvitePref);

                //==================Theme_key======================
                var mCategoryTheme = (PreferenceCategory)FindPreference("Theme_key");

                if (!AppSettings.ShowSettingsWallpaper)
                    mCategoryTheme.RemovePreference(WallpaperPref);

                //==================category_Support======================
                var mCategorySupport = (PreferenceCategory)FindPreference("category_Support");

                if (!AppSettings.ShowSettingsRateApp)
                    mCategorySupport.RemovePreference(RateAppPref);
                 
                if (!AppSettings.ShowSettingsDeleteAccount)
                    mCategorySupport.RemovePreference(SupportDeleteAccountPref);
                  
                //Add Click event to Preferences
                EditProfile.Intent = new Intent(ActivityContext, typeof(MyProfileActivity));
                BlockedUsers.Intent = new Intent(ActivityContext, typeof(BlockedUsersActivity));
                AccountPref.Intent = new Intent(ActivityContext, typeof(MyAccountActivity));
                PasswordPref.Intent = new Intent(ActivityContext, typeof(PasswordActivity));
                GeneralInvitePref.Intent = new Intent(ActivityContext, typeof(InviteFriendsActivity));

                //Update Preferences data on Load
                OnSharedPreferenceChanged(MainSettings.SharedData, "about_me_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "whocanfollow_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "whocanMessage_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "whocanseemybirthday_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "notifications_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "chatheads_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "onlineuser_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "checkBox_PlaySound_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "Night_Mode_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "FingerprintLock_key");
                 
                EditProfile.IconSpaceReserved = false;
                //LangPref.IconSpaceReserved = false;
                AboutMePref.IconSpaceReserved = false;
                BlockedUsers.IconSpaceReserved = false;
                AccountPref.IconSpaceReserved = false;
                PasswordPref.IconSpaceReserved = false;
                GeneralInvitePref.IconSpaceReserved = false;
                GeneralCallPref.IconSpaceReserved = false;
                TwoFactorPref.IconSpaceReserved = false;
                ManageSessionsPref.IconSpaceReserved = false;
                WallpaperPref.IconSpaceReserved = false;
                NightMode.IconSpaceReserved = false;
                SupportReportPref.IconSpaceReserved = false;
                SupportHelpPref.IconSpaceReserved = false;
                SupportLogoutPref.IconSpaceReserved = false;
                SupportDeleteAccountPref.IconSpaceReserved = false;
                PrivacyFollowPref.IconSpaceReserved = false;
                PrivacyMessagePref.IconSpaceReserved = false;
                PrivacyBirthdayPref.IconSpaceReserved = false;
                NotificationPopupPref.IconSpaceReserved = false;
                ShowOnlineUsersPref.IconSpaceReserved = false;
                ChatHeadsPref.IconSpaceReserved = false;
                NotificationPlaySoundPref.IconSpaceReserved = false;
                StorageConnectedMobilePref.IconSpaceReserved = false;
                StorageConnectedWiFiPref.IconSpaceReserved = false;
                FingerprintLockPref.IconSpaceReserved = false; 
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
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        SupportReportPref.PreferenceClick += SupportReportPrefOnPreferenceClick;
                        SupportHelpPref.PreferenceClick += SupportHelpPrefOnPreferenceClick;
                        //Add OnChange event to Preferences
                        //LangPref.PreferenceChange += Lang_Pref_PreferenceChange;
                        NotificationPopupPref.PreferenceChange += NotificationPopupPrefPreferenceChange;
                        ChatHeadsPref.PreferenceChange += ChatHeadsPrefOnPreferenceChange;
                        ShowOnlineUsersPref.PreferenceChange += ShowOnlineUsers_Pref_PreferenceChange;
                        NotificationPlaySoundPref.PreferenceChange += NotificationPlaySoundPrefPreferenceChange;
                        //Event Click Items
                        SupportLogoutPref.PreferenceClick += SupportLogout_OnPreferenceClick;
                        SupportDeleteAccountPref.PreferenceClick += SupportDeleteAccountPrefOnPreferenceClick;
                        WallpaperPref.PreferenceClick += WallpaperPrefOnPreferenceClick;
                        TwoFactorPref.PreferenceClick += TwoFactorPrefOnPreferenceClick;
                        ManageSessionsPref.PreferenceClick += ManageSessionsPrefOnPreferenceClick;
                        RateAppPref.PreferenceClick += RateAppPrefOnPreferenceClick;
                        FingerprintLockPref.PreferenceChange += FingerprintLockPrefOnPreferenceChange;
                        break;
                    default:
                        SupportReportPref.PreferenceClick -= SupportReportPrefOnPreferenceClick;
                        SupportHelpPref.PreferenceClick -= SupportHelpPrefOnPreferenceClick;
                        //Add OnChange event to Preferences
                        //LangPref.PreferenceChange -= Lang_Pref_PreferenceChange;
                        NotificationPopupPref.PreferenceChange -= NotificationPopupPrefPreferenceChange;
                        ChatHeadsPref.PreferenceChange -= ChatHeadsPrefOnPreferenceChange;
                        ShowOnlineUsersPref.PreferenceChange -= ShowOnlineUsers_Pref_PreferenceChange;
                        NotificationPlaySoundPref.PreferenceChange -= NotificationPlaySoundPrefPreferenceChange;
                        //Event Click Items
                        SupportLogoutPref.PreferenceClick -= SupportLogout_OnPreferenceClick;
                        SupportDeleteAccountPref.PreferenceClick -= SupportDeleteAccountPrefOnPreferenceClick;
                        WallpaperPref.PreferenceClick -= WallpaperPrefOnPreferenceClick;
                        TwoFactorPref.PreferenceClick -= TwoFactorPrefOnPreferenceClick;
                        ManageSessionsPref.PreferenceClick -= ManageSessionsPrefOnPreferenceClick;
                        RateAppPref.PreferenceClick -= RateAppPrefOnPreferenceClick;
                        FingerprintLockPref.PreferenceChange -= FingerprintLockPrefOnPreferenceChange;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {
                var datauser = ListUtils.MyProfileList.FirstOrDefault();
                switch (key)
                {
                    case "about_me_key":
                    {
                        // Set summary to be the user-description for the selected value
                        Preference etp = FindPreference("about_me_key");

                        if (datauser != null)
                        {
                            SAbout = WoWonderTools.GetAboutFinal(datauser);

                            MainSettings.SharedData?.Edit()?.PutString("about_me_key", SAbout)?.Commit(); 
                            etp.Summary = SAbout;
                        }

                        string getValue = MainSettings.SharedData?.GetString("about_me_key", SAbout);
                        etp.Summary = getValue;
                        break;
                    }
                    case "whocanfollow_key":
                    {
                        // Set summary to be the user-description for the selected value
                        Preference etp = FindPreference("whocanfollow_key");

                        string getValue = MainSettings.SharedData?.GetString("whocanfollow_key", datauser?.FollowPrivacy ?? String.Empty);
                        switch (getValue)
                        {
                            case "0":
                                etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                                SWhoCanFollowMe = "0";
                                break;
                            case "1":
                                etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_i_Follow);
                                SWhoCanFollowMe = "1";
                                break;
                            default:
                                etp.Summary = getValue;
                                break;
                        }

                        break;
                    }
                    case "whocanMessage_key":
                    {
                        // Set summary to be the user-description for the selected value
                        Preference etp = FindPreference("whocanMessage_key");

                        string getValue = MainSettings.SharedData?.GetString("whocanMessage_key", datauser?.MessagePrivacy ?? String.Empty);
                        switch (getValue)
                        {
                            case "0":
                                etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                                SWhoCanMessageMe = "0";
                                break;
                            case "1":
                                etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_i_Follow);
                                SWhoCanMessageMe = "1";
                                break;
                            case "2":
                                etp.Summary = ActivityContext.GetText(Resource.String.Lbl_No_body);
                                SWhoCanMessageMe = "2";
                                break;
                            default:
                                etp.Summary = getValue;
                                break;
                        }

                        break;
                    }
                    case "whocanseemybirthday_key":
                    {
                        // Set summary to be the user-description for the selected value
                        Preference etp = FindPreference("whocanseemybirthday_key");

                        string getValue = MainSettings.SharedData?.GetString("whocanseemybirthday_key", datauser?.BirthPrivacy ?? String.Empty);
                        switch (getValue)
                        {
                            case "0":
                                etp.Summary = ActivityContext.GetText(Resource.String.Lbl_Everyone);
                                SWhoCanSeeMyBirthday = "0";
                                break;
                            case "1":
                                etp.Summary = ActivityContext.GetText(Resource.String.Lbl_People_i_Follow);
                                SWhoCanSeeMyBirthday = "1";
                                break;
                            case "2":
                                etp.Summary = ActivityContext.GetText(Resource.String.Lbl_No_body);
                                SWhoCanSeeMyBirthday = "1";
                                break;
                            default:
                                etp.Summary = getValue;
                                break;
                        }

                        break;
                    }
                    case "notifications_key":
                    {
                        bool getValue = MainSettings.SharedData?.GetBoolean("notifications_key", true) ?? true;
                        NotificationPopupPref.Checked = getValue;
                        break;
                    }
                    case "onlineuser_key":
                    {
                        bool getValue = MainSettings.SharedData?.GetBoolean("onlineuser_key", true) ?? true;
                        ShowOnlineUsersPref.Checked = UserDetails.OnlineUsers = getValue;
                        break;
                    }
                    case "chatheads_key":
                    {
                        bool getValue = MainSettings.SharedData?.GetBoolean("chatheads_key", InitFloating.CanDrawOverlays(ActivityContext)) ?? false;
                        ChatHeadsPref.Checked = getValue;
                        UserDetails.ChatHead = getValue;
                        break;
                    }
                    case "checkBox_PlaySound_key":
                    {
                        bool getValue = MainSettings.SharedData?.GetBoolean("checkBox_PlaySound_key", true) ?? true;
                        NotificationPlaySoundPref.Checked = getValue;
                        UserDetails.SoundControl = getValue;
                        break;
                    }
                    case "FingerprintLock_key":
                    {
                        bool getValue = MainSettings.SharedData?.GetBoolean("FingerprintLock_key", false) ?? false;
                        FingerprintLockPref.Checked = getValue;
                        UserDetails.FingerprintLock = getValue;
                        break;
                    }
                    case "Night_Mode_key":
                    {
                        // Set summary to be the user-description for the selected value
                        Preference etp = FindPreference("Night_Mode_key");

                        string getValue = MainSettings.SharedData?.GetString("Night_Mode_key", string.Empty);
                        if (getValue == MainSettings.LightMode)
                        {
                            etp.Summary = ActivityContext.GetString(Resource.String.Lbl_Light);
                        }
                        else if (getValue == MainSettings.DarkMode)
                        {
                            etp.Summary = ActivityContext.GetString(Resource.String.Lbl_Dark);
                        }
                        else if (getValue == MainSettings.DefaultMode)
                        {
                            etp.Summary = ActivityContext.GetString(Resource.String.Lbl_SetByBattery);
                        }
                        else
                        {
                            etp.Summary = getValue;
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Event
        private void RateAppPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                StoreReviewApp store = new StoreReviewApp();
                store.OpenStoreReviewPage(Activity.PackageName);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        //TwoFactor
        private void TwoFactorPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(TwoFactorAuthActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //ManageSessions
        private void ManageSessionsPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(ManageSessionsActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Wallpaper
        private void WallpaperPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(WallpaperActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        //Delete Account
        private void SupportDeleteAccountPrefOnPreferenceClick(object sender, Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(DeleteAccountActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void ShowOnlineUsers_Pref_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                CustomSwitchPreference etp = (CustomSwitchPreference)sender;
                var value = e.NewValue.ToString();
                etp.Checked = UserDetails.OnlineUsers = bool.Parse(value);

                MainSettings.SharedData?.Edit()?.PutBoolean("onlineuser_key", etp.Checked)?.Commit();

                if (AppSettings.ConnectionTypeChat == InitializeWoWonder.ConnectionType.Socket)
                {
                    if (UserDetails.OnlineUsers)
                        UserDetails.Socket?.Emit_loggedintEvent(UserDetails.AccessToken);
                    else
                        UserDetails.Socket?.Emit_loggedoutEvent(UserDetails.AccessToken); 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Help
        private void SupportHelpPrefOnPreferenceClick(object sender,Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                SupportHelpPref.Intent = new Intent(ActivityContext, typeof(LocalWebViewActivity));
                SupportHelpPref.Intent.PutExtra("Type", ActivityContext.GetText(Resource.String.Lbl_Help));
                SupportHelpPref.Intent.PutExtra("URL", InitializeWoWonder.WebsiteUrl + "/terms/about-us");
                ActivityContext.StartActivity(SupportHelpPref.Intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Report >> Contact Us
        private void SupportReportPrefOnPreferenceClick(object sender,Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                SupportReportPref.Intent = new Intent(ActivityContext, typeof(LocalWebViewActivity));
                SupportReportPref.Intent.PutExtra("Type", ActivityContext.GetText(Resource.String.Lbl_Report_Problem));
                SupportReportPref.Intent.PutExtra("URL", InitializeWoWonder.WebsiteUrl + "/contact-us");
                ActivityContext.StartActivity(SupportReportPref.Intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Logout
        private void SupportLogout_OnPreferenceClick(object sender,Preference.PreferenceClickEventArgs preferenceClickEventArgs)
        {
            try
            {
                TypeDialog = "Logout";

                var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                dialog.Title(Resource.String.Lbl_Warning);
                dialog.Content(ActivityContext.GetText(Resource.String.Lbl_Are_you_logout));
                dialog.PositiveText(ActivityContext.GetText(Resource.String.Lbl_Ok)).OnPositive(this);
                dialog.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Notification >> Play Sound 
        private void NotificationPlaySoundPrefPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    CustomCheckBoxPreference etp = (CustomCheckBoxPreference) sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        UserDetails.SoundControl = true;
                    }
                    else
                    {
                        UserDetails.SoundControl = false;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Notifcation >> Popup 
        private void NotificationPopupPrefPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    CustomSwitchPreference etp = (CustomSwitchPreference) sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        OneSignalNotification.Instance.RegisterNotificationDevice(ActivityContext);
                    }
                    else
                    {
                        OneSignalNotification.Instance.UnRegisterNotificationDevice();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //ChatHeads
        private void ChatHeadsPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    CustomSwitchPreference etp = (CustomSwitchPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    UserDetails.ChatHead = etp.Checked;

                    OpenChatHead();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void FingerprintLockPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            try
            {
                if (e.Handled)
                {
                    CustomSwitchPreference etp = (CustomSwitchPreference)sender;
                    var value = e.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    UserDetails.FingerprintLock = etp.Checked; 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public static void OpenChatHead()
        {
            try
            {
                if (UserDetails.ChatHead && !InitFloating.CanDrawOverlays(ActivityContext))
                {
                    Intent intent = new Intent(Settings.ActionManageOverlayPermission, Uri.Parse("package:" + Application.Context.PackageName));
                    ActivityContext.StartActivityForResult(intent, InitFloating.ChatHeadDataRequestCode);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Lang
        //private void Lang_Pref_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Handled)
        //        {
        //            ListPreference etp = (ListPreference) sender;
        //            var value = e.NewValue;

        //            MainSettings.SetApplicationLang(value.ToString());

        //            ToastUtils.ShowToast(ActivityContext, GetText(Resource.String.Lbl_Closed_App), ToastLength.Long);

        //            Intent intent = new Intent(ActivityContext, typeof(SplashScreenActivity));
        //            intent.AddCategory(Intent.CategoryHome);
        //            intent.SetAction(Intent.ActionMain);
        //            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
        //            ActivityContext.StartActivity(intent);
        //            ActivityContext.FinishAffinity();
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}

        #endregion

        public override bool OnPreferenceTreeClick(Preference preference)
        {
            try
            {
                switch (preference.Key)
                {
                    case "about_me_key":
                    {
                        TypeDialog = "About";
                        var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialog.Title(GetString(Resource.String.Lbl_About));
                        dialog.Input(GetString(Resource.String.Lbl_About), preference.Summary, false, this);
                        dialog.InputType(InputTypes.TextFlagImeMultiLine);
                        dialog.PositiveText(GetText(Resource.String.Lbl_Save)).OnPositive(this);
                        dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                        break;
                    }
                    case "Call_key":
                    {
                        TypeDialog = "Call";
                        var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialog.Title(GetText(Resource.String.Lbl_Warning));
                        dialog.Content(GetText(Resource.String.Lbl_Clear_call_log));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                TabbedMainActivity.GetInstance().LastCallsTab?.MAdapter?.MCallUser?.Clear();
                                TabbedMainActivity.GetInstance().LastCallsTab?.MAdapter?.NotifyDataSetChanged();

                                TabbedMainActivity.GetInstance().LastCallsTab?.ShowEmptyPage();

                                ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Done),
                                    ToastLength.Long);

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.Clear_CallUser_List();

                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(new MyMaterialDialog());
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                        break;
                    }
                    case "Night_Mode_key":
                    {
                        TypeDialog = "NightMode";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        dialogList.Title(Resource.String.Lbl_Theme);

                        arrayAdapter.Add(GetText(Resource.String.Lbl_Light));
                        arrayAdapter.Add(GetText(Resource.String.Lbl_Dark));

                        if ((int) Build.VERSION.SdkInt >= 29)
                            arrayAdapter.Add(GetText(Resource.String.Lbl_SetByBattery));

                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                        break;
                    }
                    case "whocanfollow_key":
                    {
                        TypeDialog = "WhoCanFollow";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        dialogList.Title(Resource.String.Lbl_Who_can_follow_me);

                        arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                        arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1

                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                        break;
                    }
                    case "whocanseemybirthday_key":
                    {
                        TypeDialog = "Birthday";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        dialogList.Title(Resource.String.Lbl_Who_can_see_my_birthday);

                        arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                        arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1
                        arrayAdapter.Add(GetText(Resource.String.Lbl_No_body)); //>> value = 2

                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                        break;
                    }
                    case "whocanMessage_key":
                    {
                        TypeDialog = "Message";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        dialogList.Title(Resource.String.Lbl_Who_can_message_me);

                        arrayAdapter.Add(GetText(Resource.String.Lbl_Everyone)); //>> value = 0
                        arrayAdapter.Add(GetText(Resource.String.Lbl_People_i_Follow)); //>> value = 1
                        arrayAdapter.Add(GetText(Resource.String.Lbl_No_body)); //>> value = 2

                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                        break;
                    }
                    case "StorageConnectedMobile_key":
                    {
                        TypeDialog = "StorageConnectedMobile";

                        MainSettings.DataStorageConnected();
                        var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                             
                        var arrayIndexAdapter = new List<int>();
                        arrayIndexAdapter.AddRange(from selectClass in ListUtils.StorageTypeMobileSelect where selectClass.Value select selectClass.Id);

                        var arrayAdapter = ListUtils.StorageTypeMobileSelect.Select(selectClass => selectClass.Text).ToList();
                        dialogList.Title(GetText(Resource.String.Lbl_StorageConnectedMobile))
                            .Items(arrayAdapter)
                            .ItemsCallbackMultiChoice(arrayIndexAdapter.ToArray(), this)
                            .AlwaysCallMultiChoiceCallback()
                            .NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog())
                            .Build().Show();
                        break;
                    }
                    case "StorageConnectedWiFi_key":
                    {
                        TypeDialog = "StorageConnectedWiFi";

                        MainSettings.DataStorageConnected();

                        var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                        dialogList.Title(Resource.String.Lbl_StorageConnectedWiFi);
                             
                        var arrayIndexAdapter = new List<int>();
                        arrayIndexAdapter.AddRange(from selectClass in ListUtils.StorageTypeWiFiSelect where selectClass.Value select selectClass.Id);

                        var arrayAdapter = ListUtils.StorageTypeWiFiSelect.Select(selectClass => selectClass.Text).ToList();

                            dialogList.Title(GetText(Resource.String.Lbl_StorageConnectedWiFi))
                            .Items(arrayAdapter)
                            .ItemsCallbackMultiChoice(arrayIndexAdapter.ToArray(), this)
                            .AlwaysCallMultiChoiceCallback()
                            .NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog())
                            .Build().Show();
                        break;
                    }
                }

                return base.OnPreferenceTreeClick(preference);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return base.OnPreferenceTreeClick(preference);
            }
        }

        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                switch (TypeDialog)
                { 
                    case "Logout" when p1 == DialogAction.Positive:
                    {
                        ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long);
                        ApiRequest.Logout(ActivityContext);

                        break;
                    }
                    case "Logout":
                    {
                        if (p1 == DialogAction.Negative)
                        {
                            p0.Dismiss();
                        }

                        break;
                    }
                    default:
                    {
                        if (p1 == DialogAction.Positive)
                        {
                        }
                        else if (p1 == DialogAction.Negative)
                        {
                            p0.Dismiss();
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                
                switch (TypeDialog)
                {
                    case "NightMode":
                    {
                        string getValue = MainSettings.SharedData?.GetString("Night_Mode_key", string.Empty);

                        if (text == GetString(Resource.String.Lbl_Light) && getValue != MainSettings.LightMode)
                        {
                            //Set Light Mode   
                            NightMode.Summary = ActivityContext.GetString(Resource.String.Lbl_Light);

                            MainSettings.ApplyTheme(MainSettings.LightMode);
                            MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.LightMode)?.Commit();

                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                            {
                                ActivityContext.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                                ActivityContext.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                            }

                            Intent intent = new Intent(ActivityContext, typeof(TabbedMainActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            intent.AddFlags(ActivityFlags.NoAnimation);
                            ActivityContext.FinishAffinity();
                            ActivityContext.OverridePendingTransition(0, 0);
                            ActivityContext.StartActivity(intent);
                        }
                        else if (text == GetString(Resource.String.Lbl_Dark) && getValue != MainSettings.DarkMode)
                        {
                            NightMode.Summary = ActivityContext.GetString(Resource.String.Lbl_Dark);

                            MainSettings.ApplyTheme(MainSettings.DarkMode);
                            MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DarkMode)?.Commit();

                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                            {
                                ActivityContext.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                                ActivityContext.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                            }

                            Intent intent = new Intent(ActivityContext, typeof(TabbedMainActivity));
                            intent.AddCategory(Intent.CategoryHome);
                            intent.SetAction(Intent.ActionMain);
                            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                            intent.AddFlags(ActivityFlags.NoAnimation);
                            ActivityContext.FinishAffinity();
                            ActivityContext.OverridePendingTransition(0, 0);
                            ActivityContext.StartActivity(intent);
                                 
                        }
                        else if (text == GetString(Resource.String.Lbl_SetByBattery) && getValue != MainSettings.DefaultMode)
                        {
                            NightMode.Summary = ActivityContext.GetString(Resource.String.Lbl_SetByBattery);
                            MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DefaultMode)?.Commit();

                            if ((int)Build.VERSION.SdkInt >= 29)
                            {
                                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;

                                var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                                switch (currentNightMode)
                                {
                                    case UiMode.NightNo:
                                        // Night mode is not active, we're using the light theme
                                        MainSettings.ApplyTheme(MainSettings.LightMode);
                                        break;
                                    case UiMode.NightYes:
                                        // Night mode is active, we're using dark theme
                                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                                        break;
                                }
                            }
                            else
                            {
                                AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAutoBattery;

                                var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                                switch (currentNightMode)
                                {
                                    case UiMode.NightNo:
                                        // Night mode is not active, we're using the light theme
                                        MainSettings.ApplyTheme(MainSettings.LightMode);
                                        break;
                                    case UiMode.NightYes:
                                        // Night mode is active, we're using dark theme
                                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                                        break;
                                }

                                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                                {
                                    ActivityContext.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                                    ActivityContext.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                                }

                                Intent intent = new Intent(ActivityContext, typeof(TabbedMainActivity));
                                intent.AddCategory(Intent.CategoryHome);
                                intent.SetAction(Intent.ActionMain);
                                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                                intent.AddFlags(ActivityFlags.NoAnimation);
                                ActivityContext.FinishAffinity();
                                ActivityContext.OverridePendingTransition(0, 0);
                                ActivityContext.StartActivity(intent);
                            }
                        }

                        break;
                    }
                    case "WhoCanFollow":
                    {
                        if (text == GetString(Resource.String.Lbl_Everyone))
                        {
                            MainSettings.SharedData?.Edit()?.PutString("WhoCanFollow", "0")?.Commit();
                            PrivacyFollowPref.Summary = text;
                            SWhoCanFollowMe = "0";
                        }
                        else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                        {
                            MainSettings.SharedData?.Edit()?.PutString("WhoCanFollow", "1")?.Commit();
                            PrivacyFollowPref.Summary = text;
                            SWhoCanFollowMe = "1";
                        }

                        if (dataUser != null)
                        {
                            dataUser.FollowPrivacy = SWhoCanFollowMe;
                        }

                        if (Methods.CheckConnectivity())
                        {
                            var dataPrivacy = new Dictionary<string, string>
                            {
                                {"follow_privacy", SWhoCanFollowMe},
                            };
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                        }
                        else
                        {
                            ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                        }

                        break;
                    }
                    case "Birthday":
                    {
                        if (text == GetString(Resource.String.Lbl_Everyone))
                        {
                            MainSettings.SharedData?.Edit()?.PutString("whocanseemybirthday_key", "0")?.Commit();
                            PrivacyBirthdayPref.Summary = text;
                            SWhoCanSeeMyBirthday = "0";
                        }
                        else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                        {
                            MainSettings.SharedData?.Edit()?.PutString("whocanseemybirthday_key", "1")?.Commit();
                            PrivacyBirthdayPref.Summary = text;
                            SWhoCanSeeMyBirthday = "1";
                        }
                        else if (text == GetString(Resource.String.Lbl_No_body))
                        {
                            MainSettings.SharedData?.Edit()?.PutString("whocanseemybirthday_key", "2")?.Commit();
                            PrivacyBirthdayPref.Summary = text;
                            SWhoCanSeeMyBirthday = "2";
                        }

                        if (dataUser != null)
                        {
                            dataUser.BirthPrivacy = SWhoCanSeeMyBirthday;
                        }

                        if (Methods.CheckConnectivity())
                        {
                            var dataPrivacy = new Dictionary<string, string>
                            {
                                {"birth_privacy", SWhoCanSeeMyBirthday},
                            };

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                        }
                        else
                        {
                            ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                        }

                        break;
                    }
                    case "Message":
                    {
                        if (text == GetString(Resource.String.Lbl_Everyone))
                        {
                            MainSettings.SharedData?.Edit()?.PutString("whocanMessage_key", "0")?.Commit();
                            PrivacyMessagePref.Summary = text;
                            SWhoCanMessageMe = "0";
                        }
                        else if (text == GetString(Resource.String.Lbl_People_i_Follow))
                        {
                            MainSettings.SharedData?.Edit()?.PutString("whocanMessage_key", "1")?.Commit();
                            PrivacyMessagePref.Summary = text;
                            SWhoCanMessageMe = "1";
                        }
                        else if (text == GetString(Resource.String.Lbl_No_body))
                        {
                            MainSettings.SharedData?.Edit()?.PutString("whocanMessage_key", "2")?.Commit();
                            PrivacyMessagePref.Summary = text;
                            SWhoCanMessageMe = "2";
                        }

                        if (dataUser != null)
                        {
                            dataUser.MessagePrivacy = SWhoCanMessageMe;
                        }

                        if (Methods.CheckConnectivity())
                        {
                            var dataPrivacy = new Dictionary<string, string>
                            {
                                {"message_privacy", SWhoCanMessageMe},
                            };
                            
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                        }
                        else
                        {
                            ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnInput(MaterialDialog p0, string p1)
        {
            try
            {
                if (p1.Length <= 0) return;

                var strName = p1;
                if (!string.IsNullOrEmpty(strName)|| !string.IsNullOrWhiteSpace(strName))
                { 
                    if (TypeDialog == "About")
                    {
                        MainSettings.SharedData?.Edit()?.PutString("about_me_key", strName)?.Commit();
                        AboutMePref.Summary = strName;
                         
                        var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                        if (dataUser != null)
                        {
                            dataUser.About = strName;
                            SAbout = strName;

                            var sqLiteDatabase = new SqLiteDatabase();
                            sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                        }

                        if (Methods.CheckConnectivity())
                        {
                            var dataPrivacy = new Dictionary<string, string>
                            {
                                {"about", strName},
                            };

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy) });
                        }
                        else
                        {
                            ToastUtils.ShowToast(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public bool OnSelection(MaterialDialog dialog, int[] which, string[] text)
        {
            try
            {
                if (TypeDialog == "StorageConnectedMobile")
                {
                    UserDetails.PhotoMobile = false;
                    MainSettings.SharedData?.Edit()?.PutBoolean("photoMobile_key", false)?.Commit();

                    UserDetails.VideoMobile = false;
                    MainSettings.SharedData?.Edit()?.PutBoolean("videoMobile_key", false)?.Commit();

                    UserDetails.AudioMobile = false;
                    MainSettings.SharedData?.Edit()?.PutBoolean("audioMobile_key", false)?.Commit();

                    foreach (var t in which)
                    {
                        var typeId = ListUtils.StorageTypeMobileSelect[t].Id;
                        switch (typeId)
                        {
                            case 0:
                                UserDetails.PhotoMobile = true;
                                MainSettings.SharedData?.Edit()?.PutBoolean("photoMobile_key", true)?.Commit();
                                break;
                            case 1:
                                UserDetails.VideoMobile = true;
                                MainSettings.SharedData?.Edit()?.PutBoolean("videoMobile_key", true)?.Commit();
                                break;
                            case 2:
                                UserDetails.AudioMobile = true;
                                MainSettings.SharedData?.Edit()?.PutBoolean("audioMobile_key", true)?.Commit();
                                break;
                        }
                    }
                }
                else if (TypeDialog == "StorageConnectedWiFi")
                {
                    UserDetails.PhotoWifi = false;
                    MainSettings.SharedData?.Edit()?.PutBoolean("photoWifi_key", false)?.Commit();

                    UserDetails.VideoWifi = false;
                    MainSettings.SharedData?.Edit()?.PutBoolean("videoWifi_key", false)?.Commit();

                    UserDetails.AudioWifi = false;
                    MainSettings.SharedData?.Edit()?.PutBoolean("audioWifi_key", false)?.Commit();
                     
                    foreach (var t in which)
                    {
                        var typeId = ListUtils.StorageTypeWiFiSelect[t].Id;
                        switch (typeId)
                        {
                            case 0:
                                UserDetails.PhotoWifi = true;
                                MainSettings.SharedData?.Edit()?.PutBoolean("photoWifi_key", true)?.Commit();
                                break;
                            case 1:
                                UserDetails.VideoWifi = true;
                                MainSettings.SharedData?.Edit()?.PutBoolean("videoWifi_key", true)?.Commit();
                                break;
                            case 2:
                                UserDetails.AudioWifi = true;
                                MainSettings.SharedData?.Edit()?.PutBoolean("audioWifi_key", true)?.Commit();
                                break;
                        }

                    }
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            }
            return true;
        }

        #endregion
    }
}