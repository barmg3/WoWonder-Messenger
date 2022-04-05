using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AppIntro;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using WoWonder.Activities.StickersView;
using WoWonder.Activities.SuggestedUsers;
using WoWonder.Activities.Tab;
using WoWonder.Library.OneSignalNotif;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Requests;

namespace WoWonder.Activities.WalkTroutPage
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/Theme.MaterialComponents.Light.NoActionBar", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AppIntroWalkTroutPage : AppIntro2
    {
        private int Count = 1;
        private string Caller = "";
        private RequestBuilder FullGlideRequestBuilder;

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                AddSlide(new AnimFragment1());
                AddSlide(new AnimFragment2());
                AddSlide(new AnimFragment4());
                AddSlide(new AnimFragment3());

                if (AppSettings.WalkThroughSetFlowAnimation)
                    SetFlowAnimation();
                else if (AppSettings.WalkThroughSetZoomAnimation)
                    SetZoomAnimation();
                else if (AppSettings.WalkThroughSetSlideOverAnimation)
                    SetSlideOverAnimation();
                else if (AppSettings.WalkThroughSetDepthAnimation)
                    SetDepthAnimation();
                else if (AppSettings.WalkThroughSetFadeAnimation) SetFadeAnimation();

                ShowStatusBar(false);

                //SetNavBarColor(Color.ParseColor(AppSettings.MainColor));
                SetIndicatorColor(Color.ParseColor(AppSettings.MainColor), Color.ParseColor("#888888"));
                //SetBarColor(Color.ParseColor("#3F51B5"));
                // SetSeparatorColor(Color.ParseColor("#2196f3"));

                Caller = Intent?.GetStringExtra("class") ?? "";

                if ((int)Build.VERSION.SdkInt > 23)
                {
                    new PermissionsController(this).RequestPermission(105);
                }

                //OneSignal Notification  
                //====================================== 
                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.Instance.RegisterNotificationDevice(this);

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.Get_MyProfileData_Api});

                FullGlideRequestBuilder = Glide.With(this).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).SkipMemoryCache(true).Override(200);

                List<string> stickerList = new List<string>();
                stickerList.AddRange(StickersModel.Locally.StickerList1);
                stickerList.AddRange(StickersModel.Locally.StickerList2);
                stickerList.AddRange(StickersModel.Locally.StickerList3);
                stickerList.AddRange(StickersModel.Locally.StickerList4);
                stickerList.AddRange(StickersModel.Locally.StickerList5);
                stickerList.AddRange(StickersModel.Locally.StickerList6);
                stickerList.AddRange(StickersModel.Locally.StickerList7);
                stickerList.AddRange(StickersModel.Locally.StickerList8);
                stickerList.AddRange(StickersModel.Locally.StickerList9);
                stickerList.AddRange(StickersModel.Locally.StickerList10);
                stickerList.AddRange(StickersModel.Locally.StickerList11);

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        foreach (var item in stickerList)
                        {
                            FullGlideRequestBuilder.Load(item).Preload();
                        }
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                }); 
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
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Permissions 

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                switch (requestCode)
                {
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                if (Methods.CheckConnectivity())
                                {
                                    Dictionary<string, string> dictionaryProfile = new Dictionary<string, string>();

                                    var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dictionaryProfile = new Dictionary<string, string>();

                                        dataUser.Lat = UserDetails.Lat;
                                        dataUser.Lat = UserDetails.Lat;

                                        var sqLiteDatabase = new SqLiteDatabase();
                                        sqLiteDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                    }

                                    dictionaryProfile.Add("lat", UserDetails.Lat);
                                    dictionaryProfile.Add("lng", UserDetails.Lng);

                                    if (Methods.CheckConnectivity())
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataAsync(dictionaryProfile) });
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        }).ConfigureAwait(false);
                        break;
                    case 105:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted && PermissionsController.CheckPermissionStorage():
                        Methods.Path.Chack_MyFolder();
                        break;
                    case 108:
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                        break;

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        public override void OnSlideChanged()
        {
            try
            {
                base.OnSlideChanged();
                Pressed(); 
                Count++;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void Pressed()
        {
            try
            {
                if (Count == 0) //Location
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(105);
                }
                else if (Count == 1) //Contacts
                {
                    if ((int)Build.VERSION.SdkInt > 23 && AppSettings.InvitationSystem)
                        new PermissionsController(this).RequestPermission(101);
                }
                else if (Count == 2) // Record
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(102);
                }
                else if (Count == 3) // Storage & Camera
                {
                    if ((int)Build.VERSION.SdkInt > 23)
                        new PermissionsController(this).RequestPermission(108);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Do something when users tap on Done button.
        public override void OnDonePressed()
        {
            try
            {
                if (Caller.Contains("register"))
                {
                    if (AppSettings.ShowSuggestedUsersOnRegister)
                    {
                        Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                        newIntent.PutExtra("class", "register");
                        StartActivity(newIntent);
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                    }
                }
                else
                {
                    StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                }

                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Do something when users tap on Skip button.
        public override void OnSkipPressed()
        {
            try
            {
                if (Caller.Contains("register"))
                {
                    if (AppSettings.ShowSuggestedUsersOnRegister)
                    {
                        Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                        newIntent.PutExtra("class", "register");
                        StartActivity(newIntent);
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                    }
                }
                else
                {
                    StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                }

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