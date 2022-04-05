using System;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using WoWonder.Activities.Base;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.SettingsPreferences.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class DeleteAccountActivity : BaseActivity
    {
        #region Variables Basic

        private TextView IconPassword;
        private EditText TxtPassword;
        private CheckBox ChkDelete;
        private AppCompatButton BtnDelete;

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
                SetContentView(Resource.Layout.SettingsDeleteAccountLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdsGoogle.Ad_AdMobNative(this);
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
                IconPassword = FindViewById<TextView>(Resource.Id.IconPassword);
                TxtPassword = FindViewById<EditText>(Resource.Id.PasswordEditText);

                ChkDelete = FindViewById<CheckBox>(Resource.Id.DeleteCheckBox);
                BtnDelete = FindViewById<AppCompatButton>(Resource.Id.DeleteButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPassword, FontAwesomeIcon.Key);

                Methods.SetColorEditText(TxtPassword, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                ChkDelete.Text = GetText(Resource.String.Lbl_IWantToDelete1) + " " + UserDetails.Username + " " +
                                 GetText(Resource.String.Lbl_IWantToDelete2) + " " + AppSettings.ApplicationName + " " +
                                 GetText(Resource.String.Lbl_IWantToDelete3);
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
                    toolbar.Title = GetText(Resource.String.Lbl_DeleteAccount);
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
                    BtnDelete.Click += BtnDeleteOnClick;
                }
                else
                {
                    BtnDelete.Click -= BtnDeleteOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Event Delete
        private void BtnDeleteOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!ChkDelete.Checked)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_You_can_not_access_your_disapproval), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
                    return;
                }

                var data = ListUtils.DataUserLoginList.FirstOrDefault();
                if (data != null)
                {
                    if (TxtPassword.Text == data.Password)
                    {
                        ApiRequest.Delete(this);
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_Your_account_was_successfully_deleted), ToastLength.Long);
                    }
                    else
                    {
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_confirm_your_password), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion 
    }
}