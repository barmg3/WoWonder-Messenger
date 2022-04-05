using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using WoWonder.Activities.Base;
using WoWonder.Activities.Tab;
using WoWonder.Activities.WalkTroutPage;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WoWonder.Activities.Authentication
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class VerificationCodeActivity : BaseActivity
    {
        #region Variables Basic

        private EditText TxtNumber1;
        private AppCompatButton BtnVerify;
        private string TypeCode;

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
                SetContentView(Resource.Layout.VerificationCodeLayout);

                TypeCode = Intent?.GetStringExtra("TypeCode") ?? "";

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
                TxtNumber1 = FindViewById<EditText>(Resource.Id.TextNumber1);
                BtnVerify = FindViewById<AppCompatButton>(Resource.Id.verifyButton);

                Methods.SetColorEditText(TxtNumber1, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                    toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
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
                    BtnVerify.Click += BtnVerifyOnClick;
                }
                else
                {
                    BtnVerify.Click -= BtnVerifyOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private async void BtnVerifyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TxtNumber1.Text) && !string.IsNullOrWhiteSpace(TxtNumber1.Text))
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        switch (TypeCode)
                        {
                            case "TwoFactor":
                            {
                                var(apiStatus, respond) = await RequestsAsync.Auth.TwoFactorAsync(UserDetails.UserId, TxtNumber1.Text, UserDetails.DeviceId);
                                if (apiStatus == 200)
                                {
                                    if (respond is AuthObject auth)
                                    {
                                        AndHUD.Shared.Dismiss(this);
                                        SetDataLogin(auth);

                                        if (AppSettings.ShowWalkTroutPage)
                                        {
                                            Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                                            newIntent.PutExtra("class", "login");
                                            StartActivity(newIntent);
                                        }
                                        else
                                        {
                                            StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                        }

                                    
                                        FinishAffinity();
                                    }
                                }
                                else
                                {
                                    if (respond is ErrorObject errorMessage)
                                    {
                                        var errorId = errorMessage.Error.ErrorId;
                                        if (errorId == "3")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CodeNotCorrect), GetText(Resource.String.Lbl_Ok));
                                    }
                                    Methods.DisplayReportResult(this, respond);
                                }

                                break;
                            }
                            case "AccountSms":
                            {
                                var(apiStatus, respond) = await RequestsAsync.Auth.ActiveAccountSmsAsync(UserDetails.UserId, TxtNumber1.Text, UserDetails.DeviceId);
                                if (apiStatus == 200)
                                {
                                    if (respond is AuthObject auth)
                                    {
                                        AndHUD.Shared.Dismiss(this);
                                        SetDataLogin(auth);

                                        if (AppSettings.ShowWalkTroutPage)
                                        {
                                            Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                                            newIntent.PutExtra("class", "login");
                                            StartActivity(newIntent);
                                        }
                                        else
                                        {
                                            StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                        }
 
                                        FinishAffinity();
                                    }
                                }
                                else
                                {
                                    if (respond is ErrorObject errorMessage)
                                    {
                                        var errorId = errorMessage.Error.ErrorId;
                                        if (errorId == "3")
                                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CodeNotCorrect), GetText(Resource.String.Lbl_Ok));
                                    }
                                    Methods.DisplayReportResult(this, respond);
                                }

                                break;
                            }
                        }

                        AndHUD.Shared.Dismiss(this);
                    }
                    else
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short);
                    }
                }
                else
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion

        private void SetDataLogin(AuthObject auth)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Username,
                    Password = UserDetails.Password,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                    Email = UserDetails.Email,
                };
                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);
                

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.Get_MyProfileData_Api });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}