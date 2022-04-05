//Use DoughouzChecker last version 3.7 to 
//build your own certifcate 
//For Full Documention 
//https://paper.dropbox.com/doc/WoWonder-Messenger-2.0--ARqO4OoLf_KXGWT63gNm0pOuAQ-M6qrJYGQ0C0NZlhZ3PqI7
//CopyRight DoughouzLight
//For the accuracy of the icon and logo, please use this website " https://appicon.co/ " and add images according to size in folders " mipmap " 
 
using WoWonder.Helpers.Model;
using WoWonderClient;

namespace WoWonder
{
    internal static class AppSettings
    {
        public static string TripleDesAppServiceProvider = "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";
         
        //Main Settings >>>>>
        //********************************************************* 
        public static string Version = "4.1";
        public static string ApplicationName = "meta crypto social messenger";
        public static string DatabaseName = "MetaCryptoMsg";

        // Friend system = 0 , follow system = 1
        public static int ConnectivitySystem = 1;

        public static SystemGetLastChat LastChatSystem = SystemGetLastChat.Default;
        public static InitializeWoWonder.ConnectionType ConnectionTypeChat = InitializeWoWonder.ConnectionType.Socket; 
        public static string PortSocketServer = "465"; 

        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#a52729";
        public static string StoryReadColor = "#808080";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar_AE

        //Set Language User on site from phone 
        public static bool SetLangUser = true;  

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "036ce4fb-011a-4ca2-91ac-8282ef729b59";

        //Error Report Mode
        //*********************************************************
        public static bool SetApisReportMode = false;

        //Code Time Zone (true => Get from Internet , false => Get From #CodeTimeZone )
        //*********************************************************
        public static bool AutoCodeTimeZone = true;
        public static string CodeTimeZone = "UTC";

        public static bool EnableRegisterSystem = true; 

        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;
         
        public static bool ShowSettingsUpdateManagerApp = false; 

        public static bool ShowSettingsRateApp = true;  
        public static int ShowRateAppCount = 5;

        //AdMob >> Please add the code ad in the Here and analytic.xml 
        //********************************************************* 
        public static ShowAds ShowAds = ShowAds.AllUsers;

        //Three times after entering the ad is displayed
        public static int ShowAdInterstitialCount = 3;
        public static int ShowAdRewardedVideoCount = 3;
        public static int ShowAdNativeCount = 40;
        public static int ShowAdAppOpenCount = 2;
         
        public static bool ShowAdMobBanner = false;
        public static bool ShowAdMobInterstitial = false;
        public static bool ShowAdMobRewardVideo = false;
        public static bool ShowAdMobNative = false;
        public static bool ShowAdMobAppOpen = false; 
        public static bool ShowAdMobRewardedInterstitial = false;

        public static string AdInterstitialKey = "ca-app-pub-5135691635931982/3442638218";
        public static string AdRewardVideoKey = "ca-app-pub-5135691635931982/3814173301";
        public static string AdAdMobNativeKey = "ca-app-pub-5135691635931982/9452678647";
        public static string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/3836425196";
        public static string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/7476900652";

        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowFbBannerAds = false;
        public static bool ShowFbInterstitialAds = false;
        public static bool ShowFbRewardVideoAds = false;
        public static bool ShowFbNativeAds = false;

        //YOUR_PLACEMENT_ID
        public static string AdsFbBannerKey = "250485588986218_554026418632132";
        public static string AdsFbInterstitialKey = "250485588986218_554026125298828";
        public static string AdsFbRewardVideoKey = "250485588986218_554072818627492";
        public static string AdsFbNativeKey = "250485588986218_554706301897477";

        //Colony Ads >> Please add the code ad in the Here 
        //*********************************************************  
        public static bool ShowColonyBannerAds = false;  
        public static bool ShowColonyInterstitialAds = false; 
        public static bool ShowColonyRewardAds = false;

        public static string AdsColonyAppId = "appff22269a7a0a4be8aa";
        public static string AdsColonyBannerId = "vz85ed7ae2d631414fbd";
        public static string AdsColonyInterstitialId = "vz39712462b8634df4a8";
        public static string AdsColonyRewardedId = "vz32ceec7a84aa4d719a";
        //********************************************************* 

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file or AndroidManifest.xml
        //Facebook >> ../values/analytic.xml .. 
        //Google >> ../Properties/AndroidManifest.xml .. line 37
        //*********************************************************
        public static bool EnableSmartLockForPasswords = false; //#New
         
        public static bool ShowFacebookLogin = false;
        public static bool ShowGoogleLogin = false;

        public static readonly string ClientId = "81603239249-i35mh67livs9gifrlv83e47dd3ohamsg.apps.googleusercontent.com";

        //Chat Window Activity >>
        //*********************************************************
        //if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        //Just replace it with this 5 lines of code
        /*
         <uses-permission android:name="android.permission.READ_CONTACTS" />
         <uses-permission android:name="android.permission.READ_PHONE_NUMBERS" /> 
         */
        public static bool ShowButtonContact = true;
        public static bool InvitationSystem = true;  //Invite friends section
        /////////////////////////////////////

