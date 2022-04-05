using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
using WoWonder.Activities.Authentication;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.SettingsPreferences.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PasswordActivity : BaseActivity
    {
        #region Variables Basic

        private EditText TxtCurrentPassword, TxtNewPassword, TxtRepeatPassword;
        private TextView TxtSave, IconCurrentPassword, IconNewPassword, IconRepeatPassword, TxtLinkForget;

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
                SetContentView(Resource.Layout.SettingsPasswordLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
                base.OnPause();
                AddOrRemoveEvent(false);
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
                TxtSave = FindViewById<TextView>(Resource.Id.toolbar_title);

                IconCurrentPassword = FindViewById<TextView>(Resource.Id.IconCurrentPassword);
                TxtCurrentPassword = FindViewById<EditText>(Resource.Id.CurrentPasswordEditText);

                IconNewPassword = FindViewById<TextView>(Resource.Id.IconNewPassword);
                TxtNewPassword = FindViewById<EditText>(Resource.Id.NewPasswordEditText);

                IconRepeatPassword = (TextView)FindViewById(Resource.Id.IconRepeatPassword);
                TxtRepeatPassword = (EditText)FindViewById(Resource.Id.RepeatPasswordEditText);

                TxtLinkForget = FindViewById<TextView>(Resource.Id.linkText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconCurrentPassword, FontAwesomeIcon.Key);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconNewPassword, FontAwesomeIcon.UnlockAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconRepeatPassword, FontAwesomeIcon.LockAlt);

                Methods.SetColorEditText(TxtCurrentPassword, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtNewPassword, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtRepeatPassword, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                AdsGoogle.Ad_AdMobNative(this);
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
                    toolbar.Title = GetText(Resource.String.Lbl_Change_Password);
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
                    TxtLinkForget.Click += TxtLinkForget_OnClick;
                    TxtSave.Click += SaveData_OnClick;
                }
                else
                {
                    TxtLinkForget.Click -= TxtLinkForget_OnClick;
                    TxtSave.Click -= SaveData_OnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void TxtLinkForget_OnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(typeof(ForgetPasswordActivity));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save data 
        private async void SaveData_OnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
                    return;
                }

                if (TxtCurrentPassword.Text == "" || TxtNewPassword.Text == "" || TxtRepeatPassword.Text == "")
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long);
                    return;
                }

                if (TxtNewPassword.Text != TxtRepeatPassword.Text)
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Your_password_dont_match), ToastLength.Long);
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
               
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();

                if (TxtCurrentPassword.Text != null && TxtNewPassword.Text != null && TxtRepeatPassword.Text != null)
                {
                    var dataPrivacy = new Dictionary<string, string>
                    {
                        {"new_password", TxtNewPassword.Text},
                        {"current_password", TxtCurrentPassword.Text},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Global.UpdateUserDataAsync(dataPrivacy);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            if (result.Message.Contains("updated"))
                            {
                                UserDetails.Password = TxtNewPassword.Text;

                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_YourDetailsWasUpdated), ToastLength.Short);
                                AndHUD.Shared.Dismiss(this);
                            }
                            else
                            {
                                //Show a Error image with a message
                                AndHUD.Shared.ShowError(this, result.Message, MaskType.Clear, TimeSpan.FromSeconds(2));
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
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_Please_check_your_details), ToastLength.Long);
                } 
            }
            catch (Exception e)
            {             
                //Show a Error image with a message
                AndHUD.Shared.Dismiss(this); 
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}