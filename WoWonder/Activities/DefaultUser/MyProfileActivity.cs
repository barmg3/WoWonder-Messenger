using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Core.Content;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using TheArtOfDev.Edmodo.Cropper;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.Share;
using WoWonder.Library.Anjo.Share.Abstractions;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.DefaultUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MyProfileActivity : BaseActivity, MaterialDialog.IListCallback
    {  
        #region Variables Basic

        private TextView TxtUserName, TxtFullname, TxtFollowing, TxtFollowingCount, TxtFollowers, TxtFollowersCount;
        private EditText TxtFirstName, TxtLastName, TxtGenderText, TxtLocationText, TxtMobileText, TxtWebsiteText, TxtWorkText; 
        private TextView TxtNameIcon, TxtGenderIcon, TxtLocationIcon, TxtMobileIcon, TxtWebsiteIcon, TxtWorkIcon; 
        private ImageView UserProfileImage, CoverImage; 
        private CircleButton EditProfileButton;
        private TextView TxtFacebookIcon, TxtTwitterIcon,TxtVkIcon, TxtInstagramIcon, TxtYoutubeIcon;
        private EditText TxtFacebookText,TxtTwitterText,TxtVkText, TxtInstagramText, TxtYoutubeText;
        private string ImageType = "", GenderStatus = "";
        private PublisherAdView PublisherAdView;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.MyProfileLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);

                GetMyInfoData();

                AdsGoogle.Ad_Interstitial(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                PublisherAdView?.Resume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                PublisherAdView?.Pause();
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        protected override void OnDestroy()
        {
            try
            {
                PublisherAdView?.Destroy();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region Menu

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Profile_Menu, menu);

            var item = menu.FindItem(Resource.Id.menue_block); 
            item.SetVisible(false);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.menue_SaveData:
                    EditProfileButtonOnClick();
                    break;

                case Resource.Id.menue_block:
                    
                    break;

                case Resource.Id.menue_Copy:
                    OnCopeLinkToProfile_Button_Click();
                    break;

                case Resource.Id.menue_Share:
                    OnShare_Button_Click();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
          

        //Event Menu >> Cope Link To Profile
        private void OnCopeLinkToProfile_Button_Click()
        {
            try
            {
                var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

                var clipData = ClipData.NewPlainText("text", ListUtils.MyProfileList.FirstOrDefault()?.Url);
                clipboardManager.PrimaryClip = clipData;


                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Copied),ToastLength.Short);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void OnShare_Button_Click()
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

               var data = ListUtils.MyProfileList.FirstOrDefault();
                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = WoWonderTools.GetNameFinal(data),
                    Text = "",
                    Url = data?.Url
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtNameIcon = FindViewById<TextView>(Resource.Id.name_icon);
                TxtFullname = FindViewById<TextView>(Resource.Id.Txt_fullname);
                TxtUserName = FindViewById<TextView>(Resource.Id.username);

                TxtFollowers = FindViewById<TextView>(Resource.Id.Txt_flowersView);
                TxtFollowersCount = FindViewById<TextView>(Resource.Id.Txt_flowers_count);

                TxtFollowing = FindViewById<TextView>(Resource.Id.flowinglabelView);
                TxtFollowingCount = FindViewById<TextView>(Resource.Id.Txt_flowing_countView);

                TxtFirstName = FindViewById<EditText>(Resource.Id.FirstName_text);
                TxtLastName = FindViewById<EditText>(Resource.Id.LastName_text);

                UserProfileImage = FindViewById<ImageView>(Resource.Id.profile_image);
                CoverImage = FindViewById<ImageView>(Resource.Id.coverImageView);

                TxtGenderIcon = FindViewById<TextView>(Resource.Id.gender_icon);
                TxtGenderText = FindViewById<EditText>(Resource.Id.gender_text);

                TxtLocationIcon = FindViewById<TextView>(Resource.Id.location_icon);
                TxtLocationText = FindViewById<EditText>(Resource.Id.location_text);

                TxtMobileIcon = FindViewById<TextView>(Resource.Id.mobile_icon);
                TxtMobileText = FindViewById<EditText>(Resource.Id.mobile_text);

                TxtWebsiteIcon = FindViewById<TextView>(Resource.Id.website_icon);
                TxtWebsiteText = FindViewById<EditText>(Resource.Id.website_text);

                TxtWorkIcon = FindViewById<TextView>(Resource.Id.work_icon);
                TxtWorkText = FindViewById<EditText>(Resource.Id.work_text);

                TxtFacebookIcon = FindViewById<TextView>(Resource.Id.facebook_icon);
                TxtFacebookText = FindViewById<EditText>(Resource.Id.facebook_text);
                TxtTwitterIcon = FindViewById<TextView>(Resource.Id.Twitter_icon);
                TxtTwitterText = FindViewById<EditText>(Resource.Id.Twitter_text);
                TxtVkIcon = FindViewById<TextView>(Resource.Id.VK_icon);
                TxtVkText = FindViewById<EditText>(Resource.Id.VK_text);
                TxtInstagramIcon = FindViewById<TextView>(Resource.Id.Instagram_icon);
                TxtInstagramText = FindViewById<EditText>(Resource.Id.Instagram_text);
                TxtYoutubeIcon = FindViewById<TextView>(Resource.Id.Youtube_icon);
                TxtYoutubeText = FindViewById<EditText>(Resource.Id.Youtube_text);
                  
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtGenderIcon, IonIconsFonts.Male);
                TxtGenderIcon.SetTextColor(Color.ParseColor("#4693d8"));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtLocationIcon, IonIconsFonts.Pin);
                TxtLocationIcon.SetTextColor(Color.ParseColor(AppSettings.MainColor));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtMobileIcon, IonIconsFonts.Call);
                TxtMobileIcon.SetTextColor(Color.ParseColor("#fa6670"));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtWebsiteIcon, IonIconsFonts.Globe);
                TxtWebsiteIcon.SetTextColor(Color.ParseColor("#6b38d1"));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtWorkIcon, IonIconsFonts.Briefcase);
                TxtWorkIcon.SetTextColor(Color.ParseColor("#eca72c"));

                EditProfileButton = FindViewById<CircleButton>(Resource.Id.Edit_button);
                //EditProfile_button.Click += EditProfileButtonOnClick;
                EditProfileButton.Visibility = ViewStates.Invisible;
                EditProfileButton.SetColor(Color.ParseColor("#282828"));

                if (AppSettings.SetTabDarkTheme)
                {
                    TxtFirstName.SetTextColor(Color.White);
                    TxtFirstName.SetHintTextColor(Color.White);

                    TxtLastName.SetTextColor(Color.White);
                    TxtLastName.SetHintTextColor(Color.White);

                    TxtGenderText.SetTextColor(Color.White);
                    TxtGenderText.SetHintTextColor(Color.White);

                    TxtLocationText.SetTextColor(Color.White);
                    TxtLocationText.SetHintTextColor(Color.White);

                    TxtMobileText.SetTextColor(Color.White);
                    TxtMobileText.SetHintTextColor(Color.White);

                    TxtWebsiteText.SetTextColor(Color.White);
                    TxtWebsiteText.SetHintTextColor(Color.White);
                     
                    TxtWorkText.SetTextColor(Color.White);
                    TxtWorkText.SetHintTextColor(Color.White);

                    TxtFacebookText.SetTextColor(Color.White);
                    TxtFacebookText.SetHintTextColor(Color.White);

                    TxtTwitterText.SetTextColor(Color.White);
                    TxtTwitterText.SetHintTextColor(Color.White);

                    TxtVkText.SetTextColor(Color.White);
                    TxtVkText.SetHintTextColor(Color.White);

                    TxtInstagramText.SetTextColor(Color.White);
                    TxtInstagramText.SetHintTextColor(Color.White);

                    TxtYoutubeText.SetTextColor(Color.White);
                    TxtYoutubeText.SetHintTextColor(Color.White);
                }
                else
                {
                    TxtFirstName.SetTextColor(Color.Black);
                    TxtFirstName.SetHintTextColor(Color.Black);

                    TxtLastName.SetTextColor(Color.Black);
                    TxtLastName.SetHintTextColor(Color.Black);

                    TxtGenderText.SetTextColor(Color.Black);
                    TxtGenderText.SetHintTextColor(Color.Black);

                    TxtLocationText.SetTextColor(Color.Black);
                    TxtLocationText.SetHintTextColor(Color.Black);

                    TxtMobileText.SetTextColor(Color.Black);
                    TxtMobileText.SetHintTextColor(Color.Black);

                    TxtWebsiteText.SetTextColor(Color.Black);
                    TxtWebsiteText.SetHintTextColor(Color.Black);

                    TxtWorkText.SetTextColor(Color.Black);
                    TxtWorkText.SetHintTextColor(Color.Black);

                    TxtFacebookText.SetTextColor(Color.Black);
                    TxtFacebookText.SetHintTextColor(Color.Black);

                    TxtTwitterText.SetTextColor(Color.Black);
                    TxtTwitterText.SetHintTextColor(Color.Black);

                    TxtVkText.SetTextColor(Color.Black);
                    TxtVkText.SetHintTextColor(Color.Black);

                    TxtInstagramText.SetTextColor(Color.Black);
                    TxtInstagramText.SetHintTextColor(Color.Black);

                    TxtYoutubeText.SetTextColor(Color.Black);
                    TxtYoutubeText.SetHintTextColor(Color.Black);
                }


                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtNameIcon, IonIconsFonts.Person);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtFacebookIcon, IonIconsFonts.LogoFacebook);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtTwitterIcon, IonIconsFonts.LogoTwitter);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, TxtVkIcon, FontAwesomeIcon.Vk);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtInstagramIcon, IonIconsFonts.LogoInstagram);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtYoutubeIcon, IonIconsFonts.LogoYoutube);

                Methods.SetFocusable(TxtGenderText);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = " ";
                    toolbar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    if (AppSettings.FlowDirectionRightToLeft)
                        toolbar.LayoutDirection = LayoutDirection.Rtl;
                }
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
                // true +=  // false -=
                if (addEvent)
                {
                    UserProfileImage.Click += UserProfileImageOnClick;
                    CoverImage.Click += ImageUserCoverOnClick;
                    TxtGenderText.Touch += TxtGenderTextOnClick;
                   
                }
                else
                {
                    UserProfileImage.Click -= UserProfileImageOnClick;
                    CoverImage.Click -= ImageUserCoverOnClick;
                    TxtGenderText.Touch -= TxtGenderTextOnClick; 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Events

        private void ImageUserCoverOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                OpenDialogGallery("Cover");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UserProfileImageOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                OpenDialogGallery("Avatar");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtGenderTextOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action != MotionEventActions.Down) return;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (ListUtils.SettingsSiteList?.Genders?.Count > 0)
                {
                    arrayAdapter.AddRange(from item in ListUtils.SettingsSiteList?.Genders select item.Value);
                }
                else
                {
                    arrayAdapter.Add(GetText(Resource.String.Radio_Male));
                    arrayAdapter.Add(GetText(Resource.String.Radio_Female));
                }

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

       
        #endregion

        #region Update Image Avatar && Cover

        private void OpenDialogGallery(string typeImage)
        {
            try
            {
                ImageType = typeImage;
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && PermissionsController.CheckPermissionStorage() && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void Update_Image_Api(string type, string path)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    switch (type)
                    {
                        case "Avatar":
                        {
                            var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserAvatarAsync(path);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    Console.WriteLine(result.Message);
                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully),ToastLength.Short);

                                    //Set image 
                                    File file2 = new File(path);
                                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                    Glide.With(this).Load(photoUri).Apply(new RequestOptions().CircleCrop()).Into(UserProfileImage);

                                    //GlideImageLoader.LoadImage(this, path, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                 
                                    var local = ListUtils.MyProfileList.FirstOrDefault();
                                    if (local != null)
                                    {
                                        local.Avatar = path;

                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Insert_Or_Update_To_MyProfileTable(local);
                                    
                                    }
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                        case "Cover":
                        {
                            var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserCoverAsync(path);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    Console.WriteLine(result.Message);
                                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Image_changed_successfully),ToastLength.Short);

                                    //Set image 
                                    //var file = Uri.FromFile(new File(path));
                                    File file2 = new File(path);
                                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                    Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(CoverImage);

                                    //GlideImageLoader.LoadImage(this, path, CoverImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                                    var local = ListUtils.MyProfileList.FirstOrDefault();
                                    if (local != null)
                                    {
                                        local.Cover = path;

                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                        dbDatabase.Insert_Or_Update_To_MyProfileTable(local);
                                    
                                    }

                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

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

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                //If its from Camera or Gallery
                if (requestCode == CropImage.CropImageActivityRequestCode && resultCode == Result.Ok)
                {
                    var result = CropImage.GetActivityResult(data);

                    if (result.IsSuccessful)
                    {
                        var resultUri = result.Uri;

                        if (!string.IsNullOrEmpty(resultUri.Path))
                        {
                            string pathimg;
                            switch (ImageType)
                            {
                                case "Cover":
                                    pathimg = resultUri.Path;
                                    Update_Image_Api(ImageType, pathimg);
                                    break;
                                case "Avatar":
                                    pathimg = resultUri.Path;
                                    Update_Image_Api(ImageType, pathimg);
                                    break;
                            }
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery(ImageType);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion
        
        #region Get Data User

        //Get Data User From Database 
        private void GetMyInfoData()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                LoadDataUser(dataUser);

                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Data My Profile API
        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetProfileApi });
        }

        private async Task GetProfileApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Global.GetUserDataAsync(UserDetails.UserId, "user_data,followers,following");

            if (apiStatus != 200 || respond is not GetUserDataObject result || result.UserData == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                RunOnUiThread(() => LoadDataUser(result.UserData));
            }
        }
         

        private void LoadDataUser(UserDataObject data)
        {
            try
            {
                //Cover
                GlideImageLoader.LoadImage(this, data.Cover, CoverImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                //profile_picture
                GlideImageLoader.LoadImage(this, data.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                UserDetails.FullName = data.Name;
                 
                TxtFullname.Text = WoWonderTools.GetNameFinal(data);
                TxtUserName.Text = "@" + data.Username;
                TxtFirstName.Text = Methods.FunString.DecodeString(data.FirstName);
                TxtLastName.Text = Methods.FunString.DecodeString(data.LastName);

                if (data.Details.DetailsClass != null)
                {
                    var following = Methods.FunString.FormatPriceValue(Convert.ToInt32(data.Details.DetailsClass?.FollowingCount));
                    var followers = Methods.FunString.FormatPriceValue(Convert.ToInt32(data.Details.DetailsClass?.FollowersCount));

                    if (AppSettings.ConnectivitySystem == 1)
                    {
                        TxtFollowing.Visibility = ViewStates.Visible;
                        TxtFollowingCount.Visibility = ViewStates.Visible;

                        TxtFollowers.Visibility = ViewStates.Visible;
                        TxtFollowersCount.Visibility = ViewStates.Visible;

                        TxtFollowing.Text = GetText(Resource.String.Lbl_Following);
                        TxtFollowingCount.Text = following;

                        TxtFollowers.Text = GetText(Resource.String.Lbl_Followers);
                        TxtFollowersCount.Text = followers;
                    }
                    else
                    {
                        TxtFollowing.Visibility = ViewStates.Visible;
                        TxtFollowingCount.Visibility = ViewStates.Visible;

                        TxtFollowers.Visibility = ViewStates.Gone;
                        TxtFollowersCount.Visibility = ViewStates.Gone;

                        TxtFollowing.Text = GetText(Resource.String.Lbl_Friends);
                        TxtFollowingCount.Text = following;
                    }
                }

                switch (data.Gender.ToLower())
                {
                    case "male":
                        TxtGenderText.Text = GetText(Resource.String.Radio_Male);
                        break;
                    case "female":
                        TxtGenderText.Text = GetText(Resource.String.Radio_Female);
                        break;
                    default:
                        TxtGenderText.Text = data.Gender;
                        break;
                }

                TxtLocationText.Text = data.Address;
                TxtMobileText.Text = data.PhoneNumber;
                TxtWebsiteText.Text = data.Website;
                TxtWorkText.Text = data.Working;

                TxtFacebookText.Text = data.Facebook;
                TxtTwitterText.Text = data.Twitter;
                TxtVkText.Text = data.Vk;
                TxtInstagramText.Text = data.Instagram;
                TxtYoutubeText.Text = data.Youtube;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region MaterialDialog
         
        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (ListUtils.SettingsSiteList?.Genders?.Count > 0)
                {
                    TxtGenderText.Text = itemString;

                    var key = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Value == itemString).Key;
                    GenderStatus = key ?? "male";
                }
                else
                {
                    if (itemString == GetText(Resource.String.Radio_Male))
                    {
                        TxtGenderText.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                    }
                    else if (itemString == GetText(Resource.String.Radio_Female))
                    {
                        TxtGenderText.Text = GetText(Resource.String.Radio_Female);
                        GenderStatus = "female";
                    }
                    else
                    {
                        TxtGenderText.Text = GetText(Resource.String.Radio_Male);
                        GenderStatus = "male";
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private async void EditProfileButtonOnClick()
        {
            try
            {  
                if (Methods.CheckConnectivity())
                {             
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                     
                    var dictionary = new Dictionary<string, string>();

                    if (!string.IsNullOrEmpty(TxtFirstName.Text))
                        dictionary.Add("first_name", TxtFirstName.Text);

                    if (!string.IsNullOrEmpty(TxtLastName.Text))
                        dictionary.Add("last_name", TxtLastName.Text);

                    if (!string.IsNullOrEmpty(TxtFacebookText.Text))
                        dictionary.Add("facebook", TxtFacebookText.Text);

                    if (!string.IsNullOrEmpty(TxtTwitterText.Text))
                        dictionary.Add("twitter", TxtTwitterText.Text);

                    if (!string.IsNullOrEmpty(TxtYoutubeText.Text))
                        dictionary.Add("youtube", TxtYoutubeText.Text);

                    if (!string.IsNullOrEmpty(TxtInstagramText.Text))
                        dictionary.Add("instagram", TxtInstagramText.Text);

                    if (!string.IsNullOrEmpty(TxtVkText.Text))
                        dictionary.Add("vk", TxtVkText.Text);

                    if (!string.IsNullOrEmpty(TxtWebsiteText.Text))
                        dictionary.Add("website", TxtWebsiteText.Text);

                    if (!string.IsNullOrEmpty(TxtLocationText.Text))
                        dictionary.Add("address", TxtLocationText.Text);

                    if (!string.IsNullOrEmpty(TxtGenderText.Text))
                        dictionary.Add("gender", GenderStatus);

                    if (!string.IsNullOrEmpty(TxtMobileText.Text))
                        dictionary.Add("phone_number", TxtMobileText.Text);
                    
                    var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserDataAsync(dictionary);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            if (result.Message.Contains("updated"))
                            {
                                AndHUD.Shared.Dismiss(this);
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourDetailsWasUpdated), ToastLength.Short);


                                if (dataUser != null)
                                {
                                    dataUser.FirstName = TxtFirstName.Text;
                                    dataUser.LastName = TxtLastName.Text;
                                    dataUser.Address = TxtLocationText.Text;
                                    dataUser.Working = TxtWorkText.Text;
                                    dataUser.GenderText = TxtGenderText.Text;
                                    dataUser.Gender = GenderStatus;
                                    dataUser.Website = TxtWebsiteText.Text;
                                    dataUser.Facebook = TxtFacebookText.Text;
                                    dataUser.Twitter = TxtTwitterText.Text;
                                    dataUser.Youtube = TxtYoutubeText.Text;
                                    dataUser.Vk = TxtVkText.Text;
                                    dataUser.Instagram = TxtInstagramText.Text;
                                    dataUser.PhoneNumber = TxtMobileText.Text;

                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    dbDatabase.Insert_Or_Update_To_MyProfileTable(dataUser);
                                }
                            }
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond); 
                    }
                }
                else
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
                } 
            }
            catch (Exception e)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}