        public static bool ShowButtonCamera = true;
        public static bool ShowButtonImage = true;
        public static bool ShowButtonVideo = true;
        public static bool ShowButtonAttachFile = true;
        public static bool ShowButtonColor = true;
        public static bool ShowButtonStickers = true;
        public static bool ShowButtonMusic = true;
        public static bool ShowButtonGif = true;
        public static bool ShowButtonLocation = true;
         
        public static bool OpenVideoFromApp = true;
        public static bool OpenImageFromApp = true;
        
        //Record Sound Style & Text 
        public static bool ShowButtonRecordSound = true;

        // Options List Message
        public static bool EnableReplyMessageSystem = true; 
        public static bool EnableForwardMessageSystem = true; 
        public static bool EnableFavoriteMessageSystem = true; 
        public static bool EnablePinMessageSystem = true; 
        public static bool EnableReactionMessageSystem = true; 

        public static bool ShowNotificationWithUpload = true;  

        public static bool AllowDownloadMedia = true; //#New
        public static bool EnableFitchOgLink = true; //#New

        /// <summary>
        /// https://dashboard.stipop.io/
        /// you can get api key from here https://prnt.sc/26ofmq9
        /// </summary>
        public static string StickersApikey = "0a441b19287cad752e87f6072bb914c0";//#New
         
        //List Chat >>
        //*********************************************************
        public static bool EnableChatPage = true;
        public static bool EnableChatGroup = true;
         
        // Options List Chat
        public static bool EnableChatArchive = true; 
        public static bool EnableChatPin = true;  
        public static bool EnableChatMute = true; 
        public static bool EnableChatMakeAsRead = true; 
         
        // User Profile >>
        //*********************************************************
        public static bool EnableShowPhoneNumber = true;

        // Story >>
        //*********************************************************
        //Set a story duration >> Sec
        public static long StoryImageDuration = 7;
        public static long StoryVideoDuration = 30; //#New

        /// <summary>
        /// If it is false, it will appear only for the specified time in the value of the StoryVideoDuration
        /// </summary>
        public static bool ShowFullVideo = false; //#New

        public static bool EnableStorySeenList = true; 
        public static bool EnableReplyStory  = true;

        /// <summary>
        /// you can edit video using FFMPEG 
        /// </summary>
        public static bool EnableVideoEditor = true; //#New
         
        //*********************************************************
        /// <summary>
        ///  Currency
        /// CurrencyStatic = true : get currency from app not api 
        /// CurrencyStatic = false : get currency from api (default)
        /// </summary>
        public static readonly bool CurrencyStatic = false;
        public static readonly string CurrencyIconStatic = "$";
        public static readonly string CurrencyCodeStatic = "USD";

        // Video/Audio Call Settings >>
        //*********************************************************
        public static bool EnableAudioVideoCall = true;

        public static bool EnableAudioCall = true;
        public static bool EnableVideoCall = true;

        public static SystemCall UseLibrary = SystemCall.Twilio;  

        // Walkthrough Settings >>
        //*********************************************************
        public static bool ShowWalkTroutPage = true;

        public static bool WalkThroughSetFlowAnimation = true;
        public static bool WalkThroughSetZoomAnimation = false;
        public static bool WalkThroughSetSlideOverAnimation = false;
        public static bool WalkThroughSetDepthAnimation = false;
        public static bool WalkThroughSetFadeAnimation = false;

        // Register Settings >>
        //*********************************************************
        public static bool ShowGenderOnRegister = true;
         
        //Last Messages Page >>
        //*********************************************************
        public static bool ShowOnlineOfflineMessage = true;

        public static int RefreshChatActivitiesSeconds = 3500; // 3 Seconds
        public static int MessageRequestSpeed = 4000; // 3 Seconds

        public static bool RenderPriorityFastPostLoad = true;
         
        public static ToastTheme ToastTheme = ToastTheme.Custom; 
        public static ColorMessageTheme ColorMessageTheme = ColorMessageTheme.Default; 
        public static PostButtonSystem ReactionTheme = PostButtonSystem.ReactionDefault;
          
        //Bypass Web Errors 
        //*********************************************************
        public static bool TurnTrustFailureOnWebException = true;
        public static bool TurnSecurityProtocolType3072On = true;

        public static bool EnableVideoCompress = false;  
        public static bool ShowTextWithSpace = true; 

        public static bool SetTabDarkTheme = false;

        public static bool ShowSuggestedUsersOnRegister = true;
        public static bool ImageCropping = true;

        //Settings Page >> General Account
        public static bool ShowSettingsAccount = true;
        public static bool ShowSettingsPassword = true;
        public static bool ShowSettingsBlockedUsers = true;
        public static bool ShowSettingsDeleteAccount = true;
        public static bool ShowSettingsTwoFactor = true;
        public static bool ShowSettingsManageSessions = true;
        public static bool ShowSettingsWallpaper  = true; 
        public static bool ShowSettingsFingerprintLock = true; //#New

        //Options chat heads (Bubbles) 
        //*********************************************************
        public static bool ShowChatHeads = true; //#New

        //Always , Hide , FullScreen
        public static string DisplayModeSettings = "Always";

        //Default , Left  , Right , Nearest , Fix , Thrown
        public static string MoveDirectionSettings = "Right";

        //Circle , Rectangle
        public static string ShapeSettings = "Circle";

        // Last position
        public static bool IsUseLastPosition = true;

        public static int AvatarPostSize = 60; 
        public static int ImagePostSize = 200; 
    }
}