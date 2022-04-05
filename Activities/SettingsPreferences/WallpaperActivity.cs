using System;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using JaredRummler.Android.ColorPicker;
using Java.IO;
using Java.Lang;
using TheArtOfDev.Edmodo.Cropper;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using CardView = AndroidX.CardView.Widget.CardView;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.SettingsPreferences
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class WallpaperActivity : BaseActivity, IColorPickerDialogListener
    {
        #region Variables Basic

        private ImageView ImageView;
        private CardView CardViewGallery, CardViewColor;
        private TextView TxtSave, IconGallery, IconColor, IconDefaultWallpaper;
        private LinearLayout LayoutDefaultWallpaper;
        private PublisherAdView PublisherAdView;
        private string EventSave = "", HexColor = "";
        private Uri ResultUri;

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
                SetContentView(Resource.Layout.WallpaperLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                GetWallpaper();

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
                PublisherAdView?.Resume();
                base.OnResume();
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

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
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

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
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

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtSave = FindViewById<TextView>(Resource.Id.toolbar_title);

                ImageView = FindViewById<ImageView>(Resource.Id.image);

                CardViewGallery = FindViewById<CardView>(Resource.Id.cardViewGallery);
                IconGallery = FindViewById<TextView>(Resource.Id.iconGallery);

                CardViewColor = FindViewById<CardView>(Resource.Id.cardViewColor);
                IconColor = FindViewById<TextView>(Resource.Id.iconColor);

                IconDefaultWallpaper = FindViewById<TextView>(Resource.Id.IconDefaultWallpaper);

                LayoutDefaultWallpaper = FindViewById<LinearLayout>(Resource.Id.LayoutDefaultWallpaper);

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconGallery, IonIconsFonts.Images);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconColor, IonIconsFonts.ColorPalette);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconDefaultWallpaper, IonIconsFonts.Aperture);
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
                    toolbar.Title = GetText(Resource.String.Lbl_Wallpaper);
                    toolbar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetHomeAsUpIndicator(AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.ic_action_right_arrow_color : Resource.Drawable.ic_action_left_arrow_color));
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
                    TxtSave.Click += TxtSaveOnClick;
                    CardViewGallery.Click += CardViewGalleryOnClick;
                    CardViewColor.Click += CardViewColorOnClick;
                    LayoutDefaultWallpaper.Click += LayoutDefaultWallpaperOnClick;
                }
                else
                {
                    TxtSave.Click -= TxtSaveOnClick;
                    CardViewGallery.Click -= CardViewGalleryOnClick;
                    CardViewColor.Click -= CardViewColorOnClick;
                    LayoutDefaultWallpaper.Click -= LayoutDefaultWallpaperOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Events

        private void TxtSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                var resultIntent = new Intent();
                switch (EventSave)
                {
                    case "DefaultWallpaper":
                        MainSettings.SharedData?.Edit()?.PutString("Wallpaper_key", string.Empty)?.Commit();
                        SetResult(Result.Ok, resultIntent);
                        break;
                    case "Image":
                        MainSettings.SharedData?.Edit()?.PutString("Wallpaper_key", ResultUri.Path)?.Commit();
                        SetResult(Result.Ok, resultIntent);
                        break;
                    case "Color":
                        MainSettings.SharedData?.Edit()?.PutString("Wallpaper_key", HexColor)?.Commit();
                        SetResult(Result.Ok, resultIntent);
                        break;
                    default:
                        SetResult(Result.Canceled, resultIntent);
                        break;
                }

                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LayoutDefaultWallpaperOnClick(object sender, EventArgs e)
        {
            try
            {
                Glide.With(this).Load(Resource.Drawable.ImagePlacholder).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(ImageView);
                EventSave = "DefaultWallpaper";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CardViewColorOnClick(object sender, EventArgs e)
        {
            try
            {
                ColorPickerDialog.NewBuilder()
                    .SetDialogType(ColorPickerDialog.TypeCustom)
                    .SetAllowPresets(false)
                    .SetDialogId(0)
                    .SetColor(Color.ParseColor(AppSettings.MainColor))
                    .SetShowAlphaSlider(true)
                    .Show(this);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CardViewGalleryOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                        ResultUri = result.Uri;

                        if (!string.IsNullOrEmpty(ResultUri.Path))
                        {
                            Glide.With(this).Load(ResultUri).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(ImageView);
                            EventSave = "Image";
                        }
                        else
                        {
                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
                        }
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long);
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
                        OpenDialogGallery();
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

        private void GetWallpaper()
        {
            try
            {
                string path = MainSettings.SharedData?.GetString("Wallpaper_key", string.Empty);
                if (!string.IsNullOrEmpty(path))
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(path);
                    if (type == "Image")
                        Glide.With(this).Load(path).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(ImageView);
                    else if (path.Contains("#"))
                        Glide.With(this).Load(new ColorDrawable(Color.ParseColor(path))).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(ImageView);
                }
                else
                {
                    Glide.With(this).Load(Resource.Drawable.Grey_Offline).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(ImageView);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenDialogGallery()
        {
            try
            {
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

        public void OnColorSelected(int dialogId, int color)
        {
            try
            {
                HexColor = "#" + Integer.ToHexString(color);
                Glide.With(this).Load(new ColorDrawable(Color.ParseColor(HexColor))).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(ImageView);

                EventSave = "Color";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnDialogDismissed(int dialogId)
        {

        }
    }
}