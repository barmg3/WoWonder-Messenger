using System;
using Android.App;
using WoWonder.Helpers.Utils;
using WoWonder.SocketSystem;
using WoWonderClient;

namespace WoWonder.Helpers.Model
{
    internal static class UserDetails
    {
        //############# DONT'T MODIFY HERE #############
        //Auto Session bindable objects 
        //*********************************************************
        public static string AccessToken = string.Empty;
        public static string UserId = string.Empty;
        public static string Username = string.Empty;
        public static string FullName = string.Empty;
        public static string Password = string.Empty;
        public static string Email = string.Empty;
        public static string Cookie = string.Empty;
        public static string Status = string.Empty;
        public static string Avatar = string.Empty;
        public static string Cover = string.Empty;
        public static string DeviceId = string.Empty;
        public static string Lang = string.Empty;
        public static string IsPro = string.Empty;
        public static string Url = string.Empty;
        public static string Lat = string.Empty;
        public static string Lng = string.Empty;
        public static string LangName = string.Empty;
         
        public static bool IsLogin = false;
        public static int CountNotificationsStatic = 0;

        public static bool NotificationPopup { get; set; } = true;

        public static string NearByDistanceCount = "0";
        public static string NearByGender = "all";
        public static string NearByStatus = "2";
        public static string NearByRelationship = "5";

        public static string SearchGender = "all";
        public static string SearchCountry = "all";
        public static string SearchStatus = "all";
        public static string SearchVerified = "all";
        public static string SearchProfilePicture = "all";
        public static string SearchFilterByAge = "off";
        public static string SearchAgeFrom = "10";
        public static string SearchAgeTo = "70";

        public static bool SoundControl = true;
        public static bool OnlineUsers = false;
        public static bool ChatHead = true; 
        public static bool FingerprintLock = false; 

        public static bool PhotoWifi = true; 
        public static bool VideoWifi = true; 
        public static bool AudioWifi = true; 

        public static bool PhotoMobile = true; 
        public static bool VideoMobile = true; 
        public static bool AudioMobile = true; 
         
        public static string AndroidId = Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
        public static WoSocketHandler Socket { get; set; }
        public static bool OpenDialog;

        public static void ClearAllValueUserDetails()
        {
            try
            {
                AccessToken = string.Empty;
                UserId = string.Empty;
                Username = string.Empty;
                FullName = string.Empty;
                Password = string.Empty;
                Email = string.Empty;
                Cookie = string.Empty;
                Status = string.Empty;
                Avatar = string.Empty;
                Cover = string.Empty;
                DeviceId = string.Empty; 
                LangName = string.Empty;
                Lat = string.Empty;
                Lng = string.Empty;

                Current.AccessToken = string.Empty;
                Socket = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}