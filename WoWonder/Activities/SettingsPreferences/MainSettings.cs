using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.Preference;
using WoWonder.Activities.Story.Service;
using WoWonder.Frameworks.Floating;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.SettingsPreferences
{
    public static class MainSettings
    {
        public static ISharedPreferences SharedData, LastPosition, InAppReview;
        public static readonly string LightMode = "light";
        public static readonly string DarkMode = "dark";
        public static readonly string DefaultMode = "default";

        public static readonly string PrefKeyLastPositionX = "last_position_x";
        public static readonly string PrefKeyLastPositionY = "last_position_y";
        public static readonly string PrefKeyInAppReview = "In_App_Review"; 

        public static void Init()
        {
            try
            {
                StoryService.ActionStory = Application.Context.PackageName + ".action.ACTION_STORY";
                 
                SharedData = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
                LastPosition = Application.Context.GetSharedPreferences("last_position", FileCreationMode.Private);
                InAppReview = Application.Context.GetSharedPreferences("In_App_Review", FileCreationMode.Private);

                string getValue = SharedData.GetString("Night_Mode_key", string.Empty);
                ApplyTheme(getValue);

                var cdv = InitFloating.CanDrawOverlays(Application.Context);
                if (cdv)
                    UserDetails.ChatHead = SharedData.GetBoolean("chatheads_key", cdv);
                   
                UserDetails.SoundControl = SharedData.GetBoolean("checkBox_PlaySound_key", true);
                UserDetails.NotificationPopup = SharedData.GetBoolean("notifications_key", true);
                UserDetails.OnlineUsers = SharedData.GetBoolean("onlineUser_key", true);

                if (AppSettings.ShowSettingsFingerprintLock)
                    UserDetails.FingerprintLock = SharedData.GetBoolean("FingerprintLock_key", false);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static void ApplyTheme(string themePref)
        {
            try
            {
                if (themePref == LightMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    AppSettings.SetTabDarkTheme = false;
                }
                else if (themePref == DarkMode)
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    AppSettings.SetTabDarkTheme = true;
                }
                else if (themePref == DefaultMode)
                {
                    AppCompatDelegate.DefaultNightMode = (int)Build.VERSION.SdkInt >= 29 ? AppCompatDelegate.ModeNightFollowSystem : AppCompatDelegate.ModeNightAutoBattery;

                    var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;
                    switch (currentNightMode)
                    {
                        case UiMode.NightNo:
                            // Night mode is not active, we're using the light theme
                            AppSettings.SetTabDarkTheme = false;
                            break;
                        case UiMode.NightYes:
                            // Night mode is active, we're using dark theme
                            AppSettings.SetTabDarkTheme = true;
                            break;
                    }
                }
                else
                {
                    if (AppSettings.SetTabDarkTheme) 
                        return;

                    //AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAutoBattery;
                    var currentNightMode = Application.Context.Resources?.Configuration?.UiMode & UiMode.NightMask;
                    switch (currentNightMode)
                    {
                        case UiMode.NightNo:
                            // Night mode is not active, we're using the light theme
                            AppSettings.SetTabDarkTheme = false;
                            break;
                        case UiMode.NightYes:
                            // Night mode is active, we're using dark theme
                            AppSettings.SetTabDarkTheme = true;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public static void DataStorageConnected()
        {
            try
            {
                UserDetails.PhotoWifi = SharedData?.GetBoolean("photoWifi_key", true) ?? true;
                UserDetails.VideoWifi = SharedData?.GetBoolean("videoWifi_key", true) ?? true;
                UserDetails.AudioWifi = SharedData?.GetBoolean("audioWifi_key", true) ?? true;

                UserDetails.PhotoMobile = SharedData?.GetBoolean("photoMobile_key", true) ?? true;
                UserDetails.VideoMobile = SharedData?.GetBoolean("videoMobile_key", true) ?? true;
                UserDetails.AudioMobile = SharedData?.GetBoolean("audioMobile_key", true) ?? true;

                ListUtils.StorageTypeMobileSelect = new List<Classes.StorageTypeSelectClass>
                {
                    new Classes.StorageTypeSelectClass()
                    {
                        Id = 0,
                        Type = "photoMobile",
                        Value = UserDetails.PhotoMobile,
                        Text = Application.Context.GetText(Resource.String.image)
                    },
                    new Classes.StorageTypeSelectClass()
                    {
                        Id = 1,
                        Type = "videoMobile",
                        Value = UserDetails.VideoMobile,
                        Text = Application.Context.GetText(Resource.String.video)
                    },
                    //new Classes.StorageTypeSelectClass()
                    //{
                    //    Id = 2,
                    //    Type = "audioMobile",
                    //    Value = UserDetails.AudioMobile,
                    //    Text = Application.Context.GetText(Resource.String.audio)
                    //}
                };
                  
                ListUtils.StorageTypeWiFiSelect = new List<Classes.StorageTypeSelectClass>
                {
                    new Classes.StorageTypeSelectClass()
                    {
                        Id = 0,
                        Type = "photoWifi",
                        Value = UserDetails.PhotoWifi,
                        Text = Application.Context.GetText(Resource.String.image)
                    },
                    new Classes.StorageTypeSelectClass()
                    {
                        Id = 1,
                        Type = "videoWifi",
                        Value = UserDetails.VideoWifi,
                        Text = Application.Context.GetText(Resource.String.video)
                    },
                    //new Classes.StorageTypeSelectClass()
                    //{
                    //    Id = 2,
                    //    Type = "audioWifi",
                    //    Value = UserDetails.AudioWifi,
                    //    Text = Application.Context.GetText(Resource.String.audio)
                    //}
                };
                 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}
